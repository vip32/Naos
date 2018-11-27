namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    public class Job : IJob
    {
        private readonly Func<string[], Task> task;
        private readonly Func<CancellationToken, string[], Task> task2;
        private readonly Action<string[]> action;
        private readonly Action action2;

        public Job(Action action)
        {
            EnsureArg.IsNotNull(action, nameof(action));

            this.action2 = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="cron">The cron expression.</param>
        /// <param name="action">The action.</param>
        public Job(Action<string[]> action)
        {
            EnsureArg.IsNotNull(action, nameof(action));

            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="cron">The cron expression.</param>
        /// <param name="task">The task.</param>
        public Job(Func<string[], Task> task)
        {
            EnsureArg.IsNotNull(task, nameof(task));

            this.task = task;
        }

        public Job(Func<CancellationToken, string[], Task> task)
        {
            EnsureArg.IsNotNull(task, nameof(task));

            this.task2 = task;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="cron">The cron.</param>
        protected Job()
        {
        }

        // TODO: Other things to schedule
        // Schedule - Command
        // Schedule - Message
        public virtual async Task ExecuteAsync(string[] args = null)
        {
            await this.ExecuteAsync(CancellationToken.None, args).ConfigureAwait(false);
        }

        public virtual async Task ExecuteAsync(CancellationToken cancellationToken, string[] args = null)
        {
            if(this.task != null)
            {
                await this.task(args).ConfigureAwait(false);
            }
            else if (this.task2 != null)
            {
                await this.task2(cancellationToken, args).ConfigureAwait(false);
            }
            else if (this.action2 != null)
            {
                this.action2();
            }
            else if (this.action != null)
            {
                this.action(args);
            }
        }

        // TODO: action/func/type containing the thing to invoke when due
    }
}
