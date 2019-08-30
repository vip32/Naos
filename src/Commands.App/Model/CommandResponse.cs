namespace Naos.Core.Commands.App
{
    using Naos.Foundation;

    public class CommandResponse<TResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResponse{TResponse}"/> class.
        /// </summary>
        /// <param name="cancelledReason">The optional cancelled reason.</param>
        public CommandResponse(string cancelledReason = null)
        {
            if (!cancelledReason.IsNullOrEmpty())
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
