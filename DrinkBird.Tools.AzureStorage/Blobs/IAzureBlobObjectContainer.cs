using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DrinkBird.Tools.AzureStorage
{
    public interface IAzureBlobObjectContainer<T> where T : class
    {
        void SetRetryPolicy(IRetryPolicy retryPolicy);
        T Get(string name);
        void AddOrReplace(T obj, string name);
        void Add(T obj, string name);
        void Delete(string name);
        DateTime LastModified(string name);
        long Size(string name);
        bool Exists(string name);
        List<string> ListNames();
    }
}