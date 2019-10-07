namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public class NaosApplicationContext : INaosApplicationContext
    {
        public IApplicationBuilder Application { get; set; }

#if NETCOREAPP3_0
        public IWebHostEnvironment Environment { get; set; }
#endif

#if NETSTANDARD2_0
        public IHostingEnvironment Environment { get; set; }
#endif

        public List<string> Messages { get; set; } = new List<string>();
    }
}
