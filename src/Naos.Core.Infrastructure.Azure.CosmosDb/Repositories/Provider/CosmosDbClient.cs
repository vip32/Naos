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
            string endpointUrl,
            string authorizationKey,
            CosmosDbSqlConnectionPolicy connectionPolicy = null)
        {
            ValidateArguments(endpointUrl, authorizationKey);
            InitializeJsonSettings(null);

            var defaultPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };

            return new DocumentClient(
                new Uri(endpointUrl),
                authorizationKey,
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

        private static void InitializeJsonSettings(JsonSerializerSettings jsonSettings)
        {
            if (jsonSettings != null)
            {
                JsonConvert.DefaultSettings = () => jsonSettings;
            }
            else
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
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
}
