namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;
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
            this.EnsureFilterContext();

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
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/health'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard?q=TrackType=trace'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a>&nbsp;&nbsp;&nbsp;</br>
"); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                this.EnsureFilterContext();

                var entities = await this.repository.FindAllAsync(
                    this.filterContext.GetSpecifications<LogTrace>(),
                    this.filterContext.GetFindOptions<LogTrace>()).AnyContext();

                foreach(var entity in entities) // .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace }))
                {
                    var levelColor = "lime";
                    if(entity.Level.Equals("Verbose", StringComparison.OrdinalIgnoreCase) || entity.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#75715E";
                    }
                    else if(entity.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#FF8C00";
                    }
                    else if(entity.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) || entity.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#FF0000";
                    }

                    var messageColor = levelColor;
                    var extraStyles = string.Empty;

                    await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>");
                    await this.HttpContext.Response.WriteAsync($"{entity.Timestamp.ToUniversalTime():u}");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {levelColor}'>");
                    await this.HttpContext.Response.WriteAsync($"{entity.Level.ToUpper().Truncate(3, string.Empty)}</span>]");
                    await this.HttpContext.Response.WriteAsync(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logtraces/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;");
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>");
                    //await this.HttpContext.Response.WriteAsync(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
                    await this.HttpContext.Response.WriteAsync($"{entity.SpanId} {entity.Message} <a target=\"blank\" href=\"/api/operations/logtraces?q=Id={entity.Id}\">*</a> {entity.ParentSpanId} -> took {entity.Duration.Humanize()}");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync("</div>");
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>");
            }
        }

        private void EnsureFilterContext()
        {
            // environment (default: current environment)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Environment))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Environment), CriteriaOperator.Equal, Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production"));
            }

            // level (default: Information)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Level))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.Equal, "Information"));
            }

            // time range (default: last 24 hours)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Ticks))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.LessThanOrEqual, DateTime.UtcNow.Ticks));
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.GreaterThanOrEqual, DateTime.UtcNow.AddHours(-24).Ticks));
            }

            //foreach(var criteria in this.filterContext.Criterias)
            //{
            //    await this.HttpContext.Response.WriteAsync($"criteria: {criteria}<br/>");
            //}

            this.filterContext.Take ??= 1000; // get amount per request, repeat while logevents.ticks >= past

            //await foreach(var name in this.service.GetLogEventsAsync(this.filterContext))
            //{
            //    this.logger.LogInformation(name);
            //}
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
