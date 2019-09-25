namespace Naos.Tracing.Domain
{
    using System.Collections.Generic;
    using EnsureThat;
    using Naos.Foundation;

    /// <summary>
    /// A sampler which only samples spans that match the patterns
    /// </summary>
    public class OperationNamePatternSampler : ISampler
    {
        private readonly IEnumerable<string> operationNamePatterns;

        public OperationNamePatternSampler(IEnumerable<string> operationNamePatterns)
        {
            this.operationNamePatterns = operationNamePatterns;
        }

        public void SetSampled(ISpan span)
        {
            if (span != null)
            {
                if (span.OperationName.EqualsPatternAny(this.operationNamePatterns))
                {
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "peroparation");
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, 1.0);
                    span.SetSampled(true);
                }
                else
                {
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerType, "peroparation");
                    span.Tags.AddOrUpdate(SpanTagKey.SamplerParam, 0.0);
                    span.SetSampled(false);
                }
            }
        }
    }
}