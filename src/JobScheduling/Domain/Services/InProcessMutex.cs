namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class InProcessMutex : IMutex
    {
        private readonly ILogger<InProcessMutex> logger;
        private readonly DateTime moment;
        private readonly Dictionary<string, MutexItem> items = new Dictionary<string, MutexItem>();
        private readonly object @lock = new object();

        public InProcessMutex(ILoggerFactory loggerFactory)
            : this(loggerFactory, DateTime.UtcNow)
        {
        }

        public InProcessMutex(ILoggerFactory loggerFactory, DateTime moment)
        {
            //EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = loggerFactory.CreateLogger<InProcessMutex>();
            this.moment = moment;
        }

        public void ReleaseLock(string key)
        {
            lock(this.@lock)
            {
                if(this.items.TryGetValue(key, out var item))
                {
                    item.Locked = false;
                    item.ExpireDate = null;
                    this.logger?.LogDebug($"{{LogKey:l}} lock released (key={key})", LogKeys.JobScheduling);
                }
            }
        }

        public bool TryAcquireLock(string key, int timeoutMinutes = 1440)
        {
            lock(this.@lock)
            {
                if(this.items.TryGetValue(key, out var item))
                {
                    if(item.Locked)
                    {
                        if(this.moment >= item.ExpireDate)
                        {
                            return this.CreateLock(key, timeoutMinutes);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return this.CreateLock(key, timeoutMinutes);
                    }
                }
                else
                {
                    return this.CreateLock(key, timeoutMinutes);
                }
            }
        }

        private bool CreateLock(string key, int timeoutMinutes)
        {
            if(this.items.TryGetValue(key, out var item))
            {
                item.Locked = true;
                item.ExpireDate = this.moment.AddMinutes(timeoutMinutes);
            }
            else
            {
                this.items.Add(
                    key,
                    new MutexItem
                    {
                        Locked = true,
                        ExpireDate = this.moment.AddMinutes(timeoutMinutes)
                    });
            }

            this.logger?.LogDebug($"{{LogKey:l}} lock created (key={key}, timeout({new TimeSpan(0, timeoutMinutes, 0).ToString("c")}))", LogKeys.JobScheduling);

            return true;
        }

        private class MutexItem
        {
            public bool Locked { get; set; }

            public DateTime? ExpireDate { get; set; }
        }
    }
}