namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    //using System.IO;
    //using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    //using Microsoft.Azure.Documents;

    public interface ICosmosDbSqlProvider<T>
    {
        Task<T> GetByIdAsync(string id);

        Task<T> UpsertAsync(T entity);

        Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression);

        Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression = null,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            int count = 100,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            int count = 100,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<bool> DeleteByIdAsync(string id);

        //Task<T> UpsertAttachmentAsync(T entity, string attachmentId, string contentType, Stream stream);

        //Task<int> CountAsync();

        //Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        //Task<IEnumerable<T>> GetAllAsync(int count = -1);

        //IEnumerable<string> GetAllIdsBatched(int count = 100);

        //IEnumerable<T> GetAllBatched(int count = 100);

        //Task<Attachment> GetAttachmentByIdAsync(string id, string attachmentId);

        //Task<IEnumerable<string>> GetAttachmentIdsAsync(string id);

        //Task<IEnumerable<Attachment>> GetAttachmentsAsync(string id);

        //Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId);

        //Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        //Task<IQueryable<T>> QueryAsync(int count = 100);

        //Task<IEnumerable<T>> QueryAsync(string query);

        //Task<bool> DeleteAllAsync();
    }
}