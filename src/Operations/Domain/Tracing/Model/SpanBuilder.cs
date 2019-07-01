namespace Naos.Core.Operations.Domain
{
    using Naos.Foundation;

    public class SpanBuilder : ISpanBuilder
    {
        private readonly ITracer tracer;
        private readonly string operationName;
        private readonly string traceId;

        public SpanBuilder(ITracer tracer, string operationName, ISpan parent = null)
        {
            this.tracer = tracer;
            this.traceId = tracer.ActiveSpan?.TraceId;
            this.operationName = operationName;
        }

        public ISpan Build()
        {
            return new Span(this.traceId ?? IdGenerator.Instance.Next, RandomGenerator.GenerateString(5))
            .SetOperationName(this.operationName)
            .SetStartedDate();
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
            //    1a |  1c  (children)
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
            return this;
        }
    }
}