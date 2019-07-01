namespace Naos.Core.Operations.Domain
{
    using EnsureThat;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SpanBuilder : ISpanBuilder
    {
        private readonly ITracer tracer;
        private readonly string operationName;
        private readonly string traceId;
        private readonly DataDictionary tags = new DataDictionary();

        public SpanBuilder(ITracer tracer, string operationName, ISpan parent = null)
        {
            EnsureArg.IsNotNull(tracer, nameof(tracer));

            this.tracer = tracer;
            this.traceId = parent?.TraceId;
            this.operationName = operationName;
            // TODO: copy baggage items from parent
        }

        public ISpan Build()
        {
            return new Span(this.traceId ?? IdGenerator.Instance.Next, RandomGenerator.GenerateString(5))
            .WithOperationName(this.operationName)
            .WithTags(this.tags)
            .Start();
        }

        public IScope Start(bool finishOnDispose = true)
        {
            return this.tracer.ScopeManager.Activate(this.Build(), finishOnDispose);
        }

        public ISpanBuilder ChildOf(ISpan parent)
        {
            return this;
        }

        public ISpanBuilder SiblingOf(ISpan parent)
        {
            //       1---2---4 (siblings)
            //     / | \
            //    1a |  1c     (children)
            //       1b
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            //this.traceId = null;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            this.tags.AddOrUpdate(key, value);
            return this;
        }
    }
}