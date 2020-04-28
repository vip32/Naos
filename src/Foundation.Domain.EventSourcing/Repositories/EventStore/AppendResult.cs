namespace Naos.Foundation.Domain.EventSourcing
{
    public class AppendResult
    {
        public AppendResult(long nextExpectedVersion)
        {
            this.NextExpectedVersion = nextExpectedVersion;
        }

        public long NextExpectedVersion { get; }
    }
}
