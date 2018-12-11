namespace Naos.Core.Scheduling.Domain
{
    using System;
    using EnsureThat;
    using SimpleInjector;

    public class SimpleInjectorJobFactory : IJobFactory
    {
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorJobFactory"/> class.
        /// </summary>
        /// <param name="container">The service provider.</param>
        public SimpleInjectorJobFactory(Container container)
        {
            EnsureArg.IsNotNull(container, nameof(container));

            this.container = container;
        }

        public object Create(Type jobType)
        {
            return this.container.GetInstance(jobType);
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <returns></returns>
        public IJob CreateJob(Type jobType)
        {
            return this.container.GetInstance(jobType) as IJob;
        }
    }
}
