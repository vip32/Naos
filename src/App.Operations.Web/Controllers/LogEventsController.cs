namespace Naos.Core.App.Operations.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;
    using Naos.Core.Operations.Domain.Repositories;

    [Route("api/operations/[controller]")]
    [ApiController]
    public class LogEventsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<LogEventsController> logger;
        private readonly ILogEventRepository repository;

        public LogEventsController(
            ILogger<LogEventsController> logger,
            ILogEventRepository repository)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.repository = repository;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<LogEvent>>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().ConfigureAwait(false);
            //}

            return this.Ok(await this.GetJsonAsync().ConfigureAwait(false));
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
            return await this.repository.FindAllAsync().ConfigureAwait(false);
        }

        private async Task GetHtmlAsync()
        {
            this.HttpContext.Response.ContentType = "text/html";
            await this.HttpContext.Response.WriteAsync(@"
<html>
<head>
<link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
<meta charset='utf-8'>
<style>
    body {
        background-color: black;
        color: white;
        font-family: monospace;
        margin: 1em 0px;
        font-size: 12px;
    }
    a {
        text-decoration: none;
        color: gray;
    }
    a:link {
    }
    a:visited {
    }
    a:hover {
      color: white;
    }
    hr {
        display: block;
        height: 1px;
        border: 0;
        border-top: 1px solid #222222;
        margin: 1em 0;
        padding: 0; 
    }
</style>
</head>
<body>");
            try
            {
                var logEvents = await this.repository.FindAllAsync().ConfigureAwait(false);
                foreach (var logEvent in logEvents)
                {
                    var levelColor = "lime";
                    if (logEvent.Level.Equals("Verbose", StringComparison.OrdinalIgnoreCase) || logEvent.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#75715E";
                    }
                    else if (logEvent.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        levelColor = "#FF8C00";
                    }
                    else if (logEvent.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) || logEvent.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase))
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
                    await this.HttpContext.Response.WriteAsync($"</span>]&nbsp;{logEvent.CorrelationId}&nbsp;");
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>");
                    await this.HttpContext.Response.WriteAsync($"{logEvent.Message}");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync("</div>");
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>");
            }
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
