namespace Naos.Core.Messaging
{
    using System;
    using EnsureThat;

    public class SubscriptionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDetails"/> class.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        private SubscriptionDetails(Type handlerType)
        {
            this.HandlerType = handlerType;
        }

        /// <summary>
        /// Gets the type of the handler.
        /// </summary>
        /// <value>
        /// The type of the handler.
        /// </value>
        public Type HandlerType { get; }

        /// <summary>
        /// Creates a <see cref="SubscriptionDetails"/> for specified handler type.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        public static SubscriptionDetails Create(Type handlerType)
        {
            EnsureArg.IsNotNull(handlerType, nameof(handlerType));

            return new SubscriptionDetails(handlerType);
        }
    }
}
