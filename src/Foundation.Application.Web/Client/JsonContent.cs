namespace Naos.Foundation.Application
{
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;

    public class JsonContent : StringContent // alternative is to use extensions in Microsoft.AspNet.WebApi.Client: https://github.com/dotnet/corefx/issues/26233 (PostAsJsonAsync)
    {
        public JsonContent(object content)
            : base(JsonConvert.SerializeObject(content), Encoding.UTF8, ContentType.JSON.ToValue())
        {
        }

        public JsonContent(object content, ContentType contentType)
            : base(JsonConvert.SerializeObject(content), Encoding.UTF8, contentType.ToValue())
        {
        }

        public JsonContent(object content, Encoding encoding, ContentType contentType)
            : base(JsonConvert.SerializeObject(content), encoding, contentType.ToValue())
        {
        }
    }
}
