namespace Naos.Foundation.Application
{
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Naos.Foundation;
    using Newtonsoft.Json;

    /// <summary>
    ///     Extends the HttpResponse type.
    /// </summary>
    public static class HttpResponseExtensions
    {
        public static void WriteJson<T>(this HttpResponse response, T content, JsonSerializerSettings settings = null, ContentType contentType = ContentType.JSON)
        {
            response.ContentType = contentType.ToValue();
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    JsonSerializer.Create(settings ?? DefaultJsonSerializerSettings.Create()).Serialize(jsonWriter, content);
                }
            }
        }
    }
}