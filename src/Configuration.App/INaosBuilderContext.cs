namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;

    public interface INaosBuilderContext
    {
        IConfiguration Configuration { get; set; }

        Naos.Core.Common.ServiceDescriptor Descriptor { get; set; }

        IServiceCollection Services { get; set; }

        INaosBuilderContext AddTag(string tag);
    }
}