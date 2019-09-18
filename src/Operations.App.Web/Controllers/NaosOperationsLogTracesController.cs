namespace Naos.Core.Operations.App.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;
    using Naos.Core.RequestFiltering.App;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("api/operations/logtraces")]
    [ApiController]
    public class NaosOperationsLogTracesController : ControllerBase
    {
        private readonly ILogger<NaosOperationsLogTracesController> logger;
        private readonly FilterContext filterContext;
        private readonly ILogTraceRepository repository;
        private readonly ILogEventService service;
        private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();

        public NaosOperationsLogTracesController(
            ILoggerFactory loggerFactory,
            ILogTraceRepository repository,
            ILogEventService service,
            IFilterContextAccessor filterContext)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureThat.EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsLogTracesController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.repository = repository;
            this.service = service;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<IEnumerable<LogTrace>>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().AnyContext();
            //}

            return this.Ok(await this.GetJsonAsync().AnyContext());
        }

        [HttpGet]
        [Route("dashboard")]
        [Produces("text/html")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public Task GetHtml()
        {
            return this.GetHtmlAsync();
        }

        private async Task<IEnumerable<LogTrace>> GetJsonAsync()
        {
            LoggingFilterContext.Prepare(this.filterContext);

            return await this.repository.FindAllAsync(
                this.filterContext.GetSpecifications<LogTrace>(),
                this.filterContext.GetFindOptions<LogTrace>()).AnyContext();
        }

        private async Task GetHtmlAsync()
        {
            this.HttpContext.Response.ContentType = "text/html";
            await this.HttpContext.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
    <link href='css/naos.css' rel ='stylesheet' />
</head>
<body>
    <pre style='color: cyan;font-size: xx-small;'>
    " + ResourcesHelper.GetLogoAsString() + @"
    </pre>
    <hr />
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/healthcheck'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard?q=TrackType=trace'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a>&nbsp;&nbsp;&nbsp;</br>
").AnyContext(); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                LoggingFilterContext.Prepare(this.filterContext);

                var entities = await this.repository.FindAllAsync(
                    this.filterContext.GetSpecifications<LogTrace>(),
                    this.filterContext.GetFindOptions<LogTrace>()).AnyContext();
                var nodes = Node<LogTrace>.CreateTree(entities, l => l.SpanId, l => l.ParentSpanId, true)
                    .Where(n => !n.Children.IsNullOrEmpty()).ToList();

                nodes.Render(
                    async t => await this.WriteTraceHeaderAsync(t).AnyContext(),
                    async t => await this.WriteTraceAsync(t).AnyContext(),
                    async i => await this.WriteTraceIndentAsync(i).AnyContext(),
                    new HtmlNodeRenderOptions());

                //foreach (var entity in entities) // .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace }))
                //{
                //    await this.WriteTraceAsync(entity).AnyContext();
                //}
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>").AnyContext();
            }
        }

        private async Task WriteTraceHeaderAsync(LogTrace entity)
        {
            var sb = this.stringBuilderPool.Get(); // less allocations
            if (entity.ParentSpanId.IsNullOrEmpty())
            {
                sb.AppendLine("<hr/>");
            }

            sb.AppendLine($"<div style='white-space: nowrap;'>");
            sb.AppendLine("<span style='color: #EB1864; font-size: x-small;'>");
            sb.AppendFormat("{0:u}", entity.Timestamp.ToUniversalTime()).AppendLine();
            sb.AppendLine("</span>");
            sb.Append("&nbsp;[<span style='color: ").Append(this.GetTraceLevelColor(entity)).AppendLine("'>");
            sb.Append(entity.Kind?.ToUpper().Truncate(6, string.Empty)).AppendLine("</span>]");
            //sb.AppendLine(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logtraces/dashboard?q=TrackType=trace,CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;");

            await this.HttpContext.Response.WriteAsync(sb.ToString()).AnyContext();
            this.stringBuilderPool.Return(sb);
        }

        private async Task WriteTraceIndentAsync(string indent)
        {
            await this.HttpContext.Response.WriteAsync(indent).AnyContext();
        }

        private async Task WriteTraceAsync(LogTrace entity)
        {
            var extraStyles = string.Empty;

            var sb = this.stringBuilderPool.Get(); // less allocations
            sb.Append("<span style='color: ").Append(this.GetTraceLevelColor(entity)).Append("; ").Append(extraStyles).AppendLine("'>");
            //sb.AppendLine(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
            sb.Append(entity.Message).Append(" (").Append(entity.SpanId).Append("/").Append(entity.ParentSpanId).Append(")")
                .AppendLine(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logtraces/dashboard?q=TrackType=trace,CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;")
                .Append("<a target=\"blank\" href=\"/api/operations/logtraces?q=Id=").Append(entity.Id).Append("\">*</a> <span style=\"color: gray;\">-> took ").Append(entity.Duration.Humanize()).AppendLine("</span>");
            sb.AppendLine("</span>");
            sb.AppendLine("</div>");

            await this.HttpContext.Response.WriteAsync(sb.ToString()).AnyContext();
            this.stringBuilderPool.Return(sb);
        }

        private string GetTraceLevelColor(LogTrace entity)
        {
            var levelColor = "lime";
            if (entity.Status.SafeEquals(nameof(SpanStatus.Transient)))
            {
                levelColor = "#75715E";
            }
            else if (entity.Status.SafeEquals(nameof(SpanStatus.Cancelled)))
            {
                levelColor = "#FF8C00";
            }
            else if (entity.Status.SafeEquals(nameof(SpanStatus.Failed)))
            {
                levelColor = "#FF0000";
            }

            return levelColor;
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
