namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public interface INaosBuilderContext
    {
        IConfiguration Configuration { get; set; }

        string Environment { get; set; }

        Naos.Foundation.ServiceDescriptor Descriptor { get; set; }

        IServiceCollection Services { get; set; }

        List<string> Messages { get; set; }

        bool IsConsoleEnabled();

        INaosBuilderContext AddTag(string tag);
    }
}