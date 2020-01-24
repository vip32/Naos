namespace Naos.Operations.Application.Web
{
    using System.Collections.Generic;
    using System.Linq;
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
    using Naos.Operations.Domain;
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

        private async Task<NaosHealthReport> GetJsonAsync()
        {
            LoggingFilterContext.Prepare(this.filterContext);

            var httpClient = this.httpClientFactory.CreateClient("health");
            var response = await httpClient.GetAsync("https://localhost:5001/health").AnyContext();
            if (response.IsSuccessStatusCode)
            {
                return await response.ReadAsAsync<NaosHealthReport>().AnyContext();
            }

            return null;
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
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/health'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a>&nbsp;&nbsp;&nbsp;</br>
").AnyContext(); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                LoggingFilterContext.Prepare(this.filterContext);

                var entities = await this.repository.FindAllAsync(
                    this.filterContext.GetSpecifications<LogEvent>(),
                    this.filterContext.GetFindOptions<LogEvent>()).AnyContext();

                foreach (var entity in entities
                    .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace })))
                {
                    var levelColor = "lime";
                    if (entity.Level.SafeEquals(nameof(LogLevel.Trace)) || entity.Level.SafeEquals(nameof(LogLevel.Debug)) || entity.Level.SafeEquals("Verbose"))
                    {
                        levelColor = "#75715E";
                    }
                    else if (entity.Level.SafeEquals(nameof(LogLevel.Warning)))
                    {
                        levelColor = "#FF8C00";
                    }
                    else if (entity.Level.SafeEquals(nameof(LogLevel.Critical)) || entity.Level.SafeEquals(nameof(LogLevel.Error)) || entity.Level.SafeEquals("Fatal"))
                    {
                        levelColor = "#FF0000";
                    }

                    var messageColor = levelColor;
                    var extraStyles = string.Empty;

                    await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"{entity.Timestamp.ToUniversalTime():u}").AnyContext();
                    await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {levelColor}'>").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"{entity.Level.ToUpper().Truncate(3, string.Empty)}</span>]").AnyContext();
                    await this.HttpContext.Response.WriteAsync(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logevents/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;").AnyContext();
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>").AnyContext();
                    //await this.HttpContext.Response.WriteAsync(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
                    await this.HttpContext.Response.WriteAsync($"{entity.Message} <a target=\"blank\" href=\"/api/operations/logevents/{entity.Id}\">*</a>").AnyContext();
                    await this.HttpContext.Response.WriteAsync("</span>").AnyContext();
                    await this.HttpContext.Response.WriteAsync("</div>").AnyContext();
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>").AnyContext();
            }
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
