using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfLogcat.Adb;
using WpfLogcat.Data;

namespace WpfLogcat
{
    public partial class MainWindow : Window
    {
        public MainWindowData MainData { get; set; }

        LogCat _logCat;  //meow

        bool _hackFilterRecursion = false;
        Timer _pollForDevicesTimer = new Timer(5000);


        public MainWindow()
        {
            MainData = new MainWindowData();
            DataContext = MainData;

            InitializeComponent();

            _pollForDevicesTimer.AutoReset = false;
            _pollForDevicesTimer.Elapsed += PollForDevices_Elapsed;
            PollForDevices_Elapsed(null, null);
        }

        private void PollForDevices_Elapsed(object sender, ElapsedEventArgs e)
        {
            var devices = AdbDevices.GetDevicesDetailed();
            Dispatcher.BeginInvoke((Action) (() =>
            {
                //merge the adb results with our combobox and select the "saved" device if its in the list
                foreach (var dev in devices)
                {
                    if (!MainData.DeviceList.Any(x => x.DeviceId == dev.DeviceId))
                    {
                        var x = AdbDevices.GetEmulatorName(dev.DeviceId);
                        if(!string.IsNullOrEmpty(x))
                            dev.DeviceName = x;
                        else
                            dev.DeviceName = dev.Model;
                        dev.Enabled = true;
                        MainData.DeviceList.Add(dev);
                    }
                    else
                    {
                        var existing = MainData.DeviceList.Single(x => x.DeviceId == dev.DeviceId);
                        //update it or something?
                    }
                }

                foreach (var dev in MainData.DeviceList)
                {
                    if (!devices.Any(x => x.DeviceId == dev.DeviceId))
                        dev.Enabled = false;
                }

                if (MainData.SelectedDevice == null && !string.IsNullOrEmpty(Properties.Settings.Default.LastDeviceId))
                {
                    var savedDevice = MainData.DeviceList.FirstOrDefault(x => x.DeviceId == Properties.Settings.Default.LastDeviceId);
                    if (savedDevice != null)
                        MainData.SelectedDevice = savedDevice;
                }
                _pollForDevicesTimer.Start();
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            CloseLogcat();
            
            if(MainData.SelectedDevice != null)
                Properties.Settings.Default.LastDeviceId = MainData.SelectedDevice.DeviceId;

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
            _logCat.Start(MainData.SelectedDevice.DeviceId);
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
                        LogEntry lastGuy = null;
                        foreach (LogEntry logEntry in e.LogEntries)
                        {
                            if (!string.IsNullOrEmpty(logEntry.Tag) && !MainData.TagFilters.Any(x => x.Tag.Equals(logEntry.Tag, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                MainData.TagFilters.Add(new TagFilter { Tag = logEntry.Tag, IsChecked = true });
                                MainData.TagFilters = new ObservableCollection<TagFilter>(MainData.TagFilters.OrderBy(x => x.Tag));
                            }

                            MainData.pendingLogs.Add(logEntry);

                            if (!IsFiltered(logEntry))
                            {
                                MainData.LogItems.Add(logEntry);
                                lastGuy = logEntry;
                            }
                        }
                        if (MainData.AutoScroll && lastGuy!= null)
                            ListLogEntries.ScrollIntoView(lastGuy);
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
                if (textBlock.Text == MainWindowData.SpecialAllFlag)
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
            try
            {
                var filtered = MainData.pendingLogs.Where(x => !IsFiltered(x));
                MainData.LogItems = new ObservableCollection<LogEntry>(filtered);
                SetStats();
            }
            catch (Exception exc)
            {   //fall down go boom now what?
            }
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
                    logEntry.Text != null && logEntry.Text.Contains(MainData.SearchFilter, StringComparison.CurrentCultureIgnoreCase) ||
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

        private void ButtonClearGui_Click(object sender, RoutedEventArgs e)
        {
            MainData.LogItems.Clear();
        }
    }
}
