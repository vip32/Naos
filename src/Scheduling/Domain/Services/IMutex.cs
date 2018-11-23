namespace Naos.Core.Scheduling.Domain
{
    public interface IMutex
    {
        void ReleaseLock(string key);

        bool TryAcquireLock(string key, int timeoutMinutes = 1440);
    }
}