namespace Naos.Foundation
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;

    public class HtmlNodeRenderOptions : INodeRenderOptions
    {
        private readonly HttpContext httpContext;

        public HtmlNodeRenderOptions(HttpContext httpContext)
        {
            EnsureArg.IsNotNull(httpContext, nameof(httpContext));

            this.httpContext = httpContext;
        }

        public string RootNodeBreak { get; set; } = "<br />";

        public string ChildNodeBreak { get; set; } = "<br />";

        public string Cross { get; set; } = "&nbsp;├─"; // " ├─";

        public string Corner { get; set; } = "&nbsp;└─"; //" └─";

        public string Vertical { get; set; } = "&nbsp;│&nbsp;"; //" │ ";

        public string Space { get; set; } = "&nbsp;&nbsp;&nbsp;"; //"   ";

        public async Task WriteAsync(string value)
        {
            await this.httpContext.Response.WriteAsync(value).AnyContext();
        }
    }
}
