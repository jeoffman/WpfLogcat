using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfLogcat.Data;

namespace WpfLogcat
{
    public partial class MainWindow : Window
    {
        public MainWindowData MainData { get; set; }

        LogCat logCat;  //meow

        bool HackFilterRecursion = false;

        public MainWindow()
        {
            MainData = new MainWindowData();
            DataContext = MainData;

            InitializeComponent();

            //MainData.LogItems.Add(new LogEntry { App="app", Level = "E", PID = "57", Time="12345", });
            //MainData.LogItems.Add(new LogEntry { App = "app", Level = "W", PID = "57", Time = "12345" });
            //MainData.LogItems.Add(new LogEntry { App = "app", Level = "I", PID = "57", Time = "12345" });
            //MainData.LogItems.Add(new LogEntry { App = "app", Level = "E", PID = "57", Time = "12345" });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (logCat != null)
            {
                logCat.Kill();
                logCat.OnLogReceived -= LogCat_OnLogReceived;
            }
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
            logCat = new LogCat();
            logCat.OnLogReceived += LogCat_OnLogReceived;
            logCat.Start();
        }

        private void LogCat_OnLogReceived(object sender, LogEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(e.LogEntry.Tag) && !MainData.TagFilters.Any(x => x.Tag.Equals(e.LogEntry.Tag, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            MainData.TagFilters.Add(new TagFilter { Tag = e.LogEntry.Tag, IsChecked = true });
                        }
                        MainData.pendingLogs.Add(e.LogEntry);
                        if(!IsFiltered(e.LogEntry))
                            MainData.LogItems.Add(e.LogEntry);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
                });
            }
        }

        private void CheckBoxFilter_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            if (!HackFilterRecursion)
            {
                HackFilterRecursion = true;
                CheckBox qq = (CheckBox)e.Source;
                TextBlock textBlock = (TextBlock)((System.Windows.Controls.Panel)qq.Parent).Children[1];
                if (textBlock.Text == MainWindowData.SpeialAllFlag)
                {
                    foreach (var filter in MainData.TagFilters)
                        filter.IsChecked = qq.IsChecked.Value;  // this will cause recursion, so use HackSettingFiltersBlock
                }
                FilterLogs();
                HackFilterRecursion = false;
            }
        }

        private void FilterLogs()
        {
            var filtered = MainData.pendingLogs.Where(x => !IsFiltered(x));
            MainData.LogItems = new ObservableCollection<LogEntry>(filtered);
        }

        private bool IsFiltered(LogEntry x)
        {
            bool retval =
            MainData.TagFilters
                .Any(y => y.IsChecked && y.Tag.Equals(x.Tag, StringComparison.InvariantCultureIgnoreCase)) &&
                    (string.IsNullOrEmpty(MainData.SearchFilter) ||
                        x.Text.Contains(MainData.SearchFilter, StringComparison.CurrentCultureIgnoreCase) ||
                        x.Tag.Contains(MainData.SearchFilter, StringComparison.CurrentCultureIgnoreCase));
            return !retval;
        }

        private void TextBox_SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox cc = (TextBox)sender;
            MainData.SearchFilter = cc.Text;
            FilterLogs();
        }
    }
}
