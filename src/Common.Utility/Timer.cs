namespace Naos.Foundation
{
    using System;
    using System.Diagnostics;

    public class Timer : IDisposable
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public Timer()
        {
            this.stopwatch.Start();
        }

        /// <summary>
        /// Gets the total elapsed time measured by the current instance.
        /// </summary>
        public long ElapsedTicks => this.stopwatch.ElapsedTicks;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance.
        /// </summary>
        public long ElapsedMilliseconds => this.stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance.
        /// </summary>
        public TimeSpan Elapsed => this.stopwatch.Elapsed;

        /// <summary>
        /// Stops measuring elapsed time for an interval.
        /// </summary>
        public void Stop()
        {
            this.stopwatch.Stop();
        }

        /// <summary>
        ///  Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
        /// </summary>
        public void Restart()
        {
            this.stopwatch.Restart();
        }

        public void Dispose()
        {
            if(this.stopwatch.IsRunning)
            {
                this.stopwatch.Stop();
            }
        }
    }
}
