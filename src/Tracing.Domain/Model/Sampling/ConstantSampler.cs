namespace Naos.Tracing.Domain
{
    public class ConstantSampler : ISampler
    {
        public void SetSampled(ISpan span)
        {
            if (span != null)
            {
                span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "constant");
                span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, 1.0);
                span.SetSampled(true);
            }
        }
    }
}