using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Mobee.Client.WPF.Connection
{
    public class MobeeReconnectPolicy : IRetryPolicy
    {
        private int _retryCount = 0;
        
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            _retryCount++;

            if (_retryCount < 3)
                return TimeSpan.FromSeconds(2);

            if (_retryCount < 10)
                return TimeSpan.FromSeconds(5);

            return TimeSpan.FromSeconds(10);
        }
    }
}
