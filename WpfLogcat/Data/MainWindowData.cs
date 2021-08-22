using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfLogcat.Adb;

namespace WpfLogcat.Data
{
    public class MainWindowData : INotifyPropertyChanged
    {
        public const string SpecialAllFlag = "<All>";
        private ObservableCollection<LogEntry> logItems = new ObservableCollection<LogEntry>();
        private ObservableCollection<TagFilter> tagFilters = new ObservableCollection<TagFilter>() { { new TagFilter { Tag = SpecialAllFlag, IsChecked = true } } };
        private string searchFilter;
        private bool autoScroll = false;
        private string stats;
        private ObservableCollection<AdbDevice> deviceList = new ObservableCollection<AdbDevice>();
        private AdbDevice selectedDevice;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogEntry> LogItems
        {
            get => this.logItems;
            set
            {
                if (ReferenceEquals(value, this.logItems))
                {
                    return;
                }

                this.logItems = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<LogEntry> pendingLogs = new List<LogEntry>();

        public ObservableCollection<TagFilter> TagFilters
        {
            get => this.tagFilters;
            set
            {
                if (ReferenceEquals(value, this.tagFilters))
                {
                    return;
                }

                this.tagFilters = value;
                this.OnPropertyChanged();
            }
        }

        public string SearchFilter
        {
            get => this.searchFilter;
            set
            {
                if (value == this.searchFilter)
                {
                    return;
                }

                this.searchFilter = value;
                this.OnPropertyChanged();
            }
        }

        public bool AutoScroll
        {
            get => this.autoScroll;
            set
            {
                if (value == this.autoScroll)
                {
                    return;
                }

                this.autoScroll = value;
                this.OnPropertyChanged();
            }
        }

        public string Stats
        {
            get => this.stats;
            set
            {
                if (value == this.stats)
                {
                    return;
                }

                this.stats = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<AdbDevice> DeviceList
        {
            get => this.deviceList;
            set
            {
                if (ReferenceEquals(value, this.deviceList))
                {
                    return;
                }

                this.deviceList = value;
                this.OnPropertyChanged();
            }
        }

        public AdbDevice SelectedDevice
        {
            get => this.selectedDevice;
            set
            {
                if (ReferenceEquals(value, this.selectedDevice))
                {
                    return;
                }

                this.selectedDevice = value;
                this.OnPropertyChanged();
            }
        }
    }
}
