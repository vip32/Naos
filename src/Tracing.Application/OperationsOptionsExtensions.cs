namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Operations;
    using Naos.Tracing.Application;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class OperationsOptionsExtensions
    {
        public static OperationsOptions AddTracing(
            this OperationsOptions options,
            Action<OperationsTracingOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"naos services builder: tracing added");
            options.Context.Services.AddScoped<ITracer>(sp =>
            {
                return new Tracer(
                    new AsyncLocalScopeManager((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator))),
                    sp.GetService<ISampler>());
            });

            options.Context.Services.AddScoped<HttpClientTracerHandler>();

            if (optionsAction == null)
            {
                options.Context.Services.AddSingleton<ISampler, ConstantSampler>();
            }
            else
            {
                optionsAction.Invoke(new OperationsTracingOptions(options.Context));
            }

            //options.Context.Services.AddSingleton<ISampler>(sp => new OperationNamePatternSampler(new[] { "http*" })); // TODO: configure different samplers
            //options.Context.Services.AddSingleton<ISampler>(sp => new RateLimiterSampler(new RateLimiter(2.0, 2.0))); // TODO: configure different samplers

            return options;
        }
    }
}