using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;

namespace DrinkBird.Tools.AzureStorage
{
    public class AzureQueue<T> : IAzureQueue<T>, IUpdateableAzureQueue where T : AzureQueueMessage
    {
        protected const int MaxAllowedQueueMessagesPerRequest = 32; //Set by azure storage policy - cannot be changed
        protected readonly TimeSpan MessageVisibilityTimeout;
        protected readonly CloudQueueClient Client;
        protected readonly CloudQueue Queue;

        public AzureQueue(CloudStorageAccount account) : this(account, typeof(T).Name.ToLowerInvariant()) { }

        public AzureQueue(CloudStorageAccount account, string queueName) : this(account, queueName, TimeSpan.FromSeconds(30)) { }

        public AzureQueue(CloudStorageAccount account, string queueName, TimeSpan visibilityTimeout)
        {
            MessageVisibilityTimeout = visibilityTimeout;
            Client = account.CreateCloudQueueClient();
            Client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5);
            Queue = Client.GetQueueReference(queueName.ToLower());
            Queue.CreateIfNotExists();
        }

        public void SetRetryPolicy(IRetryPolicy retryPolicy)
        {
            Client.DefaultRequestOptions.RetryPolicy = retryPolicy;
        }

        public void Clear()
        {
            Queue.Clear();
        }

        public void AddMessage(T message)
        {
            Queue.AddMessage(new CloudQueueMessage(GetSerializedMessage(message)));
        }

        public void DeleteMessage(T message)
        {
            var messageRef = message.MessageReference;
            if (messageRef == null)
            {
                throw new ArgumentException("Message reference cannot be null", "message");
            }
            Queue.DeleteMessage(messageRef.Id, messageRef.PopReceipt);
        }

        public void UpdateMessage(T message)
        {
            var messageRef = message.MessageReference;
            if (messageRef == null)
            {
                throw new ArgumentException("Message reference cannot be null", "message");
            }
            
            messageRef.SetMessageContent(GetSerializedMessage(message));
            Queue.UpdateMessage(messageRef, MessageVisibilityTimeout, MessageUpdateFields.Visibility | MessageUpdateFields.Content);
        }

        public T GetMessage()
        {
            var message = Queue.GetMessage(MessageVisibilityTimeout);
            return message == null
                ? default(T)
                : GetDeserializedMessage(message);
        }
        
        public int RetrieveApproximateMessageCount()
        {
            return Queue.ApproximateMessageCount ?? 0;
        }
        
        public List<T> GetMessages(int maxMessagesToReturn)
        {
            if (maxMessagesToReturn < 1)
            {
                throw new ArgumentException("You need to ask for at least 1 queue message", "maxMessagesToReturn");
            }

            List<CloudQueueMessage> retrievedMessages;

            if (maxMessagesToReturn <= MaxAllowedQueueMessagesPerRequest)
            {
                retrievedMessages = GetMessagesBatch(maxMessagesToReturn);
            }
            else
            {
                retrievedMessages = new List<CloudQueueMessage>();
                var numberOfBatches = Math.Ceiling(maxMessagesToReturn / (double)MaxAllowedQueueMessagesPerRequest);

                for (int i = 0; i < numberOfBatches; i++)
                {
                    var batchRetrievedMessages = GetMessagesBatch(MaxAllowedQueueMessagesPerRequest);
                    if (batchRetrievedMessages.Count == 0) break;
                    retrievedMessages.AddRange(batchRetrievedMessages);
                }
            }

            return retrievedMessages.Select(GetDeserializedMessage).ToList();
        }
        
        private List<CloudQueueMessage> GetMessagesBatch(int maxMessagesToReturn)
        {
            if (maxMessagesToReturn > MaxAllowedQueueMessagesPerRequest)
            {
                throw new ArgumentException(string.Format("You cannot ask for more than {0} messages per actual queue request", MaxAllowedQueueMessagesPerRequest), "maxMessagesToReturn");
            }

            return Queue.GetMessages(maxMessagesToReturn, MessageVisibilityTimeout).ToList();
        }

        private static string GetSerializedMessage(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        private T GetDeserializedMessage(CloudQueueMessage message)
        {
            var deserializedMessage = JsonConvert.DeserializeObject<T>(message.AsString);
            
            // set references (allows updating message)
            deserializedMessage.UpdateableAzureQueueReference = this;
            deserializedMessage.MessageReference = message;

            return deserializedMessage;
        }

        public void DeleteMessage(AzureQueueMessage message)
        {
            if (!(message is T))
            {
                throw new ArgumentException("Message should be instance of T", "message");
            }

            DeleteMessage(message as T);
        }

        public void UpdateMessage(AzureQueueMessage message)
        {
            if (!(message is T))
            {
                throw new ArgumentException("Message should be instance of T", "message");
            }

            var messageRef = message.MessageReference;
            if (messageRef == null)
            {
                throw new ArgumentException("Message reference cannot be null", "message");
            }

            messageRef.SetMessageContent(GetSerializedMessage(message as T));
            Queue.UpdateMessage(messageRef, MessageVisibilityTimeout, MessageUpdateFields.Visibility | MessageUpdateFields.Content);
        }
    }
}