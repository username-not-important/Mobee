using System;

namespace Mobee.Client.WPF.Logs
{
    public class Logger
    {
        #region Singleton

        private static Logger? _instance;
        private static object _lock = true;

        public static Logger Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ?? (_instance = new Logger());
                }
            }
        }

        #endregion

        public static string CH_APP = "app";
        public static string CH_PLAYER = "player";
        public static string CH_UI = "ui";
        public static string CH_COMMS = "comms";
        public static string CH_PERFORMANCE = "performance";

        public event LogEventHandler? LogInProgress;
        public ILogger _logger { get; set; }

        private Logger()
        {
            _logger = new FileLogger();
        }

        private void onLog(string time, string e, string ch)
        {
            LogInProgress?.Invoke(this, time, e, ch);
        }

        public void Log(string feedback, string channel, bool verbose=false)
        {
            if (verbose && ! Properties.Settings.Default.LOG_VERBOSE)
                return;
            
            try
            {
                string time = DateTime.Now.ToString("G").PadRight(40);
                _logger.Log($"{time}{feedback}", channel);

                onLog(time, feedback, channel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        public void Log(string feedback, bool verbose = false)
        {
            Log(feedback, CH_APP, verbose);
        }
    }
}