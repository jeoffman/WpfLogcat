#define BATCHY
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using WpfLogcat.Data;

namespace WpfLogcat
{
    public class LogCat //Meow
    {
        public Process Process { get; set; }
        public event EventHandler<LogEventArgs> OnLogReceived;

        Timer _timer;
        readonly List<LogEntry> _backlog = new List<LogEntry>();

        /// <summary>TODO: hard coded path, sorry</summary>
        public void Start()
        {
            try
            {
#if BATCHY  //this doesn't help as much as I hoped that it would...
                _timer = new Timer(100);
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();
#endif

                Process = new Process();
                Process.StartInfo.FileName = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                Process.StartInfo.CreateNoWindow = true;
                Process.StartInfo.Arguments = "logcat -v threadtime";
                Process.StartInfo.RedirectStandardInput = true;
                Process.StartInfo.RedirectStandardOutput = true;
                Process.StartInfo.RedirectStandardError = true;
                Process.StartInfo.UseShellExecute = false;
                Process.OutputDataReceived += Process_OutputDataReceived;
                Process.Start();
                Process.BeginOutputReadLine();
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
                throw;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<LogEntry> copy;
            lock (_backlog)
            {
                copy = new List<LogEntry>(_backlog);
                _backlog.Clear();
            }
            OnLogReceived?.Invoke(this, new LogEventArgs { LogEntries = copy });
        }


        internal void Kill()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= Timer_Elapsed;
                _timer?.Dispose();
            }
            Process?.Kill();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var log = ParseLog(e.Data);
            if (log != null)
            {
#if BATCHY
                lock (_backlog)
                {
                    _backlog.Add(log);
                }
#else
                OnLogReceived?.Invoke(this, new LogEventArgs { LogEntries = new List<LogEntry> { log } });
#endif
            }
        }

        public static LogEntry ParseLog(string rawString)
        {
            LogEntry retval = null;
            if (!string.IsNullOrEmpty(rawString))
            {
                retval = new LogEntry();

                //Example: (for counting Substring indexes)
                //          1         2         3         4         5         6         7         8
                //012345678901234567890123456789012345678901234567890123456789012345678901234567890
                //08-06 16:29:00.003  1926  1927 D KeyguardClockSwitch: Updating clock: 4î¸29

                if (rawString.Length > 18)
                {
                    DateTime maybeDateTime;  //eg: 08-06 16:29:00.003
                    if (DateTime.TryParseExact(rawString.Substring(0, 18), "MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out maybeDateTime))
                    {
                        retval.DateTime = maybeDateTime;

                        if (rawString.Length > 23)
                        {
                            int maybePID;
                            if (int.TryParse(rawString.Substring(20, 4), out maybePID)) //eg: 1926
                                retval.PID = maybePID;

                            if (rawString.Length > 29)
                            {
                                int maybeTID;
                                if (int.TryParse(rawString.Substring(26, 4), out maybeTID)) //eg: 1927
                                    retval.TID = maybeTID;

                                if (rawString.Length > 31)
                                {
                                    retval.Level = rawString[31];   //eg: D

                                    var theRemainder = rawString.Substring(33);
                                    int indexOfText = theRemainder.IndexOf(' ');
                                    if (indexOfText >= 0)
                                    {
                                        retval.Tag = theRemainder.Substring(0, indexOfText); //eg: KeyguardClockSwitch:
                                        if (theRemainder.Length > indexOfText + 1)
                                            retval.Text = theRemainder.Substring(indexOfText + 1).TrimStart(); //eg: Updating clock: 4î¸29
                                    }
                                    else
                                    {
                                        retval.Text = theRemainder;
                                        Debug.WriteLine("Missing App");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Missing Level");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Bad TID");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Bad PID");
                        }
                    }
                    else
                    {
                        retval.Text = rawString;    //bad date? - just raw text I guess
                    }
                }
                else
                {
                    retval.Text = rawString;
                }
                Debug.WriteLine(rawString);
            }
            return retval;
        }
    }

    public class LogEventArgs : EventArgs
    {
        public List<LogEntry> LogEntries { get; set; }
    }
}
