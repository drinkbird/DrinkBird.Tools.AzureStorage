using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;

namespace DrinkBird.Tools.AzureStorage
{
    public class AzureTable<T> : IAzureTable<T> where T : TableEntity, new()
    {
        protected const int MaxAllowedTableEntitiesPerBatch = 100; //Set by azure storage policy - cannot be changed
        protected readonly CloudTableClient Client;
        protected readonly CloudTable Table;

        public AzureTable(CloudStorageAccount account)
            : this(account, typeof(T).Name)
        {
        }

        public AzureTable(CloudStorageAccount account, string tableName)
        {
            tableName = tableName.ToLower();
            Client = account.CreateCloudTableClient();
            Client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5);
            
            Table = Client.GetTableReference(tableName);
            Table.CreateIfNotExists();
        }

        public void SetRetryPolicy(IRetryPolicy retryPolicy)
        {
            Client.DefaultRequestOptions.RetryPolicy = retryPolicy;
        }

        public void Insert(T entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            Table.Execute(insertOperation);
        }

        public void Insert(IEnumerable<T> entities)
        {
            BulkOperation(entities, TableOperation.Insert);
        }

        public void InsertOrMerge(T entity)
        {
            var addOrUpdateOperation = TableOperation.InsertOrMerge(entity);
            Table.Execute(addOrUpdateOperation);
        }

        public void InsertOrMerge(IEnumerable<T> entities)
        {
            BulkOperation(entities, TableOperation.InsertOrMerge);
        }

        public void InsertOrReplace(T entity)
        {
            var addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            Table.Execute(addOrUpdateOperation);
        }

        public void InsertOrReplace(IEnumerable<T> entities)
        {
            BulkOperation(entities, TableOperation.InsertOrReplace);
        }

        public void Delete(T entity)
        {
            var deleteOperation = TableOperation.Delete(entity);
            Table.Execute(deleteOperation);
        }

        public void Delete(IEnumerable<T> entities)
        {
            BulkOperation(entities, TableOperation.Delete);
        }

        public IQueryable<T> Query
        {
            get { return Table.CreateQuery<T>().AsTableQuery(); }
        }
        
        /// <summary>
        /// Breaks down the bulk operation into multiple batch operations to satisfy the constraints of Azure Table Storage engine
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="operation"></param>
        private void BulkOperation(IEnumerable<T> entities, Func<ITableEntity, TableOperation> operation)
        {
            if (entities == null) throw new ArgumentException("The argument cannot be null", "entities");
            var entitiesList = entities as IList<T> ?? entities.ToList(); //Avoid enumerating the IEnumerable multiple times
            if (entitiesList.Count < 1) throw new ArgumentException("The batch operation should contain at least one entity", "entities");

            var partitions = entitiesList.GroupBy(x => x.PartitionKey);
            foreach (var partition in partitions)
            {
                if (partition.Count() > MaxAllowedTableEntitiesPerBatch)
                {
                    var batchesInPartition = (int)Math.Ceiling((double)partition.Count() / MaxAllowedTableEntitiesPerBatch);

                    for (var i = 0; i < batchesInPartition; i++)
                    {
                        BatchOperation(partition.Skip(i * MaxAllowedTableEntitiesPerBatch).Take(MaxAllowedTableEntitiesPerBatch), operation);
                    }
                }
                else
                {
                    BatchOperation(partition, operation);
                }
            }
        }
        
        /// <summary>
        /// Represents an Azure Storage Table batch operation with a specific limit of entities and a common partition key.
        /// Called by BulkOperation method only.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="operation"></param>
        private void BatchOperation(IEnumerable<T> entities, Func<ITableEntity, TableOperation> operation)
        {
            if (entities == null) throw new ArgumentException("The argument cannot be null", "entities");
            var entitiesList = entities as IList<T> ?? entities.ToList(); //Avoid enumerating the IEnumerable multiple times
            if (entitiesList.Count > MaxAllowedTableEntitiesPerBatch) throw new ArgumentException("Number of batch objects cannot exceed " + MaxAllowedTableEntitiesPerBatch, "entities");
            if (entitiesList.Count < 1) throw new ArgumentException("The batch operation should contain at least one entity", "entities");
            if (entitiesList.Select(x => x.PartitionKey).Distinct().Count() > 1) throw new ArgumentException("All partition keys of the batch items need to be the same", "entities");


            var batchAddOperation = new TableBatchOperation();
            foreach (var entity in entitiesList)
            {
                batchAddOperation.Add(operation(entity));
            }
            Table.ExecuteBatch(batchAddOperation);
        }
    }
}
