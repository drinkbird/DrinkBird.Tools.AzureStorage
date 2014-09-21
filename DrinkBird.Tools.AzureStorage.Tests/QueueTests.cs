using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;

namespace DrinkBird.Tools.AzureStorage.Tests
{
    [TestClass]
    public class AzureQueueTests
    {
        [TestMethod]
        public void Add_Queue_Message_Get_Message()
        {
            IAzureQueue<TestAzureQueueMessage> testMessageQueue = new AzureQueue<TestAzureQueueMessage>(CloudStorageAccount.DevelopmentStorageAccount, "test");

            testMessageQueue.Clear();

            var testMessage = new TestAzureQueueMessage
            {
                Name = "One",
                Value = 1
            };

            testMessageQueue.AddMessage(testMessage);

            var popTestMessage = testMessageQueue.GetMessage();
            popTestMessage.Delete();

            Assert.AreEqual(testMessage.Name, popTestMessage.Name);
            Assert.AreEqual(testMessage.Value, popTestMessage.Value);
        }

        [TestMethod]
        public void Add_Queue_Messages_Get_Messages_Less_Than_Storage_Batch_Limit()
        {
            IAzureQueue<TestAzureQueueMessage> testMessageQueue = new AzureQueue<TestAzureQueueMessage>(CloudStorageAccount.DevelopmentStorageAccount, "test2");
            
            testMessageQueue.Clear();

            var testMessage = new TestAzureQueueMessage
            {
                Name = "One",
                Value = 1
            };

            for (var i = 0; i < 20; i++)
            {
                testMessageQueue.AddMessage(testMessage);
            }


            var popTestMessages = testMessageQueue.GetMessages(20);

            Assert.AreEqual(20, popTestMessages.Count);
        }

        [TestMethod]
        public void Add_Queue_Messages_Get_Messages_More_Than_Storage_Batch_Limit()
        {
            IAzureQueue<TestAzureQueueMessage> testMessageQueue = new AzureQueue<TestAzureQueueMessage>(CloudStorageAccount.DevelopmentStorageAccount, "test3");
            
            testMessageQueue.Clear();

            var testMessage = new TestAzureQueueMessage
            {
                Name = "One",
                Value = 1
            };

            for (var i = 0; i < 120; i++)
            {
                testMessageQueue.AddMessage(testMessage);
            }


            var popTestMessages = testMessageQueue.GetMessages(120);
            
            Assert.AreEqual(120, popTestMessages.Count);
        }
    }
}
