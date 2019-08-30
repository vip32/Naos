namespace Naos.Core.Commands.App
{
    using Naos.Foundation;

    public class CommandBehaviorResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBehaviorResult"/> class.
        /// </summary>
        /// <param name="cancelledReason">The cancelled reason.</param>
        public CommandBehaviorResult(string cancelledReason = null)
        {
            if (!cancelledReason.IsNullOrEmpty())
            {
                this.Cancelled = true;
                this.CancelledReason = cancelledReason;
            }
        }

        public bool Cancelled { get; }

        public string CancelledReason { get; }
    }
}
