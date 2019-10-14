namespace Naos.Foundation
{
    using System.Text.Json;

    public static class DefaultJsonSerializerOptions
    {
        public static JsonSerializerOptions Create()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
                // https://github.com/dotnet/corefx/blob/master/src/System.Text.Json/docs/SerializerProgrammingModel.md
            };
        }
    }
}