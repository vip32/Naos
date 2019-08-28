namespace Microsoft.AspNetCore.Http
{
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.WebUtilities;
    using Naos.Foundation;
    using Newtonsoft.Json;

    /// <summary>
    ///     Extends the HttpResponse type.
    /// </summary>
    public static class HttpResponseExtensions
    {
        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(DefaultJsonSerializerSettings.Create());

        public static void WriteJson<T>(this HttpResponse response, T content, ContentType contentType = ContentType.JSON, JsonSerializerSettings settings = null)
        {
            response.ContentType = contentType.ToValue();
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    if (settings == null)
                    {
                        DefaultSerializer.Serialize(jsonWriter, content);
                    }
                    else
                    {
                        JsonSerializer.Create(DefaultJsonSerializerSettings.Create()).Serialize(jsonWriter, content);
                    }
                }
            }
        }

        public static Task Location(this HttpResponse response, string location = null)
        {
            if (location.IsNullOrEmpty())
            {
                return Task.CompletedTask;
            }

            if (!response.Headers.ContainsKey("Location"))
            {
                response.Headers.Add("Location", location);
            }

            return Task.CompletedTask;
        }

        public static Task Ok(this HttpResponse response, object content = null)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            if (content != null)
            {
                response.WriteJson(content);
            }

            return Task.CompletedTask;
        }

        public static Task Created(this HttpResponse response, string location = null)
        {
            response.StatusCode = (int)HttpStatusCode.Created;

            return response.Location(location);
        }

        public static Task Accepted(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.Accepted;
            return Task.CompletedTask;
        }

        public static Task NoContent(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NoContent;
            return Task.CompletedTask;
        }

        public static Task BadRequest(this HttpResponse response, string warning = null)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            if (!response.Headers.ContainsKey("Warning") && !warning.IsNullOrEmpty())
            {
                response.Headers.Add("Warning", $"199 naos \"{warning}\""); // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Warning
            }

            return Task.CompletedTask;
        }

        public static Task NotFound(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }

        public static Task InternalServerError(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Task.CompletedTask;
        }
    }
}