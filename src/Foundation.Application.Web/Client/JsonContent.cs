namespace Naos.Foundation.Application
{
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;

    public class JsonContent : StringContent // alternative is to use extensions in Microsoft.AspNet.WebApi.Client: https://github.com/dotnet/corefx/issues/26233 (PostAsJsonAsync)
    {
        public JsonContent(object obj)
            : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, ContentType.JSON.ToValue())
        {
        }

        public JsonContent(object obj, ContentType contentType)
            : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, contentType.ToValue())
        {
        }

        public JsonContent(object obj, Encoding encoding, ContentType contentType)
            : base(JsonConvert.SerializeObject(obj), encoding, contentType.ToValue())
        {
        }
    }
}
