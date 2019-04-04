namespace Naos.Core.Commands.Domain
{
    using Common;

    public class CommandResponse<TResponse>
    {
        public CommandResponse(string cancelledReason = null)
        {
            if(!cancelledReason.IsNullOrEmpty())
            {
                this.Cancelled = true;
                this.CancelledReason = cancelledReason;
            }
        }

        public TResponse Result { get; set; }

        public bool Cancelled { get; }

        public string CancelledReason { get; }
    }
}
