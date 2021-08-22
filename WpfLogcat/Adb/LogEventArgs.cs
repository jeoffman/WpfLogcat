using System;
using System.Collections.Generic;
using WpfLogcat.Data;

namespace WpfLogcat.Adb
{
    public class LogEventArgs : EventArgs
    {
        public List<LogEntry> LogEntries { get; set; }
    }
}
