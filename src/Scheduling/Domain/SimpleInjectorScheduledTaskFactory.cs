namespace Naos.Core.Scheduling.Domain
{
    using System;
    using EnsureThat;
    using SimpleInjector;

    public class SimpleInjectorScheduledTaskFactory : IScheduledTaskFactory
    {
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorScheduledTaskFactory"/> class.
        /// </summary>
        /// <param name="container">The service provider.</param>
        public SimpleInjectorScheduledTaskFactory(Container container)
        {
            EnsureArg.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="scheduledTaskType">Type of the scheduled task.</param>
        /// <returns></returns>
        public IScheduledTask Create(Type scheduledTaskType)
        {
            return this.container.GetInstance(scheduledTaskType) as IScheduledTask;
        }
    }
}
