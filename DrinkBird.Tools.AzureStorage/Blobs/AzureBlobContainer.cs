using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DrinkBird.Tools.AzureStorage
{
    public abstract class AzureBlobContainer<T> where T : class
    {
        protected readonly CloudBlobClient Client;
        protected readonly CloudBlobContainer Container;

        protected AzureBlobContainer(CloudStorageAccount account)
            : this(account, typeof (T).Name.ToLowerInvariant())
        {
        }

        protected AzureBlobContainer(CloudStorageAccount account, string containerName)
        {
            containerName = containerName.ToLower();
            Client = account.CreateCloudBlobClient();
            Client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5);
            Container = Client.GetContainerReference(containerName);
            Container.CreateIfNotExists(BlobContainerPublicAccessType.Off);
        }

        public bool Exists(string name)
        {
            CloudBlockBlob blob = Container.GetBlockBlobReference(name);
            return blob.Exists();
        }

        public void SetRetryPolicy(IRetryPolicy retryPolicy)
        {
            Client.DefaultRequestOptions.RetryPolicy = retryPolicy;
        }

        public abstract T Get(string name);
        public abstract void AddOrReplace(T obj, string name);
        public abstract void Add(T obj, string name);
        public abstract void Delete(string name);
        public abstract DateTime LastModified(string name);
        public abstract long Size(string name);
        public abstract List<string> ListNames();
    }
}
