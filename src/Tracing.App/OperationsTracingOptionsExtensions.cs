namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Tracing.App;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class OperationsTracingOptionsExtensions
    {
        public static OperationsTracingOptions UseSampler<T>(
            this OperationsTracingOptions options)
            where T : class, ISampler
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<ISampler, T>();

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: tracing sampler used (type={typeof(T).Name})");

            return options;
        }

        public static OperationsTracingOptions UseSampler(
            this OperationsTracingOptions options,
            ISampler sampler)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton(sampler);

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: tracing sampler used (type={sampler.GetType().Name})");

            return options;
        }
    }
}
