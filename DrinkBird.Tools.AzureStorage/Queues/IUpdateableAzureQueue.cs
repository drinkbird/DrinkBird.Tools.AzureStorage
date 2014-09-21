namespace DrinkBird.Tools.AzureStorage
{
    public interface IUpdateableAzureQueue
    {
        void DeleteMessage(AzureQueueMessage message);
        void UpdateMessage(AzureQueueMessage message);
    }
}
