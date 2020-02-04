namespace Naos.Operations.Application.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
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

    [Route("naos/operations/logevents")]
    [ApiController]
    public class NaosOperationsLogEventsController : ControllerBase
    {
        private readonly ILogger<NaosOperationsLogEventsController> logger;
        private readonly FilterContext filterContext;
        private readonly ILogEventRepository repository;
        private readonly ILogEventService service;
        private readonly ServiceDescriptor serviceDescriptor;

        public NaosOperationsLogEventsController(
            ILoggerFactory loggerFactory,
            ILogEventRepository repository,
            ILogEventService service,
            IFilterContextAccessor filterContext,
            ServiceDescriptor serviceDescriptor = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsLogEventsController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.repository = repository;
            this.service = service;
            this.serviceDescriptor = serviceDescriptor;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<IEnumerable<LogEvent>>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().AnyContext();
            //}

            return this.Ok(await this.GetJsonAsync().AnyContext());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<LogEvent>> Get(string id)
        {
            return this.Ok(await this.repository.FindOneAsync(id).AnyContext());
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

        private async Task<IEnumerable<LogEvent>> GetJsonAsync()
        {
            LoggingFilterContext.Prepare(this.filterContext);

            return await this.repository.FindAllAsync(
                this.filterContext.GetSpecifications<LogEvent>(),
                this.filterContext.GetFindOptions<LogEvent>()).AnyContext();
        }

        private Task GetHtmlAsync()
        {
            this.HttpContext.Response.WriteNaosDashboard(
                title: $"{this.serviceDescriptor?.ToString()} [{this.serviceDescriptor?.Tags.ToString("|")}]",
                action: async r =>
                {
                    var entities = this.GetJsonAsync().Result;
                    foreach (var entity in entities
                        .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace })))
                    {
                        var levelColor = "#96E228";
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

                        try
                        {
                            await r.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>").AnyContext();
                            await r.WriteAsync($"{entity.Timestamp.ToUniversalTime():u}").AnyContext();
                            await r.WriteAsync("</span>").AnyContext();
                            await r.WriteAsync($"&nbsp;[<span style='color: {levelColor}'>").AnyContext();
                            await r.WriteAsync($"{entity.Level.ToUpper().Truncate(3, string.Empty)}</span>]").AnyContext();
                            await r.WriteAsync(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/naos/operations/logevents/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;").AnyContext();
                            await r.WriteAsync($"&nbsp;<span style='color: #AE81FF;'>{entity.ServiceName.Truncate(12, string.Empty, TruncateFrom.Left)}</span>&nbsp;").AnyContext();
                            await r.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>").AnyContext();
                            //await r.WriteAsync(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
                            if (entity.Message?.Length > 5 && entity.Message.Take(6).All(char.IsUpper))
                            {
                                await r.WriteAsync($"<span style='color: #37CAEC;'>{entity.Message.Slice(0, 6)}</span>").AnyContext();
                                await r.WriteAsync($"{entity.Message.Slice(6)} <a target=\"blank\" href=\"/naos/operations/logevents/{entity.Id}\">*</a>").AnyContext();
                            }
                            else
                            {
                                await r.WriteAsync($"{entity.Message} <a target=\"blank\" href=\"/naos/operations/logevents/{entity.Id}\">*</a>").AnyContext();
                            }

                            if (!entity.CorrelationId.IsNullOrEmpty())
                            {
                                await r.WriteAsync($"&nbsp;<a target=\"blank\" href=\"/naos/operations/logtraces/dashboard?q=CorrelationId={entity.CorrelationId}\"><i class='far fa-clone'></i></a>").AnyContext();
                            }

                            await r.WriteAsync("</span>").AnyContext();
                            await r.WriteAsync("</div>").AnyContext();
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                }).Wait();

            return Task.CompletedTask;
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
