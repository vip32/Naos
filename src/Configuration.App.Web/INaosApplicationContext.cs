namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public interface INaosApplicationContext
    {
        IApplicationBuilder Application { get; set; }

        IHostingEnvironment Environment { get; set; }

        List<string> Messages { get; set; }
    }
}