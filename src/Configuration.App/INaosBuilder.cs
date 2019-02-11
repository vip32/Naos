namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;

    public interface INaosBuilder
    {
        IConfiguration Configuration { get; set; }

        Naos.Core.Common.ServiceDescriptor Descriptor { get; set; }

        IServiceCollection Services { get; set; }

        INaosBuilder AddTag(string tag);
    }
}