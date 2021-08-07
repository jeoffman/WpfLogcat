using System;

namespace WpfLogcat.Data
{
    public class LogEntry
    {
        public DateTime? DateTime { get; set; }
        public int? PID { get; set; }
        public int? TID { get; set; }
        public char? Level { get; set; }
        public string Tag { get; set; }
        public string Text { get; set; }
    }
}
