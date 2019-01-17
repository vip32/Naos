namespace Naos.Core.Messaging
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;

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
            // in case of scoping (singleton needs scoped lifetime) issues
            //using (var scope = this.serviceProvider.CreateScope())
            //{
            //    var scopedProvider = scope.ServiceProvider;
            //    return scopedProvider.GetService(messageHandlerType);
            //}

            return ActivatorUtilities.CreateInstance(this.serviceProvider, messageHandlerType);
        }
    }
}
