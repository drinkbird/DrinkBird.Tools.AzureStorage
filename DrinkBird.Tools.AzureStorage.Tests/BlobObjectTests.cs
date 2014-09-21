using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;

namespace DrinkBird.Tools.AzureStorage.Tests
{
    [TestClass]
    public class BlobObjectTests
    {
        [TestMethod]
        public void Add_Blob_Object()
        {
            IAzureBlobObjectContainer<TestBlobObject> testObjectContainer = new AzureBlobObjectContainer<TestBlobObject>(CloudStorageAccount.DevelopmentStorageAccount, "test");
            
            var testMessage = new TestBlobObject
            {
                Name = "Two",
                Price = 250
            };

            testObjectContainer.AddOrReplace(testMessage, "testmessage1");

            var retrieved = testObjectContainer.Get("testmessage1");

            Assert.AreEqual("Two", retrieved.Name);
            Assert.AreEqual(250, retrieved.Price);
        }

        [TestMethod]
        public void Blob_Object_Exists()
        {
            IAzureBlobObjectContainer<TestBlobObject> testObjectContainer = new AzureBlobObjectContainer<TestBlobObject>(CloudStorageAccount.DevelopmentStorageAccount, "test2");

            var testMessage = new TestBlobObject
            {
                Name = "Two",
                Price = 250
            };

            testObjectContainer.AddOrReplace(testMessage, "testmessage2");

            var exists = testObjectContainer.Exists("testmessage2");

            Assert.AreEqual(true, exists);
        }

        [TestMethod]
        public void List_Blob_Object()
        {
            IAzureBlobObjectContainer<TestBlobObject> testObjectContainer = new AzureBlobObjectContainer<TestBlobObject>(CloudStorageAccount.DevelopmentStorageAccount, "test3");

            var testMessage = new TestBlobObject
            {
                Name = "Two",
                Price = 250
            };

            testObjectContainer.AddOrReplace(testMessage, "testmessage0/test/test1/test2");
            testObjectContainer.AddOrReplace(testMessage, "testmessage1");
            testObjectContainer.AddOrReplace(testMessage, "testmessage2");

            var names = testObjectContainer.ListNames();
            Assert.AreEqual(3, names.Count);
        }


    }
}
