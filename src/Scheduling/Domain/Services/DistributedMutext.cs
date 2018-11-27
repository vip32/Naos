namespace Naos.Core.Scheduling.Domain
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class DistributedMutext : IMutex
    {
        // TODO: use https://github.com/madelson/DistributedLock for sql based cross machine locking
        private readonly ILogger<DistributedMutext> logger;
        private readonly DateTime moment;

        public DistributedMutext(ILogger<DistributedMutext> logger)
            : this(logger, DateTime.UtcNow)
        {
        }

        public DistributedMutext(ILogger<DistributedMutext> logger, DateTime moment)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.moment = moment;
        }

        public void ReleaseLock(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryAcquireLock(string key, int timeoutMinutes = 1440)
        {
            throw new NotImplementedException();
        }
    }
}