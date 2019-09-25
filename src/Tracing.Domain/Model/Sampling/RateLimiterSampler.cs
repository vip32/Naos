namespace Naos.Tracing.Domain
{
    using EnsureThat;
    using Naos.Foundation;

    /// <summary>
    /// A sampler which only samples spans that match the patterns
    /// </summary>
    public class RateLimiterSampler : ISampler
    {
        private readonly RateLimiter rateLimiter;

        public RateLimiterSampler(RateLimiter rateLimiter)
        {
            EnsureArg.IsNotNull(rateLimiter, nameof(rateLimiter));

            this.rateLimiter = rateLimiter;
        }

        public void SetSampled(ISpan span)
        {
            if (span != null)
            {
                if (this.rateLimiter.CheckCredit(1.0))
                {
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "ratelimiter");
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, 1.0);
                    span.SetSampled(true);
                }
                else
                {
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "ratelimiter");
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, this.rateLimiter.Balance);
                    span.SetSampled(false);
                }
            }
        }
    }
}