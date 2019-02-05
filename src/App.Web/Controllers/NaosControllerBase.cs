namespace Naos.Core.App.Web.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.RequestCorrelation.App;
    using Naos.Core.RequestFiltering.App;

    [Route("api/[controller]")]
    [ApiController]
    public abstract class NaosControllerBase : ControllerBase
    {
        private ILogger logger;
        private IMediator mediator;
        private CorrelationContext correlationContext;
        private FilterContext filterContext;

        protected IMediator Mediator => this.mediator ?? (this.mediator = this.HttpContext.RequestServices.GetService<IMediator>());

        protected ILogger Logger => this.logger ?? (this.logger = this.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger(this.GetType().Name));

        protected CorrelationContext CorrelationContext => this.correlationContext ?? (this.correlationContext = this.HttpContext.RequestServices.GetService<ICorrelationContextAccessor>()?.Context);

        protected FilterContext FilterContext => this.filterContext ?? (this.filterContext = this.HttpContext.RequestServices.GetService<IFilterContextAccessor>()?.Context);
    }
}
