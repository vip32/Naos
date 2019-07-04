namespace Naos.Core.Tracing.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class SpanViewModel
    {
        public long StartTime { get; set; } // ms

        public long EndTime { get; set; } // ms

        public int NestingLevel { get; set; }

        public long Duration { get; set; } // ms

        public int DurationPercentage { get; set; } // rounded %

        public int EndPosition => decimal.ToInt32(this.DurationPercentage / 2);

        public List<SpanViewModel> Children { get; set; } = new List<SpanViewModel>();

        public static SpanViewModel Create(IEnumerable<ISpan> spans)
        {
            var rootSpan = spans.FirstOrDefault(s => s.ParentSpanId.IsNullOrEmpty());

            if(rootSpan != null)
            {
                var result = new SpanViewModel()
                {
                    StartTime = rootSpan.StartTime.Value.ToEpochMilliseconds(),
                    EndTime = rootSpan.EndTime.Value.ToEpochMilliseconds(),
                    Duration = rootSpan.EndTime.Value.ToEpochMilliseconds() - rootSpan.StartTime.Value.ToEpochMilliseconds(),
                    DurationPercentage = 100,
                    NestingLevel = 0,
                };

                var childSpans = spans.Where(s => s.ParentSpanId == rootSpan.SpanId);
                if(childSpans.SafeAny())
                {
                    foreach(var span in childSpans)
                    {
                        var viewModel = new SpanViewModel()
                        {
                            StartTime = rootSpan.StartTime.Value.ToEpochMilliseconds(),
                            EndTime = rootSpan.EndTime.Value.ToEpochMilliseconds(),
                            Duration = rootSpan.EndTime.Value.ToEpochMilliseconds() - rootSpan.StartTime.Value.ToEpochMilliseconds(),
                            DurationPercentage = decimal.ToInt32((rootSpan.EndTime.Value.ToEpochMilliseconds() - rootSpan.StartTime.Value.ToEpochMilliseconds()) / result.Duration * 100),
                            NestingLevel = 1,
                        };

                        result.Children.Add(viewModel);
                    }
                }

                return result;
            }

            return null;
        }
    }
}

//        |-----|-----|-----|-----|-----|-----|-----|-----|-----|-----| =100%

// INBREQ [SPAN /api/customers-------------------------]                                        SERVER receives request
// ......     |---[SPAN validatemodel -----------------]                                        INTERNAL validates the model
// INBREQ     |---[SPAN /api/accounts------------------]                                        SERVER receives request for data
// DOMREP     .       |---[SPAN getaccount---------]                                            INTERNAL repositpry finds entity
// DOMREP     |---[SPAN createentity---------------]                                            INTERNAL repositpry stores entity
// DOMEVT     .       |---[SPAN entitycreated------]                                            CONSUMER handles event
// MESSAG     .       |---[SPAN customercreated-----------------------]                         CONSUMER handles message
// QUEUNG     |---[SPAN emailnewcustomer --------------]                                        CONSUMER handles queue item

// each span has a parent span, when not it is the root span.
// each child span cannot take more time than the root span.
// child spans are drawn below the parent span.

// a single root span always has 100% width.


// SpanViewModel
// ------------
// long:starttime (ms)
// long:endtime   (ms)
// long:duration  (ms)
// int:duration%  (% duration of root span)
// ienumerable<SpanViewModel>:children (server/client/internal/producer/consumer)

// root span = 100% (duration)
