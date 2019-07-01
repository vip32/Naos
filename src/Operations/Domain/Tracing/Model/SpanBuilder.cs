namespace Naos.Core.Operations.Domain
{
    using System;
    using Naos.Foundation;

    public class SpanBuilder : ISpanBuilder
    {
        private readonly ITracer tracer;
        private readonly string traceId;
        private readonly string operationName;

        public SpanBuilder(ITracer tracer, string operationName = null, ISpan parent = null)
        {
            this.tracer = tracer;
            this.traceId = parent?.TraceId;
            this.operationName = operationName ?? RandomGenerator.GenerateString(5);
        }

        public ISpan Build()
        {
            return new Span(this.traceId ?? RandomGenerator.GenerateString(13), this.operationName) // TODO: set all builder stuff
            {
                StartedTimestamp = DateTimeOffset.UtcNow
            };
        }

        public IScope Activate(bool finishSpanOnDispose = true)
        {
            return this.tracer.ScopeManager.Activate(this.Build(), finishSpanOnDispose);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder AsReferenceFrom(ISpan parent)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}