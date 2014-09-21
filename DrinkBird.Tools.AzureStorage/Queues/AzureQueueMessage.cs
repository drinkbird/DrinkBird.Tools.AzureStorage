using System;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace DrinkBird.Tools.AzureStorage
{
    public abstract class AzureQueueMessage
    {
        
        [JsonIgnore]
        public IUpdateableAzureQueue UpdateableAzureQueueReference { get; set; } 

        [JsonIgnore]
        public CloudQueueMessage MessageReference { get; set; }
        
        public void Delete()
        {
            if (UpdateableAzureQueueReference == null)
            {
                throw new InvalidOperationException("QueueReference cannot return null");
            }

            UpdateableAzureQueueReference.DeleteMessage(this);
        }

        public void Update()
        {
            if (UpdateableAzureQueueReference == null)
            {
                throw new InvalidOperationException("QueueReference cannot return null");
            }

            UpdateableAzureQueueReference.UpdateMessage(this);
        }
    }
}
