namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public interface INaosApplicationContext
    {
        IApplicationBuilder Application { get; set; }

#if NETCOREAPP3_0
        IWebHostEnvironment Environment { get; set; }
#endif

#if NETSTANDARD2_0
        IHostingEnvironment Environment { get; set; }
#endif

        List<string> Messages { get; set; }
    }
}