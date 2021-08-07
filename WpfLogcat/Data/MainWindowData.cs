using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfLogcat.Data
{
    public class MainWindowData : INotifyPropertyChanged
    {
        public const string SpeialAllFlag = "<All>";
        private ObservableCollection<LogEntry> logItems = new ObservableCollection<LogEntry>();
        private ObservableCollection<TagFilter> tagFilters = new ObservableCollection<TagFilter>() { { new TagFilter { Tag = SpeialAllFlag, IsChecked = true } } };
        private string searchFilter;

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
    }
}
