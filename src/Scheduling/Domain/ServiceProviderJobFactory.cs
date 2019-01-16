namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;

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

        public object Create(Type jobType)
        {
            return ActivatorUtilities.CreateInstance(this.serviceProvider, jobType);
            //return this.serviceProvider.GetService(jobType);
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <returns></returns>
        public IJob CreateJob(Type jobType)
        {
            return ActivatorUtilities.CreateInstance(this.serviceProvider, jobType) as IJob;
            //return this.serviceProvider.GetService(jobType) as IJob;
        }
    }
}
