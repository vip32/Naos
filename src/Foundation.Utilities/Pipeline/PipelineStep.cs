namespace Naos.Foundation
{
    using System;
    using System.Collections.Concurrent;

    public partial class Pipeline<TPipeIn, TPipeOut>
    {
        public class PipelineStep<TStepIn, TStepOut> : IPipelineStep<TStepIn>, IDisposable
        {
            public BlockingCollection<InputItem<TStepIn>> Buffer { get; set; } = new BlockingCollection<InputItem<TStepIn>>();

            public Func<TStepIn, TStepOut> StepAction { get; set; }

            public void Dispose()
            {
                this.Buffer?.Dispose();
            }
        }
    }
}
