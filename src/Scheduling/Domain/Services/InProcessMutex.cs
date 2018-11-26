namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Collections.Generic;

    public class InProcessMutex : IMutex
    {
        private readonly object @lock = new object();
        private readonly DateTime moment;
        private readonly Dictionary<string, MutexItem> items = new Dictionary<string, MutexItem>();

        public InProcessMutex()
        {
            this.moment = DateTime.UtcNow;
        }

        public InProcessMutex(DateTime moment)
        {
            this.moment = moment;
        }

        public void ReleaseLock(string key)
        {
            lock (this.@lock)
            {
                if (this.items.TryGetValue(key, out var item))
                {
                    item.Locked = false;
                    item.ExpireDate = null;
                }
            }
        }

        public bool TryAcquireLock(string key, int timeoutMinutes = 1440)
        {
            lock (this.@lock)
            {
                if (this.items.TryGetValue(key, out var item))
                {
                    if (item.Locked)
                    {
                        if (this.moment >= item.ExpireDate)
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
            if (this.items.TryGetValue(key, out var item))
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

            return true;
        }

        private class MutexItem
        {
            public bool Locked { get; set; }

            public DateTime? ExpireDate { get; set; }
        }
    }
}