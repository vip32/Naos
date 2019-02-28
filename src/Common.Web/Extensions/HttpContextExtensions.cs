namespace Naos.Core.Common.Web
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    ///     Extends the HttpContext type
    /// </summary>
    public static class HttpContextExtensions
    {
        public static bool HasCorrelationId(this HttpContext context)
        {
            if (context == null)
            {
                return false;
            }

            return context.Items.ContainsKey("correlationId");
        }

        public static void SetCorrelationId(this HttpContext context, string value)
        {
            if (context == null)
            {
                return;
            }

            if (context.Items.ContainsKey("correlationId"))
            {
                context.Items.Remove("correlationId");
            }

            context.Items.Add("correlationId", value); // or use context.Features?
        }

        public static string GetCorrelationId(this HttpContext context)
        {
            if (context == null)
            {
                return default;
            }

            context.Items.TryGetValue("correlationId", out object value);
            return value?.ToString();
        }

        public static bool HasRequestId(this HttpContext context)
        {
            if (context == null)
            {
                return false;
            }

            return context.Items.ContainsKey("requestId");
        }

        public static void SetRequestId(this HttpContext context, string value)
        {
            if (context == null)
            {
                return;
            }

            if (context.Items.ContainsKey("requestId"))
            {
                context.Items.Remove("requestId");
            }

            context.Items.Add("requestId", value); // or use context.Features?
        }

        public static string GetRequestId(this HttpContext context)
        {
            if (context == null)
            {
                return default;
            }

            context.Items.TryGetValue("requestId", out object value);
            return value?.ToString();
        }

        public static bool HasServiceName(this HttpContext context)
        {
            if (context == null)
            {
                return false;
            }

            return context.Items.ContainsKey("serviceName");
        }

        public static void SetServiceName(this HttpContext context, string value)
        {
            if (context == null)
            {
                return;
            }

            if (context.Items.ContainsKey("serviceName"))
            {
                context.Items.Remove("serviceName");
            }

            context.Items.Add("serviceName", value); // or use context.Features?
        }

        public static string GetServiceName(this HttpContext context)
        {
            if (context == null)
            {
                return default;
            }

            context.Items.TryGetValue("serviceName", out object value);
            return value?.ToString();
        }
    }
}