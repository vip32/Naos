namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public static class CosmosDbClient
    {
        public static IDocumentClient Create(
            string serviceEndpointUri,
            string authKeyOrResourceToken,
            CosmosDbSqlConnectionPolicy connectionPolicy = null,
            JsonSerializerSettings jsonSettings = null)
        {
            ValidateArguments(serviceEndpointUri, authKeyOrResourceToken);
            jsonSettings = EnsureJsonSettings(jsonSettings);

            var defaultPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };

            return new DocumentClient(
                new Uri(serviceEndpointUri),
                authKeyOrResourceToken,
                jsonSettings,
                connectionPolicy != null ?
                    new ConnectionPolicy
                    {
                        ConnectionMode = (ConnectionMode) Enum.Parse(typeof(ConnectionMode), connectionPolicy.ConnectionMode.ToString()),
                        ConnectionProtocol = (Protocol) Enum.Parse(typeof(Protocol), connectionPolicy.ConnectionProtocol.ToString())
                    }
                    : defaultPolicy);
        }

        private static void ValidateArguments(string endpointUrl, string authorizationKey)
        {
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                throw new ArgumentNullException(nameof(endpointUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationKey))
            {
                throw new ArgumentNullException(nameof(authorizationKey));
            }
        }

        private static JsonSerializerSettings EnsureJsonSettings(JsonSerializerSettings jsonSettings)
        {
            return jsonSettings != null
                ? jsonSettings
                : new JsonSerializerSettings
                {
                    // disabled, camel case does not work in linq queries
                    //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new JsonConverter[]
                    {
                        new StringEnumConverter(),
                        new IsoDateTimeConverter()
                    }
                };
        }
    }
}
