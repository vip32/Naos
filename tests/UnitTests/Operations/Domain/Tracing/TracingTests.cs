namespace Naos.Core.UnitTests.Operations.Domain
{
    using Naos.Core.Operations.Domain;
    using Shouldly;
    using Xunit;

    public class TracingTests : BaseTest
    {
        [Fact]
        public void TracerTests()
        {
            var tracer = new Tracer(new AsyncLocalScopeManager(null));

            using(var parentScope = tracer.BuildSpan("spanA").Activate())
            {
                parentScope.Span.SpanId.ShouldBe("spanA");
                tracer.ActiveSpan.SpanId.ShouldBe("spanA");

                parentScope.Span.SetTag("x", "xxx");

                using(var childScope = tracer.BuildSpan("spanB").Activate())
                {
                    childScope.Span.SpanId.ShouldBe("spanB");
                    tracer.ActiveSpan.SpanId.ShouldBe("spanB");

                    childScope.Span.SetTag("y", "yyy");
                    childScope.Span.Failed = true;
                }
            }
        }
    }
}

//tracer.ActiveSpan() // use in httpclient handler to propagate infos