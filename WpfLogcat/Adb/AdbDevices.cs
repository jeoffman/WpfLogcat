using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WpfLogcat.Adb
{
    //adb devices -l
    //98893a445a35365337 device product:dreamqltesq model:SM_G950U device:dreamqltesq transport_id:2
    //emulator-5554          device product:sdk_phone_x86 model:Android_SDK_built_for_x86 device:generic_x86 transport_id:1

    //adb devices
    //98893a445a35365337 device
    //emulator-5554   device

    public class AdbDevices
    {
        public static IEnumerable<AdbDevice> GetDevicesDetailed()
        {
            List<AdbDevice> retval = new List<AdbDevice>();
            var devicesResponse = AdbProcess.ExecuteAdbCommandAndReturnResponse("devices -l");
            var deviceResponseLines = devicesResponse.Split(Environment.NewLine);
            for (int count = 1; count < deviceResponseLines.Length - 1; count++)    //start at 1 = skip first line, skip blank line at the end
            {
                var device = ParseDeviceDetails(deviceResponseLines[count]);
                if(device != null)
                    retval.Add(device);
            }
            return retval;
        }

        private static AdbDevice ParseDeviceDetails(string deviceDetailsLine)
        {
            AdbDevice retval = null;
            var firstSpace = deviceDetailsLine.IndexOf(' ');
            if (firstSpace > 0)
            {
                retval = new AdbDevice { DeviceId = deviceDetailsLine.Substring(0, firstSpace) };
                var theWordDevice = deviceDetailsLine.IndexOf(" device ", firstSpace);
                if (theWordDevice > 0)
                {
                    var productIndex = deviceDetailsLine.IndexOf("product:", theWordDevice + 8);    //+8 to skip " device "

                    var deets = deviceDetailsLine.Substring(productIndex).Split(' ');
                    foreach (var deet in deets)
                    {
                        var info = deet.Split(':');
                        if (info.Length == 2)
                        {
                            switch (info[0])
                            {
                                case "product":
                                    retval.Product = info[1];
                                    break;
                                case "model":
                                    retval.Model = info[1];
                                    break;
                                case "device":
                                    retval.Device = info[1];
                                    break;
                                case "transport_id":
                                    retval.TransportId = int.Parse(info[1]);
                                    break;
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Bad details \"{deet}\" missing colon (:) ?");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No word \"device\"??");
                }
            }
            else
            {
                Debug.WriteLine("No spaces?");
            }
            return retval;
        }

        public static string GetEmulatorName(string deviceId)
        {
            string retval = null;
            var splitted = deviceId.Split('-');
            if (splitted.Length == 2)
            {
                int port;
                if (int.TryParse(splitted[1], out port))
                {
                    TcpClient client = new TcpClient();
                    try
                    {
                        //connect and read the welcome garbage
                        client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                        NetworkStream stream = client.GetStream();
                        stream.ReadTimeout = 1000;  //probably don't do this on the GUI thread, OK?
                        byte[] buffer = new byte[1024 * 1024];
                        Thread.Sleep(10);   //let the emulator telnet catch up with our connection
                        while (stream.CanRead && stream.DataAvailable)
                        {
                            try
                            {
                                int len = stream.Read(buffer, 0, buffer.Length);    //some garbage about auth, skip it for now
                                var garbage = Encoding.UTF8.GetString(buffer, 0, len);
                            }
                            catch (IOException exc)
                            {
                                Debug.WriteLine($"GetEmulatorName IOException(1) \"{exc}\"");
                            }
                        }

                        byte[] command = System.Text.Encoding.ASCII.GetBytes("avd name" + Environment.NewLine);
                        stream.Write(command, 0, command.Length);
                        if (stream.CanRead)
                        {
                            try
                            {
                                int len = stream.Read(buffer, 0, buffer.Length);
                                var response = Encoding.UTF8.GetString(buffer, 0, len);
                                var split = response.Split(Environment.NewLine);
                                if (split.Length > 0)
                                    retval = split[0];
                            }
                            catch (IOException exc)
                            {
                                Debug.WriteLine($"GetEmulatorName IOException(2) \"{exc}\"");
                            }
                        }

                    }
                    catch (SocketException exc)
                    {
                        Debug.WriteLine($"GetEmulatorName SocketException \"{exc}\"");
                    }
                }
            }
            return retval;
        }
    }
}
