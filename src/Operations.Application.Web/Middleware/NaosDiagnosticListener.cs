namespace Naos.Operations.Application.Web
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DiagnosticAdapter;
    using Microsoft.Extensions.Logging;

    public class NaosDiagnosticListener
    {
        private readonly ILogger<NaosDiagnosticListener> logger;

        public NaosDiagnosticListener(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory?.CreateLogger<NaosDiagnosticListener>();
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareStarting")]
        public virtual void OnMiddlewareStarting(HttpContext httpContext, string name)
        {
            this.logger?.LogTrace($"middleware start: {name} {httpContext.Request.Path}");
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareException")]
        public virtual void OnMiddlewareException(Exception exception, string name)
        {
            this.logger?.LogWarning(exception, $"middleware fail: {name} {exception.Message}");
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareFinished")]
        public virtual void OnMiddlewareFinished(HttpContext httpContext, string name)
        {
            this.logger?.LogTrace($"middleware done: {name} {httpContext.Response.StatusCode}");
        }
    }
}
