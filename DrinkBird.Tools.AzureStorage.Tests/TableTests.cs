using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace DrinkBird.Tools.AzureStorage.Tests
{
    [TestClass]
    public class TableTests
    {
        private void CleanupTable(string tableName)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var cloudTableClient = new CloudTableClient(account.TableEndpoint, account.Credentials);
            var table = cloudTableClient.GetTableReference(tableName);
            table.DeleteIfExists();
        }


        [TestMethod]
        public void Insert_Table_Entity()
        {
            CleanupTable("test1");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test1");
            

            var entity = new TestTableEntity
            {
                PartitionKey = "partitionkey",
                RowKey = Guid.NewGuid().ToString(),
                Name = "testname",
                Price = 10
            };

            testTable.Insert(entity);

            var retrievedEntities = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();
            Assert.AreEqual(1, retrievedEntities.Count());
        }

        [TestMethod]
        public void BulkInsert_Table_Entities()
        {
            CleanupTable("test3");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test3");
            

            var entities = new List<TestTableEntity>();

            for (var i = 0; i < 150; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 30; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey2",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 220; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey3",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            testTable.Insert(entities);

            var entitiesInTable = testTable.Query.ToList();

            Assert.AreEqual(400, entitiesInTable.Count());
        }
        
        [TestMethod]
        public void InsertOrMerge_Table_Entity()
        {
            CleanupTable("test4");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test4");
            


            var rowKey = Guid.NewGuid().ToString();

            var entity = new TestTableEntity
            {
                PartitionKey = "partitionkey",
                RowKey = rowKey,
                Name = "testname",
                Price = 10
            };

            testTable.Insert(entity);

            var retrievedEntities = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();

            Assert.AreEqual(1, retrievedEntities.Count());
            var retrievedEntity = retrievedEntities.First();
            Assert.AreEqual("partitionkey", retrievedEntity.PartitionKey);
            Assert.AreEqual(rowKey, retrievedEntity.RowKey);
            Assert.AreEqual("testname", retrievedEntity.Name);
            Assert.AreEqual(10, retrievedEntity.Price);


            var entity2 = new TestTableEntity
            {
                PartitionKey = "partitionkey",
                RowKey = rowKey,
                Name = "testname2",
                Price = 100
            };

            testTable.InsertOrMerge(entity2);

            var retrievedEntities2 = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();

            Assert.AreEqual(1, retrievedEntities2.Count());
            var retrievedEntity2 = retrievedEntities2.First();
            Assert.AreEqual("partitionkey", retrievedEntity2.PartitionKey);
            Assert.AreEqual(rowKey, retrievedEntity2.RowKey);
            Assert.AreEqual("testname2", retrievedEntity2.Name);
            Assert.AreEqual(100, retrievedEntity2.Price);

        }
        
        [TestMethod]
        public void Bulk_InsertOrMerge_Table_Entities()
        {
            CleanupTable("test5");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test5");
            

            var entities = new List<TestTableEntity>();

            for (var i = 0; i < 150; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 30; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey2",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 220; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey3",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            testTable.Insert(entities);


            var retrievedEntities = testTable.Query.ToList();
            Assert.AreEqual(10, retrievedEntities.Select(x => x.Price).Distinct().Single());
            Assert.AreEqual("testname", retrievedEntities.Select(x => x.Name).Distinct().Single());
            

            var entitiesToMerge = entities.Select(x => new TestTableEntity
            {
                PartitionKey = x.PartitionKey,
                RowKey = x.RowKey,
                Name = "new name",
                Price = 20
            });
            testTable.InsertOrMerge(entitiesToMerge);

            var retrievedEntities2 = testTable.Query.ToList();

            Assert.AreEqual(20, retrievedEntities2.Select(x => x.Price).Distinct().Single());
            Assert.AreEqual("new name", retrievedEntities2.Select(x => x.Name).Distinct().Single());
        }

        [TestMethod]
        public void Delete_Table_Entity()
        {
            CleanupTable("test6");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test6");
            

            var entity = new TestTableEntity
            {
                PartitionKey = "partitionkey",
                RowKey = Guid.NewGuid().ToString(),
                Name = "testname",
                Price = 10
            };

            testTable.Insert(entity);

            var retrievedEntities = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();
            Assert.AreEqual(1, retrievedEntities.Count);
            
            testTable.Delete(entity);

            var retrievedEntities2 = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();
            Assert.AreEqual(0, retrievedEntities2.Count);
        }

        [TestMethod]
        public void Bulk_Delete_Table_Entities()
        {
            CleanupTable("test7");

            IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test7");
            

            var entities = new List<TestTableEntity>();

            for (var i = 0; i < 150; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 30; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey2",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            for (var i = 0; i < 220; i++)
            {
                var entity = new TestTableEntity
                {
                    PartitionKey = "partitionkey3",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "testname",
                    Price = 10
                };
                entities.Add(entity);
            }
            testTable.Insert(entities);

            var retrievedEntities = testTable.Query.ToList();
            Assert.AreEqual(400, retrievedEntities.Count);
            
            testTable.Delete(entities);
            var retrievedEntities2 = testTable.Query.ToList();
            Assert.AreEqual(0, retrievedEntities2.Count);

        }
    }
}
