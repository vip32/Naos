namespace Naos.Foundation.Domain.EventSourcing
{
    public class EventResult
    {
        public EventResult(long nextExpectedVersion)
        {
            this.NextExpectedVersion = nextExpectedVersion;
        }

        public long NextExpectedVersion { get; }
    }
}
