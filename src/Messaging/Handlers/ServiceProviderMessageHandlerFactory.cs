namespace Naos.Core.Messaging
{
    using System;
    using EnsureThat;
    using Naos.Core.Common;

    public class ServiceProviderMessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderMessageHandlerFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ServiceProviderMessageHandlerFactory(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the specified message handler type.
        /// </summary>
        /// <param name="messageHandlerType">Type of the message handler.</param>
        /// <returns></returns>
        public object Create(Type messageHandlerType)
        {
            return Factory.Create(messageHandlerType, this.serviceProvider);
        }
    }
}
