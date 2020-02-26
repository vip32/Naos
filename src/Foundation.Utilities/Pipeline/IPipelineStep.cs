namespace Naos.Foundation
{
    using System.Collections.Concurrent;

    public partial class Pipeline<TPipeIn, TPipeOut>
    {
#pragma warning disable SA1201 // Elements should appear in the correct order
        public interface IPipelineStep<TStepIn>
#pragma warning restore SA1201 // Elements should appear in the correct order
        {
            BlockingCollection<InputItem<TStepIn>> Buffer { get; set; }
        }
    }
}
