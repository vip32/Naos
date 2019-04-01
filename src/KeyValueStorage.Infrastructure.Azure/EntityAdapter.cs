namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Naos.Core.KeyValueStorage.Domain;

    public class EntityAdapter : ITableEntity
    {
        private static readonly Dictionary<Type, Func<object, EntityProperty>> PropertyMap = new Dictionary<Type, Func<object, EntityProperty>>
        {
            [typeof(string)] = o => EntityProperty.GeneratePropertyForString((string)o),
            [typeof(byte[])] = o => EntityProperty.GeneratePropertyForByteArray((byte[])o),
            [typeof(bool)] = o => EntityProperty.GeneratePropertyForBool((bool)o),
            [typeof(DateTimeOffset)] = o => EntityProperty.GeneratePropertyForDateTimeOffset((DateTimeOffset)o),
            [typeof(DateTime)] = o => EntityProperty.GeneratePropertyForDateTimeOffset((DateTimeOffset)(DateTime)o),
            [typeof(double)] = o => EntityProperty.GeneratePropertyForDouble((double)o),
            [typeof(Guid)] = o => EntityProperty.GeneratePropertyForGuid((Guid)o),
            [typeof(int)] = o => EntityProperty.GeneratePropertyForInt((int)o),
            [typeof(long)] = o => EntityProperty.GeneratePropertyForLong((long)o)
        };

        private readonly Value row;

        public EntityAdapter(Key rowId)
        {
            this.Init(rowId, true);
        }

        public EntityAdapter(Value row)
        {
            this.row = row;

            this.Init(row?.Key, true);
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
            foreach (KeyValuePair<string, object> cell in this.row)
            {
                if (cell.Value == null)
                {
                    continue;
                }

                EntityProperty property;
                Type type = cell.Value.GetType();

                if (!PropertyMap.TryGetValue(type, out Func<object, EntityProperty> factoryMethod))
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

        private static string ToInternalId(string userId)
        {
            return WebUtility.UrlEncode(userId);
        }

        private void Init(Key rowId, bool useConcurencyKey)
        {
            EnsureArg.IsNotNull(rowId, nameof(rowId));

            this.PartitionKey = ToInternalId(rowId.PartitionKey);
            this.RowKey = ToInternalId(rowId.RowKey);
            this.ETag = "*";
        }
    }
}
