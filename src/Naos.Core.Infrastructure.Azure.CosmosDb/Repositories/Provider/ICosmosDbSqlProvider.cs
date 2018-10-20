namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Naos.Core.Domain;

    public interface ICosmosDbSqlProvider<TEntity>
        where TEntity : class, IEntity
    {
        Task<TEntity> AddOrUpdateAsync(TEntity entity);

        Task<TEntity> AddOrUpdateAttachmentAsync(TEntity entity, string attachmentId, string contentType, Stream stream);

        Task<int> CountAsync();

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null);

        Task<IEnumerable<TEntity>> GetAllAsync(int maxItemCount = -1);

        IEnumerable<string> GetAllIdsPaged(int pageSize = 100);

        IEnumerable<TEntity> GetAllPaged(int pageSize = 100);

        IEnumerable<string> GetAllUrisPaged(int pageSize = 100);

        Attachment GetAttachmentById(string id, string attachmentId);

        IEnumerable<string> GetAttachmentIds(string id);

        IEnumerable<Attachment> GetAttachments(string id);

        Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId);

        Task<TEntity> GetByEtagAsync(string etag);

        Task<TEntity> GetByIdAsync(string id);

        Task<string> GetCollectionUriAsync();

        Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> expression = null);

        Task<IQueryable<TEntity>> QueryAsync(int maxItemCount = 100);

        Task<IEnumerable<TEntity>> QueryAsync(string query);

        Task<bool> DeleteAllAsync();

        Task<bool> DeleteAsync(string idOrUri);

        Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<TEntity>> WhereAsync<TKey>(Expression<Func<TEntity, bool>> expression = null, IEnumerable<Expression<Func<TEntity, bool>>> expressions = null, int maxItemCount = 100, Expression<Func<TEntity, TKey>> orderExpression = null, bool desc = false);

        Task<IEnumerable<TEntity>> WhereAsync<TKey>(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, TEntity>> selector, int maxItemCount = 100, Expression<Func<TEntity, TKey>> orderExpression = null);
    }
}