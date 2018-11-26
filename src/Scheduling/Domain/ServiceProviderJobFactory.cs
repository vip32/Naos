namespace Naos.Core.Scheduling.Domain
{
    using System;
    using EnsureThat;

    public class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderJobFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <returns></returns>
        public IJob Create(Type jobType)
        {
            return this.serviceProvider.GetService(jobType) as IJob;
        }
    }
}
