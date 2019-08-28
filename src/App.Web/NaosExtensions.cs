namespace Naos.Core.App.Web
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using NSwag.Generation.Processors;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static IMvcBuilder AddNaos(
            this IMvcBuilder mvcBuilder,
            Action<NaosMvcOptions> optionsAction = null)
        {
            var options = new NaosMvcOptions();
            optionsAction?.Invoke(options);

            // add the generic repo controllers for all registrations
            if (!options.ControllerRegistrations.IsNullOrEmpty())
            {
                mvcBuilder
                    .AddMvcOptions(o =>
                    {
                        o.Filters.Add<OperationCancelledExceptionFilter>();
                        o.Conventions.Add(new GeneratedControllerRouteConvention());
                    })
                    .ConfigureApplicationPartManager(o => o
                        .FeatureProviders.Add(
                            new GenericRepositoryControllerFeatureProvider(
                                options.ControllerRegistrations)));

                mvcBuilder.Services.AddSingleton<IOperationProcessor>(new GenericRepositoryControllerOperationProcessor()); // needed for swagger generation (for each controller registration)
            }

            mvcBuilder.AddControllersAsServices(); // needed to resolve controllers through di https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
            mvcBuilder.AddJsonOptions(o => o.AddDefaultJsonSerializerSettings(options.JsonSerializerSettings));

            return mvcBuilder;
        }
    }
}
