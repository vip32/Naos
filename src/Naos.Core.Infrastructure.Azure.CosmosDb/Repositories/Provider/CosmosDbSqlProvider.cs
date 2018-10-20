namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class CosmosDbSqlProvider<TEntity> : ICosmosDbSqlProvider<TEntity>
        where TEntity : class, IEntity
    {
        private readonly IDocumentClient client;
        private readonly string databaseId;
        private readonly AsyncLazy<Database> database;
        private readonly string collectionName;
        private readonly string defaultIdentityPropertyName = "id";
        //private readonly Action<TEntity, string> setEtagAction;
        //private readonly Action<TEntity, DateTime?> setTimestampAction;
        private readonly bool isMasterCollection;
        private readonly bool isPartitioned;
        private AsyncLazy<DocumentCollection> documentCollection;

        public CosmosDbSqlProvider(
            IDocumentClient client,
            string databaseId,
            Func<string> collectionNameFactory = null,
            string collectionPartitionKey = null,
            int collectionOfferThroughput = 400,
            //Expression<Func<TEntity, object>> idNameFactory = null,
            //Expression<Func<TEntity, string>> etagExpression = null,
            //Expression<Func<TEntity, DateTime?>> timestampExpression = null,
            bool isMasterCollection = false)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrEmpty(databaseId, nameof(databaseId));

            this.client = client;
            this.databaseId = databaseId;
            this.database = new AsyncLazy<Database>(async () => await this.GetOrCreateDatabaseAsync().ConfigureAwait(false));

            this.isMasterCollection = isMasterCollection;
            if (collectionNameFactory != null)
            {
                this.collectionName = collectionNameFactory();
            }
            else
            {
                this.collectionName = isMasterCollection ? "master" : typeof(TEntity).Name;
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

            //if (etagExpression != null)
            //{
            //    etagExpression.Compile();
            //    this.setEtagAction = etagExpression.ToSetAction();
            //}

            //if (timestampExpression != null)
            //{
            //    timestampExpression.Compile();
            //    this.setTimestampAction = timestampExpression.ToSetAction();
            //}
        }

        public async Task<string> GetCollectionUriAsync()
        {
            return (await this.documentCollection).DocumentsLink;
        }

        /// <summary>
        /// Removes the underlying DocumentDB collection.
        /// NOTE: Each time you create a collection, you incur a charge for at least one hour of use, as determined by the specified performance level of the collection.
        /// If you create a collection and delete it within an hour, you are still charged for one hour of use
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAllAsync()
        {
            var result = await this.client.DeleteDocumentCollectionAsync((await this.documentCollection).SelfLink).ConfigureAwait(false);

            bool isSuccess = result.StatusCode == HttpStatusCode.NoContent;

            this.documentCollection = new AsyncLazy<DocumentCollection>(async () => await this.GetOrCreateCollectionAsync().ConfigureAwait(false));

            return isSuccess;
        }

        public async Task<bool> DeleteAsync(string idOrUri)
        {
            if (string.IsNullOrEmpty(idOrUri))
            {
                return false;
            }

            bool isSuccess;
            var selfLink = idOrUri;

            if (!idOrUri.StartsWith("dbs/", StringComparison.OrdinalIgnoreCase))
            {
                var doc = await this.GetDocumentByIdAsync(idOrUri).ConfigureAwait(false);
                if (doc != null)
                {
                    selfLink = doc.SelfLink;
                }
                else
                {
                    return false;
                }
            }

            var result = await this.client.DeleteDocumentAsync(selfLink).ConfigureAwait(false);

            isSuccess = result.StatusCode == HttpStatusCode.NoContent;

            return isSuccess;
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            var doc = await this.client.UpsertDocumentAsync((await this.documentCollection).SelfLink, entity).ConfigureAwait(false);

            var retVal = JsonConvert.DeserializeObject<TEntity>(doc.Resource.ToString());
            //if (retVal != null)
            //{
            //    this.setEtagAction?.Invoke(retVal, doc.Resource.ETag);
            //    this.setTimestampAction?.Invoke(retVal, doc.Resource.Timestamp);
            //}

            return retVal;
        }

        public async Task<TEntity> AddOrUpdateAttachmentAsync(TEntity entity, string attachmentId, string contentType, Stream stream)
        {
            stream.Position = 0;

            var doc = await this.client.UpsertDocumentAsync((await this.documentCollection).SelfLink, entity).ConfigureAwait(false);
            await this.client.UpsertAttachmentAsync(doc.Resource.SelfLink, stream, new MediaOptions { ContentType = contentType, Slug = attachmentId }).ConfigureAwait(false);

            var retVal = JsonConvert.DeserializeObject<TEntity>(doc.Resource.ToString());
            //if (retVal != null)
            //{
            //    this.setEtagAction?.Invoke(retVal, doc.Resource.ETag);
            //    this.setTimestampAction?.Invoke(retVal, doc.Resource.Timestamp);
            //}

            return retVal;
        }

        public async Task<int> CountAsync()
        {
            var retVal = this.client.CreateDocumentQuery<int>(
                await this.GetCollectionUriAsync().ConfigureAwait(false),
                "SELECT VALUE COUNT(c) from c")
                .AsEnumerable().First();

            return retVal;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(int maxItemCount = -1)
        {
            return this.client.CreateDocumentQuery<Document>((await this.documentCollection).SelfLink, new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = this.isPartitioned })
                .AsEnumerable()
                .Select(doc => new {doc, retVal = (TEntity)(dynamic)doc })
                //.ForEach(e =>
                //{
                //    this.setEtagAction?.Invoke(e.retVal, e.doc.ETag);
                //    this.setTimestampAction?.Invoke(e.retVal, e.doc.Timestamp);
                //})
                .Select(e => e.retVal);
        }

        public IEnumerable<TEntity> GetAllPaged(int pageSize = 100)
        {
            var query = this.client.CreateDocumentQuery<Document>(
                this.GetCollectionUriAsync().Result,
                new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = this.isPartitioned })
                .AsDocumentQuery();
            while (query.HasMoreResults)
            {
                var docs = query.ExecuteNextAsync().Result;

                foreach (var retVal in docs.Select(doc => new { doc, retVal = (TEntity)doc })
                        //.ForEach(e =>
                        //{
                        //    this.setEtagAction?.Invoke(e.retVal, e.doc._etag);
                        //    this.setTimestampAction?.Invoke(e.retVal, this.TimeStampToDateTime(e.doc._ts));
                        //})
                    .Select(e => e.retVal))
                {
                    yield return retVal;
                }
            }
        }

        public IEnumerable<string> GetAllIdsPaged(int pageSize = 100)
        {
            var query = this.client.CreateDocumentQuery<Document>(
                this.GetCollectionUriAsync().Result,
                new SqlQuerySpec("SELECT VALUE c.id FROM c"),
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
                this.GetCollectionUriAsync().Result,
                new SqlQuerySpec("SELECT VALUE c._self FROM c"),
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
            var doc = await this.GetDocumentByIdAsync(id).ConfigureAwait(false);
            var retVal = (TEntity)(dynamic)doc;
            //if (doc != null)
            //{
            //    this.setEtagAction?.Invoke(retVal, doc.ETag);
            //    this.setTimestampAction?.Invoke(retVal, doc.Timestamp);
            //}

            return retVal;
        }

        public async Task<TEntity> GetByEtagAsync(string etag)
        {
            var doc = await this.GetDocumentByEtagAsync(etag).ConfigureAwait(false);
            var retVal = (TEntity)(dynamic)doc;
            //if (doc != null)
            //{
            //    this.setEtagAction?.Invoke(retVal, doc.ETag);
            //    this.setTimestampAction?.Invoke(retVal, doc.Timestamp);
            //}

            return retVal;
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
            return media != null ? media.Media : null;
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null) // FAST
        {
            return this.client.CreateDocumentQuery<TEntity>(
                await this.GetCollectionUriAsync().ConfigureAwait(false),
                new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(expression)
                .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public async Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null) // FAST
        {
            return this.client.CreateDocumentQuery<TEntity>(
                await this.GetCollectionUriAsync().ConfigureAwait(false),
                new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(expression)
                .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName)
                .AsEnumerable()
                .LastOrDefault();
        }

        public async Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> expression) // TODO: shouldn't this return IEnumerable<T>?
        {
            if (!this.isMasterCollection)
            {
                return this.client.CreateDocumentQuery<TEntity>(
                    await this.GetCollectionUriAsync().ConfigureAwait(false),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .AsEnumerable();
            }

            return this.client.CreateDocumentQuery<TEntity>(
                await this.GetCollectionUriAsync().ConfigureAwait(false),
                    new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(expression)
                .WhereExpression(e => e.EntityType == typeof(TEntity).FullName)
                .AsEnumerable();
        }

        public async Task<IEnumerable<TEntity>> WhereAsync<TKey>(
            Expression<Func<TEntity, bool>> expression = null,
            IEnumerable<Expression<Func<TEntity, bool>>> expressions = null,
            int maxItemCount = 100,
            Expression<Func<TEntity, TKey>> orderExpression = null,
            bool desc = false)
        {
            if (desc)
            {
                return this.client.CreateDocumentQuery<TEntity>((await this.documentCollection).SelfLink, new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressions(expressions)
                    .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName)
                    .OrderByDescending(orderExpression)
                    .AsEnumerable();
            }

            return this.client.CreateDocumentQuery<TEntity>((await this.documentCollection).SelfLink, new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = this.isPartitioned })
                    .WhereExpression(expression)
                    .WhereExpressions(expressions)
                    .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName)
                    .OrderBy(orderExpression)
                    .AsEnumerable();
        }

        public async Task<IEnumerable<TEntity>> WhereAsync<TKey>(
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, TEntity>> selector,
            int maxItemCount = 100,
            Expression<Func<TEntity, TKey>> orderExpression = null)
        {
            return this.client.CreateDocumentQuery<TEntity>((await this.documentCollection).SelfLink, new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(expression)
                .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName)
                .Select(selector)
                .OrderBy(orderExpression)
                .AsEnumerable();
        }

        public async Task<IQueryable<TEntity>> QueryAsync(int maxItemCount = 100)
        {
            return this.client.CreateDocumentQuery<TEntity>((await this.documentCollection).SelfLink, new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpressionIf(this.isMasterCollection, e => e.EntityType == typeof(TEntity).FullName);
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(string query)
        {
            return this.client.CreateDocumentQuery<TEntity>(
                await this.GetCollectionUriAsync().ConfigureAwait(false),
                new SqlQuerySpec(query),
                new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned });
        }

        //private string TryGetIdProperty(Expression<Func<TEntity, object>> idNameFactory)
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

        private void EnsurePropertyHasJsonAttributeWithCorrectPropertyName(MemberInfo idProperty)
        {
            var attributes = idProperty.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            if (!(attributes.Length == 1
                && ((JsonPropertyAttribute)attributes[0]).PropertyName == this.defaultIdentityPropertyName))
            {
                throw new ArgumentException(
                        string.Format(
                            "\"{0}\" property needs to be decorated with JsonAttirbute with PropertyName set to \"id\"",
                            idProperty.Name));
            }
        }

        private async Task<Document> GetDocumentByIdAsync(object id)
        {
            return this.client.CreateDocumentQuery<Document>((await this.documentCollection).SelfLink, new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(d => d.Id == id.ToString()).AsEnumerable().FirstOrDefault();
        }

        private async Task<Document> GetDocumentByEtagAsync(string etag)
        {
            return this.client.CreateDocumentQuery<Document>((await this.documentCollection).SelfLink, new FeedOptions { EnableCrossPartitionQuery = this.isPartitioned })
                .WhereExpression(d => d.ETag == etag).AsEnumerable().FirstOrDefault();
        }

        private async Task<DocumentCollection> GetOrCreateCollectionAsync(string collectionPartitionKey = null, int collectionOfferThroughput = 400)
        {
            var documentCollection = this.client.CreateDocumentCollectionQuery((await this.database).SelfLink)
                .WhereExpression(c => c.Id == this.collectionName).AsEnumerable().FirstOrDefault();

            if (documentCollection == null)
            {
                documentCollection = new DocumentCollection { Id = this.collectionName };
                if (!string.IsNullOrEmpty(collectionPartitionKey))
                {
                    documentCollection.PartitionKey.Paths.Add(string.Format("/{0}", collectionPartitionKey.Replace(".", "/")));
                }

                var requestOptions = new RequestOptions
                {
                    OfferThroughput = collectionOfferThroughput,
                };

                documentCollection = await this.client.CreateDocumentCollectionAsync((await this.database).SelfLink, documentCollection, requestOptions).ConfigureAwait(false);
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

        private MemberExpression GetMemberExpression(Expression<Func<TEntity, object>> expr)
        {
            var member = expr.Body as MemberExpression;
            var unary = expr.Body as UnaryExpression;
            return member ?? (unary != null ? unary.Operand as MemberExpression : null);
        }

        private DateTime TimeStampToDateTime(double ts)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(ts).ToLocalTime();
    }
}
