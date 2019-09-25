namespace Naos.Foundation
{
    using System;

    /// <summary>
    /// <see cref="RateLimiter"/> is a rate limiter based on leaky bucket algorithm, formulated in terms of a
    /// credits balance that is replenished every time <see cref="CheckCredit"/> method is called (tick) by the amount proportional
    /// to the time elapsed since the last tick, up to max of creditsPerSecond. A call to <see cref="CheckCredit"/> takes a cost
    /// of an item we want to pay with the balance. If the balance exceeds the cost of the item, the item is "purchased"
    /// and the balance reduced, indicated by returned value of true. Otherwise the balance is unchanged and return false.
    /// <para/>
    /// This can be used to limit a rate of messages emitted by a service by instantiating the Rate Limiter with the
    /// max number of messages a service is allowed to emit per second, and calling <c>CheckCredit(1.0)</c> for each message
    /// to determine if the message is within the rate limit.
    /// <para/>
    /// It can also be used to limit the rate of traffic in bytes, by setting creditsPerSecond to desired throughput
    /// as bytes/second, and calling <see cref="CheckCredit"/> with the actual message size.
    ///
    /// origin: https://github.com/jaegertracing/jaeger-client-csharp/blob/master/src/Jaeger/Util/RateLimiter.cs
    /// </summary>
    public class RateLimiter
    {
        private readonly double creditsPerMillisecond;
        private readonly double maxBalance;

        private long lastTick;

        public RateLimiter(double creditsPerSecond, double maxBalance)
            : this(creditsPerSecond, maxBalance, DateTime.UtcNow)
        {
        }

        public RateLimiter(double creditsPerSecond, double maxBalance, DateTime timestamp)
        {
            this.creditsPerMillisecond = creditsPerSecond / 1000;
            this.maxBalance = maxBalance;
            this.Balance = maxBalance;
            this.lastTick = timestamp.ToEpochMilliseconds();
        }

        public double Balance { get; private set; }

        public bool CheckCredit(double itemCost)
        {
            // calculate how much time passed since the last tick, and update current tick
            var currentTime = DateTime.UtcNow.ToEpochMilliseconds();
            var elapsedTime = currentTime - this.lastTick;
            this.lastTick = currentTime;

            // calculate how much credit have we accumulated since the last tick
            this.Balance += elapsedTime * this.creditsPerMillisecond;
            if (this.Balance > this.maxBalance)
            {
                this.Balance = this.maxBalance;
            }

            // if we have enough credits to pay for current item, then reduce balance and allow
            if (this.Balance >= itemCost)
            {
                this.Balance -= itemCost;
                return true;
            }

            return false;
        }
    }
}