namespace Naos.Core.App.Web
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static IMvcBuilder AddNaos(
            this IMvcBuilder mvcBuilder,
            Action<NaosMvcOptions> optionsAction = null)
        {
            var options = new NaosMvcOptions();
            optionsAction?.Invoke(options);

            if(!options.ControllerRegistrations.IsNullOrEmpty())
            {
                mvcBuilder
                    .ConfigureApplicationPartManager(o => o
                        .FeatureProviders.Add(
                            new GeneratedRepositoryControllerFeatureProvider(options.ControllerRegistrations)));
            }

            return mvcBuilder;
        }
    }
}
