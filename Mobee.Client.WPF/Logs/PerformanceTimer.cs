using System;
using System.Diagnostics;

namespace Mobee.Client.WPF.Logs
{
    public sealed class PerformanceTimer : IDisposable
    {
        private readonly string _message;
        private readonly bool _isVerbose;
        private readonly Stopwatch _stopwatch;

        private bool _disposed;

        public PerformanceTimer(string message, bool isVerbose = false)
        {
            _message = message;
            _stopwatch = Stopwatch.StartNew();
            _isVerbose = isVerbose;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Logger.Instance.Log($"{_message}: {_stopwatch.ElapsedMilliseconds}ms", Logger.CH_PERFORMANCE, _isVerbose);

            _disposed = true;
        }
    }
}
