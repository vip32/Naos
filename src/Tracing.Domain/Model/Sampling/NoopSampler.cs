namespace Naos.Core.Tracing.Domain
{
    public class NoopSampler : ISampler
    {
        public void SetSampled(ISpan span)
        {
            if (span != null)
            {
                span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "noop");
                span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, 0.0);
                span.SetSampled(false);
            }
        }
    }
}