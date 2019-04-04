namespace Naos.Core.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common;
    using Newtonsoft.Json;

    /// <summary>
    ///     Extends the HttpResponseMessage type
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        ///     Returns an individual header value
        /// </summary>
        /// <param name="source">the request to use</param>
        /// <param name="name">the key of the querystring arg</param>
        /// <returns>the value</returns>
        public static string GetHeader(this HttpResponseMessage source, string name)
        {
            if(source == null)
            {
                return null;
            }

            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            var result = source.GetHeaderInternal(name);
            if(string.IsNullOrEmpty(result))
            {
                result = source.GetHeaderInternal($"X-{name}");
            }

            return result;
        }

        public static string GetErrorHeader(this HttpResponseMessage source)
        {
            return source.GetHeader("http-error-description");
        }

        public static string GetCorrelationIdHeader(this HttpResponseMessage source)
        {
            return source.GetHeader("correlationid");
        }

        public static string GetRequestIdHeader(this HttpResponseMessage source)
        {
            return source.GetHeader("requestid");
        }

        public static string GetEntityIdHeader(this HttpResponseMessage source)
        {
            return source.GetHeader("entityid");
        }

        /// <summary>
        ///     Returns all header values as a dictionary
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public static IDictionary<string, string> GetHeaders(
            this HttpResponseMessage source,
            string prefix = null)
        {
            if(source == null || source.Headers == null)
            {
                return null;
            }

            if(string.IsNullOrEmpty(prefix))
            {
                return source.Headers
                    .ToDictionary(h => h.Key, h => h.Value?.FirstOrDefault());
            }
            else
            {
                return source.Headers
                    .Where(h => h.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(h => h.Key, h => h.Value?.FirstOrDefault());
            }
        }

        public static HttpResponseMessage WithErrorHeader(this HttpResponseMessage source, string value, bool allowEmptyValue = false)
        {
            return source.WithHeader("x-http-error-description", value, true);
        }

        public static HttpResponseMessage WithHeader(this HttpResponseMessage source, string key, string value, bool allowEmptyValue = false)
        {
            if(source == null || key.IsNullOrEmpty() || (value.IsNullOrEmpty() && !allowEmptyValue))
            {
                return source;
            }

            if(source.Headers.Contains(key))
            {
                source.Headers.Remove(key);
            }

            source.Headers.Add(key, value);

            return source;
        }

        public static HttpResponseMessage WithLocationHeader(this HttpResponseMessage source, string url, bool allowEmptyValue = false)
        {
            if(source == null || url.IsNullOrEmpty() || (url.IsNullOrEmpty() && !allowEmptyValue))
            {
                return source;
            }

            source.Headers.Location = new Uri(url);

            return source;
        }

        public static async Task<string> ReadAsStringAsync(this HttpResponseMessage source, string defaultValue = "")
        {
            if(source == null || source.Content == null)
            {
                return defaultValue;
            }

            return await source.Content.ReadAsStringAsync().AnyContext();
        }

        public static async Task<Stream> ReadAsStreamAsync(this HttpResponseMessage source)
        {
            if(source == null || source.Content == null)
            {
                return null;
            }

            return await source.Content.ReadAsStreamAsync().AnyContext();
        }

        public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage source)
        {
            if(source == null || source.Content == null)
            {
                return default;
            }

            return await source.Content.ReadAsJsonAsync<T>().AnyContext();
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            if(json.IsNullOrEmpty())
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///     Returns an individual HTTP Header value
        /// </summary>
        /// <param name="source">the request to use</param>
        /// <param name="name">the key of the header</param>
        /// /// <returns>the value</returns>
        private static string GetHeaderInternal(this HttpResponseMessage source, string name)
        {
            if(source == null || source.Headers == null)
            {
                return null;
            }

            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            return !source.Headers.TryGetValues(name, out var keys) ? null : keys.First();
        }
    }
}