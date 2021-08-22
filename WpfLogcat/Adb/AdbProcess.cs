using System.Diagnostics;
using System.Text;

namespace WpfLogcat.Adb
{
    class AdbProcess
    {
        /// <summary>Executes the adb.exe <command and parameters> and WAITS FOR A RESPONSE - the 
        /// return value is the complete response including CRLF etc.</summary>
        public static string ExecuteAdbCommandAndReturnResponse(string commandAndParameters)
        {
            StringBuilder retval = new StringBuilder();
            using (var Process = new Process())
            {
                Process.StartInfo.FileName = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                Process.StartInfo.CreateNoWindow = true;
                Process.StartInfo.Arguments = commandAndParameters;
                Process.StartInfo.RedirectStandardInput = true;
                Process.StartInfo.RedirectStandardOutput = true;
                Process.StartInfo.RedirectStandardError = true;
                Process.StartInfo.UseShellExecute = false;
                Process.Start();
                while (!Process.StandardOutput.EndOfStream)
                {
                    retval.AppendLine(Process.StandardOutput.ReadLine());
                }
            }
            return retval.ToString();
        }
    }
}
