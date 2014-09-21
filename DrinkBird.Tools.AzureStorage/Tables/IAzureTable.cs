using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace DrinkBird.Tools.AzureStorage
{
    public interface IAzureTable<T> where T : TableEntity
    {
        void SetRetryPolicy(IRetryPolicy retryPolicy);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void InsertOrMerge(T entity);
        void InsertOrMerge(IEnumerable<T> entities);
        void InsertOrReplace(T entity);
        void InsertOrReplace(IEnumerable<T> entities);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        IQueryable<T> Query { get; }
    }
}
