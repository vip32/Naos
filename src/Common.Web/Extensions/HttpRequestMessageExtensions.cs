namespace Naos.Core.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Naos.Core.Common;
    using Newtonsoft.Json;

    /// <summary>
    ///     Extends the HttpRequestMessage type.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        ///     Returns a dictionary of QueryStrings that's easier to work with
        ///     than GetQueryNameValuePairs KevValuePairs collection.
        ///     If you need to pull a few single values use GetQueryString instead.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <returns>the querystring key/values.</returns>
        public static IDictionary<string, StringValues> GetQueryValues(this HttpRequestMessage source)
        {
            if(source == null)
            {
                return null;
            }

            return QueryHelpers.ParseNullableQuery(source.RequestUri?.Query);
        }

        /// <summary>
        ///     Returns an individual querystring value.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <param name="name">the key of the querystring arg.</param>
        /// <returns>the value.</returns>
        public static string GetQueryValue(this HttpRequestMessage source, string name)
        {
            if(source == null)
            {
                return default;
            }

            if(string.IsNullOrEmpty(name))
            {
                return default;
            }

            var queryParts = QueryHelpers.ParseNullableQuery(source.RequestUri.Query);
            if(queryParts == null || queryParts.Count == 0 || !queryParts.ContainsKey(name))
            {
                return default;
            }

            var match = queryParts
                .FirstOrDefault(kv => string.Compare(kv.Key, name, StringComparison.OrdinalIgnoreCase) == 0);
            return string.IsNullOrEmpty(match.Value) ? null : match.Value.FirstOrDefault();
        }

        /// <summary>
        ///     Returns individual querystring values.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <param name="name">the key of the querystring arg.</param>
        /// <returns>the value.</returns>
        public static StringValues GetQueryValues(this HttpRequestMessage source, string name)
        {
            if(source == null)
            {
                return default;
            }

            if(string.IsNullOrEmpty(name))
            {
                return default;
            }

            var queryParts = QueryHelpers.ParseNullableQuery(source.RequestUri.Query);
            if(queryParts == null || queryParts.Count == 0 || !queryParts.ContainsKey(name))
            {
                return default;
            }

            var match = queryParts
                .FirstOrDefault(kv => string.Compare(kv.Key, name, StringComparison.OrdinalIgnoreCase) == 0);
            return string.IsNullOrEmpty(match.Value) ? default : match.Value;
        }

        /// <summary>
        ///     Returns an individual querystring or header value.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <param name="name">the key of the querystring arg.</param>
        /// <returns>the value.</returns>
        public static string GetQueryOrHeaderValue(this HttpRequestMessage source, string name)
        {
            if(source == null)
            {
                return default;
            }

            if(string.IsNullOrEmpty(name))
            {
                return default;
            }

            var result = source.GetQueryValue(name);
            if(string.IsNullOrEmpty(result))
            {
                result = source.GetHeaderValue($"X-{name}");
            }

            return result;
        }

        /// <summary>
        ///     Returns an individual HTTP Header value.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <param name="name">the key of the header.</param>
        /// /// <returns>the value.</returns>
        public static string GetHeaderValue(this HttpRequestMessage source, string name)
        {
            if(source == null || source.Headers == null || string.IsNullOrEmpty(name))
            {
                return default;
            }

            return !source.Headers.TryGetValues(name, out var values) ? null : values.First();
        }

        /// <summary>
        ///     Returns an individual HTTP Header value.
        /// </summary>
        /// <param name="source">the request to use.</param>
        /// <param name="name">the key of the header.</param>
        /// /// <returns>the value.</returns>
        public static IEnumerable<string> GetHeaderValues(this HttpRequestMessage source, string name)
        {
            if(source == null || source.Headers == null || string.IsNullOrEmpty(name))
            {
                return default;
            }

            return !source.Headers.TryGetValues(name, out var keys) ? null : keys;
        }

        public static IDictionary<string, string> GetHeaders(
            this HttpRequestMessage source,
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

        ///// <summary>
        /////     Retrieves an individual cookie from the cookies collection
        ///// </summary>
        ///// <param name="source">the request to use</param>
        ///// <param name="name">the name of the cookie</param>
        ///// <returns>the value</returns>
        //public static string GetCookie(this HttpRequestMessage source, string name)
        //{
        //    if (source == null || source.Headers == null)
        //    {
        //        return null;
        //    }

        //    if (string.IsNullOrEmpty(name))
        //    {
        //        return null;
        //    }

        //    var cookie = source.Headers.GetCookies(name).FirstOrDefault();
        //    return cookie?[name].Value;
        //}

        /// <summary>
        ///     Retrieves the correlationid from a specific header or if not present
        ///     get it from the request directly.
        /// </summary>
        /// <param name="source"></param>
        public static string GetCorrelationId(this HttpRequestMessage source)
        {
            if(source == null)
            {
                return null;
            }

            return source.GetQueryOrHeaderValue("correlationid");
        }

        /// <summary>
        ///     Retrieves the requestid from a specific header or if not present
        ///     get it from the request directly.
        /// </summary>
        /// <param name="source"></param>
        public static string GetRequestId(this HttpRequestMessage source)
        {
            if(source == null)
            {
                return null;
            }

            return source.GetQueryOrHeaderValue("requestid");
        }

        public static HttpRequestMessage WithCorrelationId(this HttpRequestMessage source, string value)
        {
            return source.WithHeader("x-correlationid", value);
        }

        public static HttpRequestMessage WithHeaders(this HttpRequestMessage source, IDictionary<string, object> items, bool allowEmptyValue = false)
        {
            if(source == null)
            {
                return source;
            }

            foreach(var value in items.Safe())
            {
                source.WithHeader(value.Key, value.Value as string, allowEmptyValue);
            }

            return source;
        }

        public static HttpRequestMessage WithHeaders(this HttpRequestMessage source, IEnumerable<KeyValuePair<string, object>> items, bool allowEmptyValue = false)
        {
            if(source == null)
            {
                return source;
            }

            foreach(var value in items.Safe())
            {
                source.WithHeader(value.Key, value.Value as string, allowEmptyValue);
            }

            return source;
        }

        public static HttpRequestMessage WithHeaders(this HttpRequestMessage source, IDictionary<string, string> items, bool allowEmptyValue = false)
        {
            if(source == null)
            {
                return source;
            }

            foreach(var value in items.Safe())
            {
                source.WithHeader(value.Key, value.Value, allowEmptyValue);
            }

            return source;
        }

        public static HttpRequestMessage WithHeaders(this HttpRequestMessage source, IEnumerable<KeyValuePair<string, string>> items, bool allowEmptyValue = false)
        {
            if(source == null)
            {
                return source;
            }

            foreach(var value in items.Safe())
            {
                source.WithHeader(value.Key, value.Value, allowEmptyValue);
            }

            return source;
        }

        public static HttpRequestMessage WithHeader(this HttpRequestMessage source, string key, string value, bool allowEmptyValue = false)
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

        public static HttpRequestMessage WithAuthHeader(this HttpRequestMessage source, string scheme, string parameter, bool allowEmptyValue = false)
        {
            if(source == null || scheme.IsNullOrEmpty() || (parameter.IsNullOrEmpty() && !allowEmptyValue))
            {
                return source;
            }

            source.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);

            return source;
        }

        public static HttpRequestMessage WithAccept(this HttpRequestMessage source, ContentType contentType = ContentType.JSON)
        {
            if(source == null)
            {
                return source;
            }

            source.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType.ToValue()));

            return source;
        }

        public static HttpRequestMessage WithJsonContent(this HttpRequestMessage source, object content, JsonSerializerSettings serializerSettings = null)
        {
            if(source == null || content == null)
            {
                return source;
            }

            if(content.IsOfType(typeof(string)))
            {
                return source.WithContent(new StringContent(
                        content as string,
                        Encoding.UTF8,
                        ContentType.JSON.ToValue()));
            }

            return source.WithContent(new StringContent(
                        JsonConvert.SerializeObject(content, serializerSettings ?? DefaultJsonSerializerSettings.Create()),
                        Encoding.UTF8,
                        ContentType.JSON.ToValue()));
        }

        public static HttpRequestMessage WithTextContent(this HttpRequestMessage source, string content, ContentType? contentType)
        {
            if(source == null || content == null)
            {
                return source;
            }

            if(contentType == null)
            {
                source.Content = new StringContent(content);
            }
            else
            {
                source.Content = new StringContent(
                            content,
                            Encoding.UTF8,
                            contentType.Value.ToValue());
            }

            return source;
        }

        public static HttpRequestMessage WithContent(this HttpRequestMessage source, HttpContent httpContent, ContentType contentType = ContentType.JSON)
        {
            if(source == null || httpContent == null)
            {
                return source;
            }

            source.Content = httpContent.WithContentType(contentType);

            return source;
        }

        public static HttpContent WithContentType(this HttpContent source, ContentType contentType = ContentType.JSON)
        {
            if(source == null)
            {
                return source;
            }

            //source.Headers.Add("Content-Type", contentType.ToValue());
            source.Headers.ContentType = new MediaTypeHeaderValue(contentType.ToValue()); // double?

            return source;
        }

        public static HttpRequestMessage WithContentType(this HttpRequestMessage source, ContentType contentType = ContentType.JSON)
        {
            if(source == null)
            {
                return source;
            }

            // allow the content-type header to be set, by default this is not allowed. however some api's need it.
            // https://stackoverflow.com/questions/10679214/how-do-you-set-the-content-type-header-for-an-httpclient-request
            var field = typeof(HttpRequestHeaders).GetField("invalidHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static) ?? typeof(HttpRequestHeaders).GetField("s_invalidHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if(field != null)
            {
                var invalidFields = (HashSet<string>)field.GetValue(null);
                invalidFields.Remove("Content-Type");
            }

            source.Headers.TryAddWithoutValidation("Content-Type", contentType.ToValue());

            return source;
        }

        public static HttpContent WithContentDisposition(this HttpContent source, string name, string fileName = null)
        {
            if(source == null || name.IsNullOrEmpty())
            {
                return source;
            }

            if(fileName.IsNullOrEmpty())
            {
                source.Headers.Add("Content-Disposition", $"form-data; name=\"{name}\"");
            }
            else
            {
                source.Headers.Add("Content-Disposition", $"form-data; name=\"{name}\"; filename=\"{fileName}\"");
            }

            return source;
        }

        // public static void SetCorrelationId(this HttpRequestMessage source, string correlationId)
        // {
        //    if (source == null) return;

        // source.Properties.Add("correlationId", correlationId);
        // }
    }
}