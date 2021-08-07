using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfLogcat.Data
{
    public class TagFilter : INotifyPropertyChanged
    {
        private bool _isChecked;
        private string _tag;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                if (value == this._isChecked)
                {
                    return;
                }

                this._isChecked = value;
                this.OnPropertyChanged();
            }
        }
        public string Tag
        {
            get => this._tag;
            set
            {
                if (value == this._tag)
                {
                    return;
                }

                this._tag = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
