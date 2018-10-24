namespace Naos.Core.Messaging
{
    using System;
    using EnsureThat;
    using SimpleInjector;

    public class SimpleInjectorMessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorMessageHandlerFactory"/> class.
        /// </summary>
        /// <param name="container">The service provider.</param>
        public SimpleInjectorMessageHandlerFactory(Container container)
        {
            EnsureArg.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>
        /// Creates the specified message handler type.
        /// </summary>
        /// <param name="messageHandlerType">Type of the message handler.</param>
        /// <returns></returns>
        public object Create(Type messageHandlerType)
        {
            return this.container.GetInstance(messageHandlerType);
        }
    }
}
