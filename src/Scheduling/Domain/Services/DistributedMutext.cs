namespace Naos.Core.Scheduling.Domain
{
    public class DistributedMutext : IMutex
    {
        // TODO: use https://github.com/madelson/DistributedLock for sql based cross machine locking

        public void ReleaseLock(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool TryAcquireLock(string key, int timeoutMinutes = 1440)
        {
            throw new System.NotImplementedException();
        }
    }
}