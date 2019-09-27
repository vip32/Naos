//namespace Naos.Operations.Infrastructure.Mongo
//{
//    using System.Collections.Generic;
//    using Microsoft.Extensions.Logging;
//    using Naos.Foundation;
//    using Naos.Operations.Domain;

//    public class MongoEntityMap
//    {
//        public MongoEntityMap(string entityProperty, string dtoProperty, string dtoPropertyFull)
//        {
//            this.EntityProperty = entityProperty;
//            this.DtoProperty = dtoProperty;
//            this.DtoPropertyFull = dtoPropertyFull;
//        }

//        public string EntityProperty { get; set; }

//        public string DtoProperty { get; set; }

//        public string DtoPropertyFull { get; set; }

//        public static IEnumerable<MongoEntityMap> CreateDefault() =>
//            new[]
//            {
//                new MongoEntityMap(nameof(LogEvent.Environment), LogPropertyKeys.Environment, $"Properties.{LogPropertyKeys.Environment}"),
//                new MongoEntityMap(nameof(LogEvent.Level), "Level", "Level"),
//                new MongoEntityMap(nameof(LogEvent.Ticks), LogPropertyKeys.Ticks, $"Properties.{LogPropertyKeys.Ticks}"), // .To<lo>()
//                new MongoEntityMap(nameof(LogEvent.TrackType), LogPropertyKeys.TrackType, $"Properties.{LogPropertyKeys.TrackType}"),
//                new MongoEntityMap(nameof(LogEvent.TrackId), LogPropertyKeys.TrackId, $"Properties.{LogPropertyKeys.TrackId}"),
//                new MongoEntityMap(nameof(LogEvent.Id), LogPropertyKeys.Id, $"Properties.{LogPropertyKeys.Id}"),
//                new MongoEntityMap(nameof(LogEvent.CorrelationId), LogPropertyKeys.CorrelationId, $"Properties.{LogPropertyKeys.CorrelationId}"),
//                new MongoEntityMap(nameof(LogEvent.Key), LogPropertyKeys.LogKey, $"Properties.{LogPropertyKeys.LogKey}"),
//                new MongoEntityMap(nameof(LogEvent.Message), "RenderedMessage", "RenderedMessage"),
//                new MongoEntityMap(nameof(LogEvent.Timestamp), "Timestamp", "Timestamp"), // to DateTime
//                new MongoEntityMap(nameof(LogEvent.SourceContext), "SourceContext", "Properties.SourceContext"),
//                new MongoEntityMap(nameof(LogEvent.ServiceName), LogPropertyKeys.ServiceName, $"Properties.{LogPropertyKeys.ServiceName}"),
//                new MongoEntityMap(nameof(LogEvent.ServiceProduct), LogPropertyKeys.ServiceProduct, $"Properties.{LogPropertyKeys.ServiceProduct}"),
//                new MongoEntityMap(nameof(LogEvent.ServiceCapability), LogPropertyKeys.ServiceCapability, $"Properties.{LogPropertyKeys.ServiceCapability}"),
//            };
//    }
//}
