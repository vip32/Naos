namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public class NaosApplicationContext : INaosApplicationContext
    {
        public IApplicationBuilder Application { get; set; }

        public IWebHostEnvironment Environment { get; set; }

        public List<string> Messages { get; set; } = new List<string>();
    }
}
