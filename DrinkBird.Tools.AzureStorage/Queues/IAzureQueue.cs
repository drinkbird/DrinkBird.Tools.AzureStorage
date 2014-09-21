using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DrinkBird.Tools.AzureStorage
{
    public interface IAzureQueue<T> where T : AzureQueueMessage
    {
        void SetRetryPolicy(IRetryPolicy retryPolicy);
        void Clear();
        void AddMessage(T message);
        void DeleteMessage(T message);
        T GetMessage();
        List<T> GetMessages(int maxMessagesToReturn);
        int RetrieveApproximateMessageCount();
    }
}