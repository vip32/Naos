namespace Naos.Core.Operations.Domain
{
    using EnsureThat;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SpanBuilder : ISpanBuilder
    {
        private readonly ITracer tracer;
        private readonly string operationName;
        private readonly SpanKind kind;
        private readonly string traceId;
        private readonly DataDictionary tags = new DataDictionary();
        private ISpan parent;

        public SpanBuilder(ITracer tracer, string operationName, SpanKind kind = SpanKind.Internal, ISpan parent = null)
        {
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNullOrEmpty(operationName, nameof(operationName));

            this.tracer = tracer;
            this.traceId = parent?.TraceId;
            this.operationName = operationName;
            this.kind = kind;
            this.parent = parent;
            // TODO: copy baggage items from parent
        }

        public ISpan Build()
        {
            return new Span(this.traceId ?? IdGenerator.Instance.Next, RandomGenerator.GenerateString(5), this.kind, this.parent?.SpanId)
                .WithOperationName(this.operationName)
                .WithTags(this.tags)
                .SetStatus(SpanStatus.Transient)
                .Start();
            }

        public IScope Activate(bool finishOnDispose = true)
        {
            return this.tracer.ScopeManager.Activate(this.Build(), finishOnDispose);
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
    }
}