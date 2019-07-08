namespace Naos.Core.UnitTests.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediatR;
    using Naos.Core.Tracing.Domain;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class TracingTests : BaseTest
    {
        [Fact]
        public void TracerTests()
        {
            var tracer = new Tracer(
                new AsyncLocalScopeManager(Substitute.For<IMediator>()));
            tracer.CurrentSpan.ShouldBeNull();
            ISpan span = null;
            var capturedSpans = new List<ISpan>();

            using(var parentScope = tracer.BuildSpan("spanA").Activate())
            {
                capturedSpans.Add(parentScope.Span);
                parentScope.Span.AddLog(SpanLogKey.Message, "test123");
                parentScope.Span.OperationName.ShouldBe("spanA");
                parentScope.Span.TraceId.ShouldNotBeNull();
                parentScope.Span.SpanId.ShouldNotBeNull();
                parentScope.Span.Status.ShouldBe(SpanStatus.Transient);
                parentScope.Span.Kind.ShouldBe(SpanKind.Internal);
                parentScope.Span.Logs.Count().ShouldBe(1); // contain logs

                tracer.CurrentSpan.ShouldNotBeNull();
                tracer.CurrentSpan.OperationName.ShouldBe("spanA");
                tracer.CurrentSpan.SpanId.ShouldBe(parentScope.Span.SpanId);

                parentScope.Span.WithTag("x", "xxx");
                span = parentScope.Span;

                using(var childScope = tracer.BuildSpan("spanB", SpanKind.Server)
                    .WithTag("a", "aaa").Activate())
                {
                    capturedSpans.Add(childScope.Span);
                    childScope.Span.OperationName.ShouldBe("spanB");
                    childScope.Span.TraceId.ShouldBe(parentScope.Span.TraceId);
                    childScope.Span.SpanId.ShouldNotBe(parentScope.Span.SpanId);
                    childScope.Span.Kind.ShouldBe(SpanKind.Server);

                    tracer.CurrentSpan.ShouldNotBeNull();
                    tracer.CurrentSpan.OperationName.ShouldBe("spanB");
                    tracer.CurrentSpan.SpanId.ShouldNotBe(parentScope.Span.SpanId);

                    childScope.Span.WithTag("y", "yyy");

                    var httpClient = new System.Net.Http.HttpClient();
                    // add ActiveSpan to http headers
                    // make httprequest (SendAsync)
                    // server should create span based on httpheaders
                }

                using(var failedScope = tracer.BuildSpan("failure").Activate())
                {
                    capturedSpans.Add(failedScope.Span);
                    var failedSpan = tracer.CurrentSpan;
                    try
                    {
                        throw new Exception("oops");
                    }
                    catch(Exception ex)
                    {
                        //tracer.End(status: SpanStatus.Failed, statusDescription: ex.Message);
                        tracer.Fail(exception: ex);
                    }

                    failedSpan.Status.ShouldBe(SpanStatus.Failed);
                    failedSpan.Logs.Count().ShouldBeGreaterThan(0); // contain error logs
                }

                using(var childScope = tracer.BuildSpan("message").Activate())
                {
                    capturedSpans.Add(childScope.Span);
                    // this happens in message handler (subscriber)
                    // get span TRACEID from message headers
                }
            }

            span.Status.ShouldBe(SpanStatus.Succeeded);
            tracer.CurrentSpan.ShouldBeNull();
        }
    }
}