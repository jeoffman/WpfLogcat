using System;

namespace WpfLogcat.Data
{
    public class LogEntry
    {
        public DateTime? DateTime { get; set; }
        public char? Level { get; set; }
        public int? PID { get; set; }
        public int? TID { get; set; }
        public string Tag { get; set; }
        public string Text { get; set; }

        /// <summary>What goes in the clipboard or file when you copy/save</summary>
        public override string ToString()
        {
            return $"{DateTime}\t{Level}\t{PID}\t{TID}\t{Tag}\t{Text}";
        }
    }
}
