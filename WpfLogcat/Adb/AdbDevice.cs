using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfLogcat.Adb
{
    //adb devices -l
    //98893a445a35365337 device product:dreamqltesq model:SM_G950U device:dreamqltesq transport_id:2
    //emulator-5554          device product:sdk_phone_x86 model:Android_SDK_built_for_x86 device:generic_x86 transport_id:1

    //adb devices
    //98893a445a35365337 device
    //emulator-5554   device

    public class AdbDevice : INotifyPropertyChanged
    {
        private string _deviceId;
        private string _product;
        private string _model;
        private string _device;
        private int _transportId;
        private string _deviceName;
        private bool _enabled;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DeviceId
        {
            get => _deviceId;
            set
            {
                if (value == _deviceId)
                {
                    return;
                }

                _deviceId = value;
                OnPropertyChanged();
            }
        }
        public string Product
        {
            get => _product;
            set
            {
                if (value == _product)
                {
                    return;
                }

                _product = value;
                OnPropertyChanged();
            }
        }
        public string Model
        {
            get => _model;
            set
            {
                if (value == _model)
                {
                    return;
                }

                _model = value;
                OnPropertyChanged();
            }
        }
        public string Device
        {
            get => _device;
            set
            {
                if (value == _device)
                {
                    return;
                }

                _device = value;
                OnPropertyChanged();
            }
        }
        public int TransportId
        {
            get => _transportId;
            set
            {
                if (value == _transportId)
                {
                    return;
                }

                _transportId = value;
                OnPropertyChanged();
            }
        }
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (value == _deviceName)
                {
                    return;
                }

                _deviceName = value;
                OnPropertyChanged();
            }
        }  //the emulator name if we *think* its an emulator, otherwise the "model:" and good luck with that!

        public bool Enabled
        {
            get => _enabled;
            internal set
            {
                if (value == _enabled)
                {
                    return;
                }

                _enabled = value;
                OnPropertyChanged();
            }
        }
    }
}
