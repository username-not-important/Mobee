using System;
using System.IO;
using Mobee.Client.WPF.Properties;

namespace Mobee.Client.WPF.Logs
{
    public class FileLogger : ILogger
    {
        public string Source
        {
            get => Environment.CurrentDirectory + "\\" + Settings.Default.LOG_FILE;
            set
            {
                Settings.Default["LOG_FILE"] = value;
                Settings.Default.Save();
            }
        }

        public void RevertSource()
        {
            Settings.Default["LOG_FILE"] = Settings.Default.DEFAULT_LOG_FILE;
            Settings.Default.Save();
        }

        public void Log(string operation, string channel)
        {
            File.AppendAllText(string.Format(Source, channel), operation + "\r\n");
        }
    }
}