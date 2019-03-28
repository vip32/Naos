namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public interface INaosServicesContext
    {
        IConfiguration Configuration { get; set; }

        string Environment { get; set; }

        Naos.Core.Common.ServiceDescriptor Descriptor { get; set; }

        IServiceCollection Services { get; set; }

        List<string> Messages { get; set; }

        bool IsConsoleEnabled();

        INaosServicesContext AddTag(string tag);
    }
}