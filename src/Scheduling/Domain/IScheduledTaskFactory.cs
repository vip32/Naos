namespace Naos.Core.Scheduling.Domain
{
    using System;

    public interface IScheduledTaskFactory
    {
        /// <summary>
        /// Creates the specified scheduled task type.
        /// </summary>
        /// <param name="scheduledTaskType">Type of the scheduled task.</param>
        /// <returns></returns>
        IScheduledTask Create(Type scheduledTaskType);
    }
}