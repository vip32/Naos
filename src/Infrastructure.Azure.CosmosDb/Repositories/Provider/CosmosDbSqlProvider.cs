namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class CosmosDbSqlProvider<TEntity> : ICosmosDbSqlProvider<TEntity>
        where TEntity : class, IEntity, IDiscriminatedEntity
    {
        private readonly IDocumentClient client;
        private readonly string databaseId;
        private readonly AsyncLazy<Database> database;
        private readonly string collectionId;
        //private readonly string defaultIdentityPropertyName = "id";
        private readonly bool isMasterCollection;
        private readonly bool isPartitioned;
        private AsyncLazy<DocumentCollection> documentCollection;

        public CosmosDbSqlProvider(
            IDocumentClient client,
            string databaseId,
            Func<string> collectionIdFactory = null,
            string collectionPartitionKey = null,
            int collectionOfferThroughput = 400,
            //Expression<Func<T, object>> idNameFactory = null,
            bool isMasterCollection = true)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrEmpty(databaseId, nameof(databaseId));

            this.client = client;
            this.databaseId = databaseId;
            this.database = new AsyncLazy<Database>(async () => await this.GetOrCreateDatabaseAsync().ConfigureAwait(false));

            this.isMasterCollection = isMasterCollection;
            if (collectionIdFactory != null)
            {
                this.collectionId = collectionIdFactory();
            }

            if (string.IsNullOrEmpty(this.collectionId))
            {
                this.collectionId = isMasterCollection ? "master" : typeof(TEntity).Name;
            }

            this.documentCollection = new AsyncLazy<DocumentCollection>(async () => await this.GetOrCreateCollectionAsync(collectionPartitionKey, collectionOfferThroughput).ConfigureAwait(false));
            if (this.documentCollection.Value.Result?.PartitionKey?.Paths?.Any() == true)
            {
                this.isPartitioned = true;
            }

            //if (idNameFactory != null)
            //{
            //    this.TryGetIdProperty(idNameFactory);
            //}
        }

        /// <summary>
        /// Removes the underlying DocumentDB collection.
        /// NOTE: Each time you create a collection, you incur a charge for at least one hour of use, as determined by the specified performance level of the collection.
        /// If you create a collection and delete it within an hour, you are still charged for one hour of use
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAllAsync()
        {
            // TODO: this does not make sense when using a master collection
            if (!this.isMasterCollection)
            {
                var result = await this.client.DeleteDocumentCollectionAsync((await this.documentCollection).SelfLink).ConfigureAwait(false);

                bool isSuccess = result.StatusCode == HttpStatusCode.NoContent;

                this.documentCollection = new AsyncLazy<DocumentCollection>(async () => await this.GetOrCreateCollectionAsync().ConfigureAwait(false));

                return isSuccess;
            }

            return false;
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var result = await this.client.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, id)).ConfigureAwait(false);

            return result.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<TEntity> UpsertAsync(TEntity entity)
        {
            var doc = await this.client.UpsertDocumentAsync((await this.documentCollection).SelfLink, entity).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TEntity>(doc.Resource.ToString());
        }

        public async Task<TEntity> AddOrUpdateAttachmentAsync(TEntity entity, string attachmentId, string contentType, Stream stream)
        {
            stream.Position = 0;

            var doc = await this.client.UpsertDocumentAsync((await this.documentCollection).SelfLink, entity).ConfigureAwait(false);
            await this.client.UpsertAttachmentAsync(doc.Resource.SelfLink, stream, new MediaOptions { ContentType = contentType, Slug = attachmentId }).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TEntity>(doc.Resource.ToString());
        }

        public async Task<int> CountAsync()
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<int>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    "SELECT VALUE COUNT(c) from c")
                    .AsEnumerable().First());
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(int count = -1)
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<Document>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
                    .TakeIf(count)
                    .AsEnumerable()
                    .Select(doc => new { doc, retVal = (TEntity)(dynamic)doc })
                    .Select(e => e.retVal));
        }

        public IEnumerable<TEntity> GetAllPaged(int pageSize = 100)
        {
            var query = this.client.CreateDocumentQuery<Document>(
                UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = this.isPartitioned })
                .TakeIf(pageSize)
                .AsDocumentQuery();
            while (query.HasMoreResults)
            {
                var docs = query.ExecuteNextAsync().Result;

                foreach (var retVal in docs
                    .Select(doc => new { doc, retVal = (TEntity)doc }).Select(e => e.retVal))
                {
                    yield return retVal;
                }
            }
        }

        public IEnumerable<string> GetAllIdsPaged(int pageSize = 100)
        {
            var query = this.client.CreateDocumentQuery<Document>(
                UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                new SqlQuerySpec("SELECT VALUE c.id FROM c"),
                // TODO: use TOP if pagesize > 0
                new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = this.isPartitioned })
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (var id in query.ExecuteNextAsync().Result)
                {
                    yield return id;
                }
            }
        }

        public IEnumerable<string> GetAllUrisPaged(int pageSize = 100)
        {
            var query = this.client.CreateDocumentQuery<Document>(
                UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                new SqlQuerySpec("SELECT VALUE c._self FROM c"),
                // TODO: use TOP if pagesize > 0
                new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = this.isPartitioned })
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (var uri in query.ExecuteNextAsync().Result)
                {
                    yield return uri;
                }
            }
        }

        public async Task<TEntity> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await this.client.ReadDocumentAsync<TEntity>(
                UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, id),
                new RequestOptions());
        }

        public IEnumerable<string> GetAttachmentIds(string id)
        {
            var doc = this.GetDocumentByIdAsync(id).Result;
            if (doc == null)
            {
                return new List<string>();
            }

            var attachments = this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
            if (attachments != null)
            {
                return attachments.Select(a => a.Id);
            }

            return new List<string>();
        }

        public IEnumerable<Attachment> GetAttachments(string id)
        {
            var doc = this.GetDocumentByIdAsync(id).Result;
            if (doc == null)
            {
                return new List<Attachment>();
            }

            return this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
        }

        public Attachment GetAttachmentById(string id, string attachmentId)
        {
            var doc = this.GetDocumentByIdAsync(id).Result;
            if (doc == null)
            {
                return null;
            }

            var attachments = this.client.CreateAttachmentQuery(doc.SelfLink).AsEnumerable();
            return attachments?.FirstOrDefault(a => a.Id.Equals(attachmentId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId)
        {
            var attachment = this.GetAttachmentById(id, attachmentId);
            if (attachment == null)
            {
                return null;
            }

            var media = await this.client.ReadMediaAsync(attachment.MediaLink).ConfigureAwait(false);
            return media?.Media;
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null) // FAST
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                    .AsEnumerable()
                    .FirstOrDefault());
        }

        public async Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null) // FAST
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                    .AsEnumerable()
                    .LastOrDefault());
        }

        public async Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> expression) // TODO: shouldn't this return IEnumerable<T>?
        {
            if (!this.isMasterCollection)
            {
                return await Task.FromResult(
                    this.client.CreateDocumentQuery<TEntity>(
                        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                        new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                        .WhereExpression(expression)
                        .AsEnumerable());
            }

            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpression(e => e.Discriminator == typeof(TEntity).FullName)
                    .AsEnumerable());
        }

        public async Task<IEnumerable<TEntity>> WhereAsync<TKey>(
            Expression<Func<TEntity, bool>> expression = null,
            IEnumerable<Expression<Func<TEntity, bool>>> expressions = null,
            int count = 100,
            Expression<Func<TEntity, TKey>> orderExpression = null,
            bool desc = false)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            if (desc)
            {
                return await Task.FromResult(
                    this.client.CreateDocumentQuery<TEntity>(
                        UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                        new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
                        .WhereExpression(expression)
                        .WhereExpressions(expressions)
                        .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                        .TakeIf(count)
                        .OrderByDescending(orderExpression)
                        .AsEnumerable());
            }

            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressions(expressions)
                    .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                    .TakeIf(count)
                    .OrderBy(orderExpression)
                    .AsEnumerable());
        }

        public async Task<IEnumerable<TEntity>> WhereAsync<TKey>(
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, TEntity>> selector,
            int count = 100,
            Expression<Func<TEntity, TKey>> orderExpression = null)
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                    .Select(selector)
                    .TakeIf(count)
                    .OrderBy(orderExpression)
                    .AsEnumerable());
        }

        public async Task<IQueryable<TEntity>> QueryAsync(int count = 100)
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { MaxItemCount = count, EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpressionIf(e => e.Discriminator == typeof(TEntity).FullName, this.isMasterCollection)
                    .TakeIf(count));
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(string query)
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new SqlQuerySpec(query),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned }));
        }

        private async Task<Document> GetDocumentByIdAsync(object id)
        {
            return await Task.FromResult(
                this.client.CreateDocumentQuery<Document>(
                    UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId).ToString(),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(d => d.Id == id.ToString()).AsEnumerable().FirstOrDefault());
        }

        private async Task<DocumentCollection> GetOrCreateCollectionAsync(string collectionPartitionKey = null, int collectionOfferThroughput = 400)
        {
            var documentCollection = this.client.CreateDocumentCollectionQuery(
                UriFactory.CreateDatabaseUri(this.databaseId).ToString())
                .WhereExpression(c => c.Id == this.collectionId).AsEnumerable().FirstOrDefault();

            if (documentCollection == null)
            {
                documentCollection = new DocumentCollection { Id = this.collectionId };
                if (!string.IsNullOrEmpty(collectionPartitionKey))
                {
                    documentCollection.PartitionKey.Paths.Add(string.Format("/{0}", collectionPartitionKey.Replace(".", "/")));
                }

                var requestOptions = new RequestOptions
                {
                    OfferThroughput = collectionOfferThroughput,
                };

                documentCollection = await this.client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(this.databaseId).ToString(),
                    documentCollection,
                    requestOptions).ConfigureAwait(false);
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
                    new Database { Id = this.databaseId }).ConfigureAwait(false);
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
    }
}
