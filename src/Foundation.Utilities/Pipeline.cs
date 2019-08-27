namespace Naos.Foundation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// <para>
    /// The pipeline consists of a chain of processing steps, arranged so that the output of each step is the input of the next step.
    /// The pipeline is linear and one-directional.
    /// </para>
    /// <para>
    ///     [TPipeIn]-----.
    ///                   |
    ///               .---v-------------------------------------.
    ///               |   |--------.   .--------.   .--------.  |
    ///               |   | step   |-->| step   |-->| step   |  |
    ///               |   `--------`   `--------`   `--------|  |
    ///               `--------------------------------------|--`
    ///                   Pipeline<TPipeIn, TPipeOut>        |
    ///                                                      `----> [TPipeOut]
    /// </para>
    /// </summary>
    /// <typeparam name="TPipeIn"></typeparam>
    /// <typeparam name="TPipeOut"></typeparam>
    public class Pipeline<TPipeIn, TPipeOut>
    {
        private readonly List<object> pipelineSteps = new List<object>();

        public Pipeline(Func<TPipeIn, Pipeline<TPipeIn, TPipeOut>, TPipeOut> steps)
        {
            steps.Invoke(default, this); // invoke once to build the blocking collections
        }

        public Task<TPipeOut> Execute(TPipeIn input)
        {
            var firstStep = this.pipelineSteps[0] as IPipelineStep<TPipeIn>;
            var tcs = new TaskCompletionSource<TPipeOut>();

            firstStep.Buffer.Add(/*input*/new InputItem<TPipeIn>()
            {
                Value = input,
                TaskCompletionSource = tcs
            });

            return tcs.Task;
        }

        public PipelineStep<TStepIn, TStepOut> GenerateStep<TStepIn, TStepOut>()
        {
            var pipelineStep = new PipelineStep<TStepIn, TStepOut>();
            var stepIndex = this.pipelineSteps.Count;

            Task.Run(() =>
            {
                IPipelineStep<TStepOut> nextPipelineStep = null;

                foreach (var input in pipelineStep.Buffer.GetConsumingEnumerable())
                {
                    var isLastStep = stepIndex == this.pipelineSteps.Count - 1;
                    TStepOut output;

                    try
                    {
                        output = pipelineStep.StepAction(input.Value);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        input.TaskCompletionSource.SetException(e);
                        continue;
                    }

                    if (isLastStep)
                    {
                        input.TaskCompletionSource.SetResult((TPipeOut)(object)output);
                    }
                    else
                    {
                        nextPipelineStep = nextPipelineStep ?? (isLastStep ? null : this.pipelineSteps[stepIndex + 1] as IPipelineStep<TStepOut>);
                        nextPipelineStep.Buffer.Add(new InputItem<TStepOut>() { Value = output, TaskCompletionSource = input.TaskCompletionSource });
                    }
                }
            });

            this.pipelineSteps.Add(pipelineStep);
            return pipelineStep;
        }

#pragma warning disable SA1201 // Elements should appear in the correct order
        public interface IPipelineStep<TStepIn>
#pragma warning restore SA1201 // Elements should appear in the correct order
        {
            BlockingCollection<InputItem<TStepIn>> Buffer { get; set; }
        }

        public class PipelineStep<TStepIn, TStepOut> : IPipelineStep<TStepIn>
        {
            public BlockingCollection<InputItem<TStepIn>> Buffer { get; set; } = new BlockingCollection<InputItem<TStepIn>>();

            public Func<TStepIn, TStepOut> StepAction { get; set; }
        }

        public class InputItem<T>
        {
            public T Value { get; set; }

            public TaskCompletionSource<TPipeOut> TaskCompletionSource { get; set; }
        }
    }
}
