namespace Naos.Tracing.Domain
{
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;

    public class SpanBuilder : ISpanBuilder
    {
        private readonly ITracer tracer;
        private readonly string operationName;
        private readonly string logKey;
        private readonly SpanKind kind;
        private readonly string traceId;
        private readonly DataDictionary tags = new DataDictionary();
        private ISpan parent;
        private string spanId;

        public SpanBuilder(ITracer tracer, string operationName, string logKey = null, SpanKind kind = SpanKind.Internal, ISpan parent = null)
        {
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNullOrEmpty(operationName, nameof(operationName));

            this.tracer = tracer;
            this.traceId = parent?.TraceId;
            this.operationName = operationName;
            this.logKey = logKey ?? LogKeys.Tracing;
            this.kind = kind;
            this.parent = parent;
            // TODO: copy baggage items from parent
        }

        public ISpan Build()
        {
            var span = new Span(
                this.traceId ?? ActivityTraceId.CreateRandom().ToString() /*IdGenerator.Instance.Next*/,
                ActivitySpanId.CreateRandom().ToString() /*RandomGenerator.GenerateString(5)*/,
                this.kind,
                this.parent?.SpanId)
                .WithOperationName(this.operationName)
                .WithLogKey(this.logKey)
                .WithTags(this.tags)
                .SetStatus(SpanStatus.Transient)
                .SetSpanId(this.spanId)
                .Start();

            if (this.parent != null)
            {
                span.SetSampled(this.parent.IsSampled);
            }
            else
            {
                // root span sampling needs to be determined
                this.tracer.Sampler?.SetSampled(span);
            }

            return span;
        }

        public IScope Activate(ILogger logger, bool finishOnDispose = true)
        {
            return this.tracer.ScopeManager.Activate(this.Build(), logger, finishOnDispose);
        }

        public ISpanBuilder IgnoreParentSpan()
        {
            this.parent = null;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            this.tags.AddOrUpdate(key, value);
            return this;
        }

        public ISpanBuilder SetSpanId(string spanId)
        {
            this.spanId = spanId;
            return this;
        }
    }
}