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
            tracer.ActiveSpan.ShouldBeNull();

            using(var parentScope = tracer.BuildSpan("spanA").Start())
            {
                parentScope.Span.OperationName.ShouldBe("spanA");
                parentScope.Span.SpanId.ShouldNotBeNull();
                tracer.ActiveSpan.ShouldNotBeNull();
                tracer.ActiveSpan.OperationName.ShouldBe("spanA");
                tracer.ActiveSpan.SpanId.ShouldBe(parentScope.Span.SpanId);

                parentScope.Span.WithTag("x", "xxx");

                using(var childScope = tracer.BuildSpan("spanB").Start())
                {
                    childScope.Span.OperationName.ShouldBe("spanB");
                    childScope.Span.TraceId.ShouldBe(parentScope.Span.TraceId);
                    childScope.Span.SpanId.ShouldNotBe(parentScope.Span.SpanId);
                    tracer.ActiveSpan.ShouldNotBeNull();
                    tracer.ActiveSpan.OperationName.ShouldBe("spanB");
                    tracer.ActiveSpan.SpanId.ShouldNotBe(parentScope.Span.SpanId);

                    childScope.Span.WithTag("y", "yyy");
                    childScope.Span.Failed = true;

                    var httpClient = new System.Net.Http.HttpClient();
                    // add ActiveSpan to http headers
                    // make httprequest (SendAsync)
                    // server should create span based on httpheaders
                }

                using(var childScope = tracer.BuildSpan("message")
                    .IgnoreActiveSpan().Start())
                {
                    // this happens in message handler (subscriber)
                    // get span TRACEID from message headers
                }
            }

            tracer.ActiveSpan.ShouldBeNull();
        }
    }
}

//tracer.ActiveSpan() // use in httpclient handler to propagate infos

//
//    -----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|
//
//       [ span A -------------------------------]
//            [ span B ------------------------]
//                       [ span C (http) ----]
