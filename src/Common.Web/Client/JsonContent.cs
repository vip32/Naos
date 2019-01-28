namespace Naos.Core.Common.Web.Client
{
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;

    public class JsonContent : StringContent
    {
        public JsonContent(object obj)
            : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, ContentType.JSON.ToValue())
        {
        }
    }
}
