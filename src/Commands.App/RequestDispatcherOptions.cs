namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Configuration.App;
    using Naos.Foundation;

    public class RequestDispatcherOptions
    {
        public RequestDispatcherOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
