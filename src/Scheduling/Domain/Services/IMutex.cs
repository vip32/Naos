namespace Naos.Core.Scheduling.Domain
{
    public interface IMutex
    {
        void ReleaseLock(string key);

        bool TryGetLock(string key, int timeoutMinutes);
    }
}