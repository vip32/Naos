namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using EnsureThat;
    using Naos.Foundation;

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

        public T Create<T>()
            where T : class
        {
            return Factory.Create(typeof(T), this.serviceProvider) as T;
        }

        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        public IJob CreateJob(Type jobType)
        {
            return Factory.Create(jobType, this.serviceProvider) as IJob;
        }
    }
}
