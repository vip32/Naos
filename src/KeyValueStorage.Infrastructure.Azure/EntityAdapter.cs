namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.Azure.Cosmos.Table;
    using Naos.Core.Common;
    //using Microsoft.WindowsAzure.Storage;
    //using Microsoft.WindowsAzure.Storage.Table;
    using Naos.Core.KeyValueStorage.Domain;

    public class EntityAdapter : ITableEntity
    {
        private static readonly Dictionary<Type, Func<object, EntityProperty>> PropertyMap =
            new Dictionary<Type, Func<object, EntityProperty>>
            {
                [typeof(string)] = o => EntityProperty.GeneratePropertyForString((string)o),
                [typeof(byte[])] = o => EntityProperty.GeneratePropertyForByteArray((byte[])o),
                [typeof(bool)] = o => EntityProperty.GeneratePropertyForBool((bool)o),
                [typeof(DateTimeOffset)] = o => EntityProperty.GeneratePropertyForDateTimeOffset((DateTimeOffset)o),
                [typeof(DateTime)] = o => EntityProperty.GeneratePropertyForDateTimeOffset((DateTime)o),
                [typeof(double)] = o => EntityProperty.GeneratePropertyForDouble((double)o),
                [typeof(Guid)] = o => EntityProperty.GeneratePropertyForGuid((Guid)o),
                [typeof(int)] = o => EntityProperty.GeneratePropertyForInt((int)o),
                [typeof(long)] = o => EntityProperty.GeneratePropertyForLong((long)o)
            };

        private readonly Value value;
        private readonly string[] ignoreProperties;

        public EntityAdapter(Key key, string[] ignoreProperties = null)
        {
            this.ignoreProperties = ignoreProperties;
            this.Init(key, true);
        }

        public EntityAdapter(Value value, string[] ignoreProperties = null)
        {
            this.ignoreProperties = ignoreProperties;
            this.value = value;
            this.Init(value?.Key, true);
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string ETag { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            //Storage client uses this when it needs to transform this entity to a writeable instance
            var result = new Dictionary<string, EntityProperty>();
            foreach (var cell in this.value)
            {
                if (cell.Value == null || cell.Key.EqualsAny(this.ignoreProperties))
                {
                    continue;
                }

                EntityProperty property;
                var type = cell.Value.GetType();

                if (!PropertyMap.TryGetValue(type, out var factoryMethod))
                {
                    property = EntityProperty.GeneratePropertyForString(cell.Value.ToString());
                }
                else
                {
                    property = factoryMethod(cell.Value);
                }

                result[cell.Key] = property;
            }

            return result;
        }

        private void Init(Key key, bool useConcurencyKey)
        {
            EnsureArg.IsNotNull(key, nameof(key));

            this.PartitionKey = WebUtility.UrlEncode(key.PartitionKey);
            this.RowKey = WebUtility.UrlEncode(key.RowKey);
            this.ETag = "*";
        }
    }
}
