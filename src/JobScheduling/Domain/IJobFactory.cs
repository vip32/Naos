namespace Naos.Core.JobScheduling.Domain
{
    using System;

    public interface IJobFactory
    {
        /// <summary>
        /// Creates the specified scheduled job type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        IJob CreateJob(Type jobType);

        T Create<T>()
            where T : class;
    }
}