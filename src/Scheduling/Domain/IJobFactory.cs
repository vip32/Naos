namespace Naos.Core.Scheduling.Domain
{
    using System;

    public interface IJobFactory
    {
        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <returns></returns>
        IJob Create(Type jobType);
    }
}