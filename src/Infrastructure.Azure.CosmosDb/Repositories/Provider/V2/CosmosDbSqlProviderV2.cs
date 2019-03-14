namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    //using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class CosmosDbSqlProviderV2<T> : ICosmosDbSqlProvider<T>
        where T : IDiscriminated
    {
        private readonly ILogger<CosmosDbSqlProviderV2<T>> logger;
        private readonly IDocumentClient client;
        private readonly string databaseId;
        private readonly AsyncLazy<Database> database;
        private readonly string partitionKeyPath;
        private readonly string partitionKeyValue;
        private readonly string collectionId;
        //private readonly string defaultIdentityPropertyName = "id";
        private readonly bool isMasterCollection;
        private readonly bool isPartitioned;
        private AsyncLazy<DocumentCollection> documentCollection;

        public CosmosDbSqlProviderV2(
            ILogger<CosmosDbSqlProviderV2<T>> logger,
            IDocumentClient client,
            string databaseId,
            Func<string> collectionIdFactory = null,
            string partitionKeyPath = "/Discriminator",
            int throughput = 400,
            //Expression<Func<T, object>> idNameFactory = null,
            bool isMasterCollection = true,
            IndexingPolicy indexingPolicy = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrEmpty(databaseId, nameof(databaseId));

            this.logger = logger;
            this.client = client;
            this.databaseId = databaseId;
            this.database = new AsyncLazy<Database>(async () => await this.GetOrCreateDatabaseAsync().AnyContext());
            this.partitionKeyPath = partitionKeyPath.EmptyToNull() ?? "/Discriminator";
            this.partitionKeyValue = typeof(T).FullName;

            this.isMasterCollection = isMasterCollection;
            if (collectionIdFactory != null)
            {
                this.collectionId = collectionIdFactory();
            }

            if (string.IsNullOrEmpty(this.collectionId))
            {
                this.collectionId = isMasterCollection ? "master" : typeof(T).Name;
            }

            this.documentCollection = new AsyncLazy<DocumentCollection>(async () =>
                await this.GetOrCreateCollectionAsync(this.partitionKeyPath, throughput).AnyContext());

            if (this.documentCollection.Value.Result != null)
            {
                if (this.documentCollection.Value.Result.PartitionKey?.Paths?.Any() == true)
                {
                    this.isPartitioned = true;
                }

                if (indexingPolicy == null)
                {
                    // change the default indexingPolicy so ORDER BY works better with string fields
                    // https://github.com/Azure/azure-cosmos-dotnet-v2/blob/2e9a48b6a446b47dd6182606c8608d439b88b683/samples/code-samples/IndexManagement/Program.cs#L305-L340
                    indexingPolicy = new IndexingPolicy();
                    indexingPolicy.IncludedPaths.Add(
                        new IncludedPath
                        {
                            Path = "/*",
                            Indexes = new Collection<Index>()
                            {
                                new RangeIndex(DataType.Number) { Precision = -1 },
                                new RangeIndex(DataType.String) { Precision = -1 } // needed for orderby on strings
                            }
                        });
                }

                this.documentCollection.Value.Result.IndexingPolicy = indexingPolicy;
            }

            //if (idNameFactory != null)
            //{
            //    this.TryGetIdProperty(idNameFactory);
            //}

            // https://github.com/Azure/azure-cosmosdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L211
        }

        public async Task<T> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return default;
            }

            try
            {
                return await this.client.ReadDocumentAsync<T>(
                    UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, id),
                    new RequestOptions { PartitionKey = new PartitionKey(this.partitionKeyValue) }).AnyContext();
            }
            catch (DocumentClientException ex)
            {
                if (ex.Message.Contains("Resource Not Found"))
                {
                    return default;
                }

                throw;
            }
        }

        public async Task<T> UpsertAsync(T entity)
        {
            var doc = await this.client.UpsertDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId),
                entity,
                new RequestOptions { PartitionKey = new PartitionKey(this.partitionKeyValue) }).AnyContext();
            return JsonConvert.DeserializeObject<T>(doc.Resource.ToString());
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                var result = await this.client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, id),
                    new RequestOptions { PartitionKey = new PartitionKey(this.partitionKeyValue) }).AnyContext();

                return result.StatusCode == HttpStatusCode.NoContent;
            }
            catch (DocumentClientException ex)
            {
                if (ex.Message.Contains("Resource Not Found"))
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression) // TODO: shouldn't this return IEnumerable<T>?
        {
            var query = this.client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned, PartitionKey = new PartitionKey(this.partitionKeyValue) })
                    .WhereExpression(expression)
                    .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
                    .AsEnumerable();
            this.logger.LogInformation($"{{LogKey:l}} sql={query.ToString().Replace("{", string.Empty).Replace("}", string.Empty)}", LogEventKeys.DomainRepository);
            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression = null,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            int count = 100,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            // cosmos only supports single orderby https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/16883608-allow-multi-order-by
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            var query = this.client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned, PartitionKey = new PartitionKey(this.partitionKeyValue) })
                    .WhereExpression(expression)
                    .WhereExpressions(expressions)
                    .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
                    .TakeIf(count)
                    .OrderByIf(orderExpression, orderDescending)
                    .AsEnumerable();
            this.logger.LogInformation($"{{LogKey:l}} sql={query.ToString().Replace("{", string.Empty).Replace("}", string.Empty)}", LogEventKeys.DomainRepository);
            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            int count = 100,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            // cosmos only supports single orderby https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/16883608-allow-multi-order-by
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            var query =
                this.client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned, PartitionKey = new PartitionKey(this.partitionKeyValue) })
                    .WhereExpression(expression)
                    .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
                    .Select(selector)
                    .TakeIf(count)
                    .OrderByIf(orderExpression, orderDescending)
                    .AsEnumerable();
            this.logger.LogInformation($"{{LogKey:l}} sql={query.ToString().Replace("{", string.Empty).Replace("}", string.Empty)}", LogEventKeys.DomainRepository);
            return await Task.FromResult(query);
        }

        //public async Task<IEnumerable<T>> GetAllAsync(int count = -1)
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<T>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
        //            .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
        //            .TakeIf(count)
        //            .AsEnumerable());
        //}

        ///// <summary>
        ///// Removes the underlying DocumentDB collection.
        ///// NOTE: Each time you create a collection, you incur a charge for at least one hour of use, as determined by the specified performance level of the collection.
        ///// If you create a collection and delete it within an hour, you are still charged for one hour of use
        ///// </summary>
        ///// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        //public async Task<bool> DeleteAllAsync()
        //{
        //    // TODO: this does not make sense when using a master collection
        //    if (!this.isMasterCollection)
        //    {
        //        var result = await this.client.DeleteDocumentCollectionAsync(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId)).AnyContext();

        //        bool isSuccess = result.StatusCode == HttpStatusCode.NoContent;

        //        this.documentCollection = new AsyncLazy<DocumentCollection>(async () => await this.GetOrCreateCollectionAsync().AnyContext());
        //        return isSuccess;
        //    }

        //    return false;
        //}

        //public async Task<int> CountAsync()
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<int>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            "SELECT VALUE COUNT(c) from c")
        //            // TODO: use discriminator in query (if ismastercollection)
        //            .AsEnumerable().First());
        //}

        //public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression = null) // FAST
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<T>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
        //            .WhereExpression(expression)
        //            .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
        //            .AsEnumerable()
        //            .FirstOrDefault());
        //}

        //public async Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> expression = null) // FAST
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<T>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
        //            .WhereExpression(expression)
        //            .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
        //            .AsEnumerable()
        //            .LastOrDefault());
        //}

        //public async Task<IQueryable<T>> QueryAsync(int count = 100)
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<T>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
        //            .WhereExpressionIf(e => e.Discriminator == typeof(T).FullName, this.isMasterCollection)
        //            .TakeIf(count));
        //}

        //public async Task<IEnumerable<T>> QueryAsync(string query)
        //{
        //    return await Task.FromResult(
        //        this.client.CreateDocumentQuery<T>(
        //            UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //            new SqlQuerySpec(query),
        //            new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned }));
        //}

        //public IEnumerable<T> GetAllBatched(int count = 100)
        //{
        //    var query = this.client.CreateDocumentQuery<Document>(
        //        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //        new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
        //        //.TakeIf(count)
        //        .AsDocumentQuery();

        //    while (query.HasMoreResults)
        //    {
        //        foreach (var entity in query.ExecuteNextAsync<T>().Result)
        //        {
        //            yield return entity;
        //        }
        //    }
        //}

        //public IEnumerable<string> GetAllIdsBatched(int count = 100)
        //{
        //    var query = this.client.CreateDocumentQuery<Document>(
        //        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //        new SqlQuerySpec("SELECT VALUE c.id FROM c"),
        //        // TODO: use TOP if pagesize > 0
        //        new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
        //        .AsDocumentQuery();

        //    while (query.HasMoreResults)
        //    {
        //        foreach (var id in query.ExecuteNextAsync().Result)
        //        {
        //            yield return id;
        //        }
        //    }
        //}

        //public async Task<IEnumerable<string>> GetAttachmentIdsAsync(string id)
        //{
        //    var doc = await this.GetDocumentById(id).AnyContext();
        //    if (doc == null)
        //    {
        //        return new List<string>();
        //    }

        //    var attachments = this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
        //    if (attachments != null)
        //    {
        //        return attachments.Select(a => a.Id);
        //    }

        //    return new List<string>();
        //}

        //public async Task<IEnumerable<Attachment>> GetAttachmentsAsync(string id)
        //{
        //    var doc = await this.GetDocumentById(id).AnyContext();
        //    if (doc == null)
        //    {
        //        return new List<Attachment>();
        //    }

        //    return this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
        //}

        //public async Task<Attachment> GetAttachmentByIdAsync(string id, string attachmentId)
        //{
        //    var doc = await this.GetDocumentById(id).AnyContext();
        //    if (doc == null)
        //    {
        //        return null;
        //    }

        //    var attachments = this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
        //    return attachments?.FirstOrDefault(a => a.Id.Equals(attachmentId, StringComparison.OrdinalIgnoreCase));
        //}

        //public async Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId)
        //{
        //    var attachment = await this.GetAttachmentByIdAsync(id, attachmentId).AnyContext();
        //    if (attachment == null)
        //    {
        //        return null;
        //    }

        //    var media = await this.client.ReadMediaAsync(attachment.MediaLink).AnyContext();
        //    return media?.Media;
        //}

        //public async Task<T> UpsertAttachmentAsync(T entity, string attachmentId, string contentType, Stream stream)
        //{
        //    stream.Position = 0;

        //    var doc = await this.client.UpsertDocumentAsync(
        //        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId),
        //        entity).AnyContext();
        //    await this.client.UpsertAttachmentAsync(doc.Resource.SelfLink, stream, new MediaOptions { ContentType = contentType, Slug = attachmentId }).AnyContext();
        //    return JsonConvert.DeserializeObject<T>(doc.Resource.ToString());
        //}

        //private async Task<Document> GetDocumentById(string id)
        //{
        //    try
        //    {
        //        return await this.client.ReadDocumentAsync(
        //            UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, id),
        //            new RequestOptions());
        //    }
        //    catch (DocumentClientException ex)
        //    {
        //        if (ex.Message.Contains("Resource Not Found"))
        //        {
        //            return null;
        //        }

        //        throw;
        //    }
        //}

        private async Task<DocumentCollection> GetOrCreateCollectionAsync(string partitionKey, int collectionOfferThroughput = 400)
        {
            var documentCollection = this.client.CreateDocumentCollectionQuery(
                UriFactory.CreateDatabaseUri(this.databaseId).ToString())
                .WhereExpression(c => c.Id == this.collectionId).AsEnumerable().FirstOrDefault();

            if (documentCollection == null)
            {
                documentCollection = new DocumentCollection { Id = this.collectionId };
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    documentCollection.PartitionKey.Paths.Add(string.Format("/{0}", partitionKey.Replace(".", "/").Trim('/')));
                }

                var requestOptions = new RequestOptions
                {
                    OfferThroughput = collectionOfferThroughput,
                };

                documentCollection = await this.client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(this.databaseId).ToString(),
                    documentCollection,
                    requestOptions).AnyContext();
            }

            return documentCollection;
        }

        private async Task<Database> GetOrCreateDatabaseAsync()
        {
            var result = this.client.CreateDatabaseQuery()
                .WhereExpression(db => db.Id == this.databaseId).AsEnumerable().FirstOrDefault();
            if (result == null)
            {
                result = await this.client.CreateDatabaseAsync(
                    new Database { Id = this.databaseId }).AnyContext();
            }

            return result;
        }

        //private async Task<string> GetCollectionUriAsync()
        //{
        //    return (await this.documentCollection).DocumentsLink;
        //    // TODO: instead use UriFactory.CreateCollectionUri(this.databaseId, this.collectionId);
        //}

        //private string TryGetIdProperty(Expression<Func<T, object>> idNameFactory)
        //{
        //    Type entityType = typeof(TEntity);
        //    var properties = entityType.GetProperties();

        //    if (idNameFactory != null)
        //    {
        //        var expr = this.GetMemberExpression(idNameFactory);
        //        var customPropertyInfo = expr.Member;

        //        this.EnsurePropertyHasJsonAttributeWithCorrectPropertyName(customPropertyInfo);

        //        return customPropertyInfo.Name;
        //    }

        //    // search for id property in entity
        //    var idProperty = properties.SingleOrDefault(p => p.Name == this.defaultIdentityPropertyName);

        //    if (idProperty != null)
        //    {
        //        return idProperty.Name;
        //    }

        //    // search for Id property in entity
        //    idProperty = properties.SingleOrDefault(p => p.Name == "Id");

        //    if (idProperty != null)
        //    {
        //        this.EnsurePropertyHasJsonAttributeWithCorrectPropertyName(idProperty);

        //        return idProperty.Name;
        //    }

        //    // identity property not found
        //    throw new ArgumentException("Unique identity property not found. Create \"id\" property for your entity or use different property name with JsonAttribute with PropertyName set to \"id\"");
        //}

        //private void EnsurePropertyHasJsonAttributeWithCorrectPropertyName(MemberInfo idProperty)
        //{
        //    var attributes = idProperty.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
        //    if (!(attributes.Length == 1
        //        && ((JsonPropertyAttribute)attributes[0]).PropertyName == this.defaultIdentityPropertyName))
        //    {
        //        throw new ArgumentException(
        //                string.Format(
        //                    "\"{0}\" property needs to be decorated with JsonAttirbute with PropertyName set to \"id\"",
        //                    idProperty.Name));
        //    }
        //}

        //private MemberExpression GetMemberExpression(Expression<Func<TEntity, object>> expr)
        //{
        //    var member = expr.Body as MemberExpression;
        //    var unary = expr.Body as UnaryExpression;
        //    return member ?? (unary != null ? unary.Operand as MemberExpression : null);
        //}

        //private DateTime TimeStampToDateTime(double ts)
        //    => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        //        .AddSeconds(ts).ToLocalTime();

        //public IEnumerable<string> GetAllUrisPaged(int pageSize = 100)
        //{
        //    var query = this.client.CreateDocumentQuery<Document>(
        //        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
        //        new SqlQuerySpec("SELECT VALUE c._self FROM c"),
        //        // TODO: use TOP if pagesize > 0
        //        new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = this.isPartitioned })
        //        .AsDocumentQuery();

        //    while (query.HasMoreResults)
        //    {
        //        foreach (var uri in query.ExecuteNextAsync().Result)
        //        {
        //            yield return uri;
        //        }
        //    }
        //}
    }
}
