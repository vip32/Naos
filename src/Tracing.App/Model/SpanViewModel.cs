namespace Naos.Tracing.App
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class SpanViewModel
    {
        public string SpanId { get; set; }

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

            if (rootSpan != null)
            {
                var result = new SpanViewModel()
                {
                    SpanId = rootSpan.SpanId,
                    StartTime = rootSpan.StartTime.Value.ToEpochMilliseconds(),
                    EndTime = rootSpan.EndTime.Value.ToEpochMilliseconds(),
                    Duration = rootSpan.EndTime.Value.ToEpochMilliseconds() - rootSpan.StartTime.Value.ToEpochMilliseconds(),
                    DurationPercentage = 100,
                    NestingLevel = 0,
                };

                var childSpans = spans.Where(s => s.ParentSpanId == rootSpan.SpanId);
                if (childSpans.SafeAny())
                {
                    foreach (var span in childSpans)
                    {
                        var viewModel = new SpanViewModel()
                        {
                            SpanId = span.SpanId,
                            StartTime = span.StartTime.Value.ToEpochMilliseconds(),
                            EndTime = span.EndTime.Value.ToEpochMilliseconds(),
                            Duration = span.EndTime.Value.ToEpochMilliseconds() - span.StartTime.Value.ToEpochMilliseconds(),
                            DurationPercentage = decimal.ToInt32((span.EndTime.Value.ToEpochMilliseconds() - span.StartTime.Value.ToEpochMilliseconds()) / result.Duration * 100),
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