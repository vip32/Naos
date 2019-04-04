namespace Naos.Core.ServiceExceptions.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlerMiddleware> logger;
        private readonly IEnumerable<IExceptionResponseHandler> responseHandlers;
        private readonly ExceptionHandlerMiddlewareOptions options;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger,
            IEnumerable<IExceptionResponseHandler> responseHandlers,
            IOptions<ExceptionHandlerMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.responseHandlers = responseHandlers;
            this.options = options.Value ?? new ExceptionHandlerMiddlewareOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch(Exception ex)
            {
                //this.logger.LogError(
                //    ex != null ? new EventId(ex.HResult) : new EventId(-1),
                //    ex,
                //    $"[{ex?.GetType().PrettyName()}] {ex?.Message}");

                var instance = context.HasCorrelationId() || context.HasRequestId()
                            ? $"{context.GetCorrelationId()} ({context.GetRequestId()})"
                            : context.TraceIdentifier;

                if(context.Response.HasStarted)
                {
                    this.logger.LogWarning("The response has already started, the http naos exception middleware will not be executed");
                    throw;
                }

                var responseHandler = this.responseHandlers?.FirstOrDefault(h => h.CanHandle(ex));
                if(responseHandler != null)
                {
                    // specific handler for exception
                    responseHandler.Handle(context, ex, instance, context.GetRequestId(), this.options.HideDetails, this.options.JsonResponse);
                }
                else
                {
                    // default handler for exception
                    var details = new ProblemDetails
                    {
                        Title = "An unhandled error occurred while processing the request",
                        Status = 500,
                        Instance = instance,
                        Detail = this.options.HideDetails ? null : ex.Demystify().ToString(),
                        Type = this.options.HideDetails ? null : ex.GetType().PrettyName(),
                    };

                    this.logger?.LogError(ex, $"{LogEventKeys.OutboundResponse} [{context.GetRequestId()}] http request {details.Title} [{ex.GetType().PrettyName()}] {ex.Message}");

                    context.Response.StatusCode = 500;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
