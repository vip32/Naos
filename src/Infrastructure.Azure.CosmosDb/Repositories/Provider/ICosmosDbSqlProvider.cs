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

    public interface ICosmosDbSqlProvider<T>
        where T : class, IEntity
    {
        Task<T> UpsertAsync(T entity);

        Task<T> AddOrUpdateAttachmentAsync(T entity, string attachmentId, string contentType, Stream stream);

        Task<int> CountAsync();

        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        Task<IEnumerable<T>> GetAllAsync(int count = -1);

        IEnumerable<string> GetAllIdsPaged(int pageSize = 100);

        IEnumerable<T> GetAllPaged(int pageSize = 100);

        IEnumerable<string> GetAllUrisPaged(int pageSize = 100);

        Attachment GetAttachmentById(string id, string attachmentId);

        IEnumerable<string> GetAttachmentIds(string id);

        IEnumerable<Attachment> GetAttachments(string id);

        Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId);

        Task<T> GetByIdAsync(string id);

        Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        Task<IQueryable<T>> QueryAsync(int count = 100);

        Task<IEnumerable<T>> QueryAsync(string query);

        Task<bool> DeleteAllAsync();

        Task<bool> DeleteByIdAsync(string id);

        Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression);

        Task<IEnumerable<T>> WhereAsync<TKey>(Expression<Func<T, bool>> expression = null, IEnumerable<Expression<Func<T, bool>>> expressions = null, int count = 100, Expression<Func<T, TKey>> orderExpression = null, bool desc = false);

        Task<IEnumerable<T>> WhereAsync<TKey>(Expression<Func<T, bool>> expression, Expression<Func<T, T>> selector, int count = 100, Expression<Func<T, TKey>> orderExpression = null);
    }
}