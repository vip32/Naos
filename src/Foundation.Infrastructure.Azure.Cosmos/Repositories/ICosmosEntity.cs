namespace Naos.Foundation.Infrastructure
{
    using Newtonsoft.Json;

    public interface ICosmosEntity
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; } // maps to id
    }
}
