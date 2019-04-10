namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Operations.Domain;
    using Naos.Core.RequestFiltering.App;

    [Route("api/operations/logevents")]
    [ApiController]
    public class NaosOperationsLogEventsController : ControllerBase
    {
        private readonly ILogger<NaosOperationsLogEventsController> logger;
        private readonly FilterContext filterContext;
        private readonly ILogEventRepository repository;
        private readonly ILogEventService service;

        public NaosOperationsLogEventsController(
            ILoggerFactory loggerFactory,
            ILogEventRepository repository,
            ILogEventService service,
            IFilterContextAccessor filterContext)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureThat.EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsLogEventsController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.repository = repository;
            this.service = service;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public ActionResult<IEnumerable<string>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().AnyContext();
            //}
            //var logEvents = await this.GetJsonAsync().AnyContext();
            //return this.Ok(logEvents);
            return this.Ok(new[] { "Test" });
        }

        [HttpGet]
        [Route("dashboard")]
        [Produces("text/html")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public Task GetHtml()
        {
            return this.GetHtmlAsync();
        }

        private async Task<IEnumerable<LogEvent>> GetJsonAsync()
        {
            await this.EnsureFilterContext();

            return await this.repository.FindAllAsync(
                this.filterContext.GetSpecifications<LogEvent>(),
                this.filterContext.GetFindOptions<LogEvent>()).AnyContext();
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
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/health'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logevents</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a></br>
"); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                await this.EnsureFilterContext();

                var logEvents = await this.repository.FindAllAsync(
                    this.filterContext.GetSpecifications<LogEvent>(),
                    this.filterContext.GetFindOptions<LogEvent>()).AnyContext();

                foreach(var logEvent in logEvents)
                {
                    var levelColor = "lime";
                    if(logEvent.Level.Equals("Verbose", StringComparison.OrdinalIgnoreCase) || logEvent.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#75715E";
                    }
                    else if(logEvent.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#FF8C00";
                    }
                    else if(logEvent.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) || logEvent.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#FF0000";
                    }

                    var messageColor = levelColor;
                    var extraStyles = string.Empty;

                    await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>");
                    await this.HttpContext.Response.WriteAsync($"{logEvent.Timestamp.ToUniversalTime():u}");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {levelColor}'>");
                    await this.HttpContext.Response.WriteAsync($"{logEvent.Level.ToUpper().Truncate(3, string.Empty)}");
                    await this.HttpContext.Response.WriteAsync($"</span>]&nbsp;{logEvent.CorrelationId}&nbsp;".Replace("&nbsp;&nbsp;", "&nbsp;"));
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>");
                    await this.HttpContext.Response.WriteAsync(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
                    await this.HttpContext.Response.WriteAsync($"{logEvent.Message} [{logEvent.Id}]");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync("</div>");
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>");
            }
        }

        private async Task EnsureFilterContext()
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

            foreach(var criteria in this.filterContext.Criterias)
            {
                await this.HttpContext.Response.WriteAsync($"criteria: {criteria}<br/>");
            }

            this.filterContext.Take = this.filterContext.Take ?? 1000; // get amount per request, repeat while logevents.ticks >= past

            //await foreach(var name in this.service.GetLogEventsAsync(this.filterContext))
            //{
            //    this.logger.LogInformation(name);
            //}
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
