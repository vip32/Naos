namespace Naos.Commands.App
{
    using Naos.Foundation;

    public class CommandBehaviorResult
    {
        public bool Cancelled { get; private set; }

        public string CancelledReason { get; private set; }

        public void SetCancelled(string cancelledReason)
        {
            if (!cancelledReason.IsNullOrEmpty())
            {
                this.Cancelled = true;
                this.CancelledReason = cancelledReason;
            }
        }
    }
}
