namespace Naos.App.Web.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.RequestCorrelation.App;
    using Naos.RequestFiltering.App;

    [Route("api/[controller]")]
    [ApiController]
    public abstract class NaosControllerBase : ControllerBase
    {
        private ILogger logger;
        private IMediator mediator;
        private CorrelationContext correlationContext;
        private FilterContext filterContext;

        /// <summary>
        /// Gets the mediator.
        /// </summary>
        /// <value>
        /// The mediator.
        /// </value>
        protected IMediator Mediator =>
            this.mediator ?? (this.mediator = this.HttpContext.RequestServices.GetService<IMediator>());

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogger Logger =>
            this.logger ?? (this.logger = this.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger(this.GetType().Name));

        /// <summary>
        /// Gets the correlation context.
        /// </summary>
        /// <value>
        /// The correlation context.
        /// </value>
        protected CorrelationContext CorrelationContext =>
            this.correlationContext ?? (this.correlationContext = this.HttpContext.RequestServices.GetService<ICorrelationContextAccessor>()?.Context);

        /// <summary>
        /// Gets the filter context.
        /// </summary>
        /// <value>
        /// The filter context.
        /// </value>
        protected FilterContext FilterContext =>
            this.filterContext ?? (this.filterContext = this.HttpContext.RequestServices.GetService<IFilterContextAccessor>()?.Context);
    }
}
