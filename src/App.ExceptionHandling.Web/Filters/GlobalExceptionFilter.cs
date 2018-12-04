namespace Naos.Core.App.ExceptionHandling.Web
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using FluentValidation;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Correlation;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class GlobalExceptionFilter : IExceptionFilter // middleware is preferred for unhandled exceptio handling, this is better suited for MVC specific handling like ModelState
    {
        private readonly IHostingEnvironment env;
        private readonly ILogger<GlobalExceptionFilter> logger;
        private readonly IServiceProvider serviceProvider;

        public GlobalExceptionFilter(
            IHostingEnvironment environment,
            ILogger<GlobalExceptionFilter> logger,
            IServiceProvider serviceProvider)
        {
            this.env = environment;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public void OnException(ExceptionContext context)
        {
            this.logger.LogError(context.Exception != null ? new EventId(context.Exception.HResult) : new EventId(-1),
                context.Exception,
                $"[{context.Exception?.GetType().PrettyName()}] {context.Exception?.Message}");

            var correlationContext = this.serviceProvider.GetService<ICorrelationContextAccessor>();
            if (context.Exception is ValidationException validationException)
            {
                var details = new ValidationProblemDetails
                {
                    Title = "A validation exception has occurred while executing the request",
                    Instance = $"{correlationContext.Context.CorrelationId} ({correlationContext.Context.RequestId})",
                    Type = validationException.GetType().FullPrettyName(),
                    Status = 400,
                    Detail = validationException.Message,
                    //Detail = context.HttpContext.Request.Path,
                };

                validationException.Errors.NullToEmpty().ForEach(f => details.Errors.Add(f.PropertyName, new[] { f.ToString() }));
                context.Result = new BadRequestObjectResult(details);
            }
            else if (context.Exception is BadHttpRequestException badHttpRequestException)
            {
                context.Result = new ObjectResult(
                    new ProblemDetails
                    {
                        Title = "Invalid request",
                        Instance = $"{correlationContext.Context.CorrelationId} ({correlationContext.Context.RequestId})",
                        Type = context.Exception?.GetType().FullPrettyName(),
                        Status = (int)typeof(BadHttpRequestException).GetProperty("StatusCode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(badHttpRequestException),
                        Detail = context.Exception?.Demystify().ToString()
                    });
            }
            else
            {
                context.Result = new InternalServerErrorObjectResult(
                    new ProblemDetails
                    {
                        Title = "An unhandled exception has occurred while executing the request",
                        Instance = $"{correlationContext.Context.CorrelationId} ({correlationContext.Context.RequestId})",
                        Type = context.Exception?.GetType().FullPrettyName(),
                        Status = 500,
                        Detail = context.Exception?.Demystify().ToString()
                    });
            }

            context.HttpContext.Response.ContentType = ContentType.JSONPROBLEM.ToValue();
            context.ExceptionHandled = true;
        }
    }
}
