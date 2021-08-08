using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfLogcat.Data;

namespace WpfLogcat
{
    public partial class MainWindow : Window
    {
        public MainWindowData MainData { get; set; }

        LogCat _logCat;  //meow

        bool _hackFilterRecursion = false;

        public MainWindow()
        {
            MainData = new MainWindowData();
            DataContext = MainData;

            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            CloseLogcat();
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string debug = (string)menuItem.Tag;
        }

        private void CheckBoxEnable_Checked(object sender, RoutedEventArgs e)
        {
            CloseLogcat();  //just in case

            _logCat = new LogCat();
            _logCat.OnLogReceived += LogCat_OnLogReceived;
            _logCat.Start();
        }

        private void CheckBoxEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            CloseLogcat();
        }

        void CloseLogcat()
        {
            if (_logCat != null)
            {
                _logCat.OnLogReceived -= LogCat_OnLogReceived;
                _logCat.Kill();
                _logCat = null;
            }
        }

        private async void LogCat_OnLogReceived(object sender, LogEventArgs e)
        {
            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(e.LogEntry.Tag) && !MainData.TagFilters.Any(x => x.Tag.Equals(e.LogEntry.Tag, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            MainData.TagFilters.Add(new TagFilter { Tag = e.LogEntry.Tag, IsChecked = true });
                            MainData.TagFilters = new ObservableCollection<TagFilter>(MainData.TagFilters.OrderBy(x => x.Tag));
                        }
                        
                        MainData.pendingLogs.Add(e.LogEntry);
                        
                        if (!IsFiltered(e.LogEntry))
                        {
                            MainData.LogItems.Add(e.LogEntry);
                            if(MainData.AutoScroll)
                                ListLogEntries.ScrollIntoView(e.LogEntry);
                        }

                        SetStats();
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
                });
            }
        }

        private void SetStats()
        {
            MainData.Stats = $"{MainData.LogItems.Count} / {MainData.pendingLogs.Count}";
        }

        private void CheckBoxFilter_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_hackFilterRecursion)
            {
                _hackFilterRecursion = true;
                CheckBox qq = (CheckBox)e.Source;
                TextBlock textBlock = (TextBlock)((System.Windows.Controls.Panel)qq.Parent).Children[1];
                if (textBlock.Text == MainWindowData.SpeialAllFlag)
                {
                    foreach (var filter in MainData.TagFilters)
                        filter.IsChecked = qq.IsChecked.Value;  // this will cause recursion, so use HackSettingFiltersBlock
                }
                FilterLogs();
                _hackFilterRecursion = false;
            }
        }

        private void FilterLogs()
        {
            var filtered = MainData.pendingLogs.Where(x => !IsFiltered(x));
            MainData.LogItems = new ObservableCollection<LogEntry>(filtered);
            SetStats();
        }

        private bool IsFiltered(LogEntry logEntry)
        {
            bool retval;

            if (logEntry.Tag != null)
                retval = MainData.TagFilters
                    .Any(x => x.IsChecked && x.Tag.Equals(logEntry.Tag, StringComparison.InvariantCultureIgnoreCase));
            else
                retval = !MainData.TagFilters.Any(x => !x.IsChecked);

            if (retval)
            {
                retval = string.IsNullOrEmpty(MainData.SearchFilter) ||
                    logEntry.Text.Contains(MainData.SearchFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    logEntry.Tag != null && logEntry.Tag.Contains(MainData.SearchFilter, StringComparison.CurrentCultureIgnoreCase);
            }
            return !retval;
        }

        private void TextBox_SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox cc = (TextBox)sender;
            MainData.SearchFilter = cc.Text;
            FilterLogs();
        }

        private void CommandCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = ListLogEntries.SelectedItems;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (LogEntry logEntry in selected)
                stringBuilder.AppendLine(logEntry.ToString());
            Clipboard.SetText(stringBuilder.ToString());
        }
    }
}
