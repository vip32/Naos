namespace Naos.Foundation.Application
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    ///     Extends the HttpContext type.
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

            context.Items.TryGetValue("correlationId", out var value);
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

            context.Items.TryGetValue("requestId", out var value);
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

            context.Items.TryGetValue("serviceName", out var value);
            return value?.ToString();
        }

        /// <summary>
        /// Gets an <see cref="IUrlHelper"/> instance. Uses <see cref="IUrlHelperFactory"/> and
        /// <see cref="IActionContextAccessor"/>.
        /// </summary>
        /// <param name="source">The HTTP context.</param>
        /// <returns>An <see cref="IUrlHelper"/> instance for the current request.</returns>
        public static IUrlHelper GetUrlHelper(this HttpContext source)
        {
            return source.RequestServices
                .GetRequiredService<IUrlHelper>();
        }

        /// <summary>
        /// Adds the Cache-Control and Pragma HTTP headers by applying the specified cache profile to the HTTP context.
        /// </summary>
        /// <param name="source">The HTTP context.</param>
        /// <param name="profile">The cache profile.</param>
        /// <returns>The same HTTP context.</returns>
        /// <exception cref="ArgumentNullException">context or cacheProfile.</exception>
        public static HttpContext ApplyCacheProfile(this HttpContext source, CacheProfile profile)
        {
            const string noCache = "no-cache";
            const string noCacheMaxAge = "no-cache,max-age=";
            const string noStore = "no-store";
            const string noStoreNoCache = "no-store,no-cache";
            const string publicMaxAge = "public,max-age=";
            const string privateMaxAge = "private,max-age=";

            EnsureThat.EnsureArg.IsNotNull(source, nameof(source));
            EnsureThat.EnsureArg.IsNotNull(profile, nameof(profile));

            var headers = source.Response.Headers;

            if (!string.IsNullOrEmpty(profile.VaryByHeader))
            {
                headers[HeaderNames.Vary] = profile.VaryByHeader;
            }

            if (profile.NoStore == true)
            {
                // Cache-control: no-store, no-cache is valid.
                if (profile.Location == ResponseCacheLocation.None)
                {
                    headers[HeaderNames.CacheControl] = noStoreNoCache;
                    headers[HeaderNames.Pragma] = noCache;
                }
                else
                {
                    headers[HeaderNames.CacheControl] = noStore;
                }
            }
            else
            {
                string cacheControlValue = null;
                var duration = profile.Duration.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
                switch (profile.Location)
                {
                    case ResponseCacheLocation.Any:
                        cacheControlValue = publicMaxAge + duration;
                        break;
                    case ResponseCacheLocation.Client:
                        cacheControlValue = privateMaxAge + duration;
                        break;
                    case ResponseCacheLocation.None:
                        cacheControlValue = noCacheMaxAge + duration;
                        headers[HeaderNames.Pragma] = noCache;
                        break;
                    default:
                        var exception = new NotImplementedException($"Unknown {nameof(ResponseCacheLocation)}: {profile.Location}");
                        Debug.Fail(exception.ToString());
                        throw exception;
                }

                headers[HeaderNames.CacheControl] = cacheControlValue;
            }

            return source;
        }
    }
}