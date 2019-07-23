namespace Naos.Foundation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Pipeline<TPipeIn, TPipeOut>
    {
        private readonly List<object> pipelineSteps = new List<object>();

        public Pipeline(Func<TPipeIn, Pipeline<TPipeIn, TPipeOut>, TPipeOut> steps)
        {
            steps.Invoke(default, this); //Invoke just once to build blocking collections
        }

        public event Action<TPipeOut> Finished;

        public void Execute(TPipeIn input)
        {
            var first = this.pipelineSteps[0] as IPipelineStep<TPipeIn>;
            first.Buffer.Add(input);
        }

        public PipelineStep<TStepIn, TStepOut> GenerateStep<TStepIn, TStepOut>()
        {
            var pipelineStep = new PipelineStep<TStepIn, TStepOut>();
            var stepIndex = this.pipelineSteps.Count;

            Task.Run(() =>
            {
                IPipelineStep<TStepOut> nextPipelineStep = null;

                foreach(var input in pipelineStep.Buffer.GetConsumingEnumerable())
                {
                    var isLastStep = stepIndex == this.pipelineSteps.Count - 1;
                    var output = pipelineStep.StepAction(input);
                    if(isLastStep)
                    {
                            // This is dangerous as the invocation is added to the last step
                            // Alternatively, you can utilize BeginInvoke like here: https://stackoverflow.com/a/16336361/1229063
                            this.Finished?.Invoke((TPipeOut)(object)output);
                    }
                    else
                    {
                        nextPipelineStep = nextPipelineStep ?? (isLastStep ? null : this.pipelineSteps[stepIndex + 1] as IPipelineStep<TStepOut>);
                        nextPipelineStep.Buffer.Add(output);
                    }
                }
            });

            this.pipelineSteps.Add(pipelineStep);
            return pipelineStep;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class PipelineStep<TStepIn, TStepOut> : IPipelineStep<TStepIn>
    {
        public BlockingCollection<TStepIn> Buffer { get; set; } = new BlockingCollection<TStepIn>();

        public Func<TStepIn, TStepOut> StepAction { get; set; }
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    public interface IPipelineStep<TStepIn>
#pragma warning restore SA1201 // Elements should appear in the correct order
    {
        BlockingCollection<TStepIn> Buffer { get; set; }
    }
#pragma warning restore SA1402 // File may only contain a single type
}
