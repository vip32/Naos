namespace Naos.Operations.Application.Web
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.RequestFiltering.Application;
    using NSwag.Annotations;

    [Route("api/operations/health")]
    [ApiController]
    public class NaosOperationsHealthController : ControllerBase
    {
        private readonly ILogger<NaosOperationsHealthController> logger;
        private readonly FilterContext filterContext;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogEventService service;

        public NaosOperationsHealthController(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            ILogEventService service,
            IFilterContextAccessor filterContext)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
            EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsHealthController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.httpClientFactory = httpClientFactory;
            this.service = service;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<NaosHealthReport>> Get()
        {
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

        private async Task<NaosHealthReport> GetJsonAsync()
        {
            LoggingFilterContext.Prepare(this.filterContext);

            var httpClient = this.httpClientFactory.CreateClient("health");
            var response = await httpClient.GetAsync("https://localhost:5001/health").AnyContext();
            var result = await response.ReadAsAsync<NaosHealthReport>().AnyContext();
            result.CorrelationId = response.GetCorrelationIdHeader();
            return result;
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
    <link href='css/naos/styles.css' rel ='stylesheet' />
</head>
<body>
    <pre style='color: cyan;font-size: xx-small;'>
    " + ResourcesHelper.GetLogoAsString() + @"
    </pre>
    <hr />
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/health/dashboard'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a>&nbsp;&nbsp;&nbsp;</br>
").AnyContext(); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                LoggingFilterContext.Prepare(this.filterContext);

                var report = await this.GetJsonAsync().AnyContext();
                if(report != null)
                {
                    await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"{report.Timestamp.ToUniversalTime():u}").AnyContext();
                    await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {this.GetHealthLevelColor(report.Status)}'>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"{report.Status.ToUpper().PadRight(9, '.')/*.Truncate(3, string.Empty)*/}</span>]").AnyContext();
                    await this.HttpContext.Response.WriteAsync(!report.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logevents/dashboard?q=CorrelationId={report.CorrelationId}\">{report.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {this.GetHealthLevelColor(report.Status)}'>").AnyContext();
                    await this.HttpContext.Response.WriteAsync("service <a target=\"blank\" href=\"/health\">*</a>").AnyContext();
                    await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"<span style=\"color: gray;\">&nbsp;-> took {report.Took}</span>").AnyContext();

                    foreach (var entry in report.Entries.Safe())
                    {
                        await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>").AnyContext();
                        await this.HttpContext.Response.WriteAsync($"{report.Timestamp.ToUniversalTime():u}").AnyContext();
                        await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                        await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {this.GetHealthLevelColor(entry.Value.Status)}'>").AnyContext();
                        await this.HttpContext.Response.WriteAsync($"{entry.Value.Status.ToUpper().PadRight(9, '.')/*.Truncate(3, string.Empty)*/}</span>]").AnyContext();
                        await this.HttpContext.Response.WriteAsync(!report.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logevents/dashboard?q=CorrelationId={report.CorrelationId}\">{report.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;").AnyContext();
                        await this.HttpContext.Response.WriteAsync($"<span style='color: {this.GetHealthLevelColor(entry.Value.Status)}'>").AnyContext();
                        if (report.Entries.NextOf(entry.Value) != null)
                        {
                            await this.HttpContext.Response.WriteAsync("<span style='color: white;'>&nbsp;├─</span>").AnyContext();
                        }
                        else
                        {
                            await this.HttpContext.Response.WriteAsync("<span style='color: white;'>&nbsp;└─</span>").AnyContext();
                        }

                        await this.HttpContext.Response.WriteAsync($"{entry.Key} [{entry.Value.Tags.ToString("|")}] <a target=\"blank\" href=\"/health\">*</a>").AnyContext();
                        await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                        await this.HttpContext.Response.WriteAsync($"<span style=\"color: gray;\">&nbsp;-> took {entry.Value.Took}</span>").AnyContext();

                        if(!entry.Value.Data.IsNullOrEmpty())
                        {
                            foreach(var data in entry.Value.Data)
                            {
                                await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>").AnyContext();
                                await this.HttpContext.Response.WriteAsync($"{report.Timestamp.ToUniversalTime():u}").AnyContext();
                                await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                                await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {this.GetHealthLevelColor(entry.Value.Status)}'>").AnyContext();
                                await this.HttpContext.Response.WriteAsync($"{entry.Value.Status.ToUpper().PadRight(9, '.')/*.Truncate(3, string.Empty)*/}</span>]").AnyContext();
                                await this.HttpContext.Response.WriteAsync(!report.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logevents/dashboard?q=CorrelationId={report.CorrelationId}\">{report.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;").AnyContext();
                                if (report.Entries.NextOf(entry.Value) != null)
                                {
                                    await this.HttpContext.Response.WriteAsync("<span style='color: white;'>&nbsp;│</span>").AnyContext();
                                }
                                else
                                {
                                    await this.HttpContext.Response.WriteAsync("&nbsp;&nbsp;").AnyContext();
                                }

                                await this.HttpContext.Response.WriteAsync($"<span style='color: gray'>&nbsp;&nbsp;&nbsp;{data.Key}=</span>").AnyContext();
                                await this.HttpContext.Response.WriteAsync($"<span style='color: cyan'>{data.Value}</span>").AnyContext();
                                await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                                await this.HttpContext.Response.WriteAsync("</div>").AnyContext();
                            }
                        }

                        await this.HttpContext.Response.WriteAsync("</div>").AnyContext();
                    }

                    await this.HttpContext.Response.WriteAsync("</div>").AnyContext();
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>").AnyContext();
            }
        }

        private string GetHealthLevelColor(string status)
        {
            var levelColor = "lime";
            if (status.SafeEquals("Unhealthy"))
            {
                levelColor = "#FF0000";
            }
            else if (status.SafeEquals("Degraded"))
            {
                levelColor = "#FF8C00";
            }

            return levelColor;
        }
    }
}
