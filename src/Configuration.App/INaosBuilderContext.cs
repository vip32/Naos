namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public interface INaosBuilderContext
    {
        IConfiguration Configuration { get; set; }

        Naos.Core.Common.ServiceDescriptor Descriptor { get; set; }

        IServiceCollection Services { get; set; }

        List<string> Messages { get; set; }

        INaosBuilderContext AddTag(string tag);
    }
}