namespace Naos.Core.App.Web.Filters
{
    using System.Diagnostics;
    using FluentValidation;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Correlation;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment env;
        private readonly ILogger<GlobalExceptionFilter> logger;
        private readonly CorrelationContextAccessor correlationContext;

        public GlobalExceptionFilter(
            IHostingEnvironment environment,
            ILogger<GlobalExceptionFilter> logger,
            CorrelationContextAccessor correlationContext)
        {
            this.env = environment;
            this.logger = logger;
            this.correlationContext = correlationContext;
        }

        public void OnException(ExceptionContext context)
        {
            this.logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception?.Message);

            if (context.Exception is ValidationException exception)
            {
                var details = new ValidationProblemDetails
                {
                    //Title = $"the request for {{this.serviceDescriptor > ACCESSOR}} has validation errors, refer to the errors property for details",
                    Title = "a validation error occurred",
                    Instance = $"{this.correlationContext.Context.CorrelationId} ({this.correlationContext.Context.RequestId})",
                    //Detail = context.HttpContext.Request.Path,
                    Type = exception.GetType().FullPrettyName(),
                    Status = 400
                };

                (context.Exception as ValidationException)?.Errors.NullToEmpty().ForEach(f => details.Errors.Add(f.PropertyName, new[] { f.ToString() }));
                context.Result = new BadRequestObjectResult(details);
            }
            else
            {
                context.Result = new InternalServerErrorObjectResult(
                    new ProblemDetails
                    {
                        Title = "an unexpected error occurred",
                        Instance = $"{this.correlationContext.Context.CorrelationId} ({this.correlationContext.Context.RequestId})",
                        Type = context.Exception.GetType().FullPrettyName(),
                        Status = 500,
                        Detail = context.Exception.Demystify().ToString()
                    });
                //context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }
    }
}
