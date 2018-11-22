namespace Naos.Core.Scheduling.Domain
{
    using System;
    using EnsureThat;

    public class ServiceProviderScheduledTaskFactory : IScheduledTaskFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderScheduledTaskFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ServiceProviderScheduledTaskFactory(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="scheduledTaskType">Type of the scheduled task.</param>
        /// <returns></returns>
        public IScheduledTask Create(Type scheduledTaskType)
        {
            return this.serviceProvider.GetService(scheduledTaskType) as IScheduledTask;
        }
    }
}
