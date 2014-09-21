![DrinkBird Logo](http://blog.drinkbird.com/assets/media/db.png)

# DrinkBird.Tools.AzureStorage

>Communicate with Azure Storage at a higher level.

## Install via NuGet
>PM> Install-Package DrinkBird.Tools.AzureStorage

**In case that you don't want to communicate with a real Azure Storage service, you can use the Azure Storage Emulator included in the Azure SDK. Just run the emulator on your system and use the object `CloudStorageAccount.DevelopmentStorageAccount` as your account credentials.**

## Azure Queues

> Instead of plain strings, communicate with class models. 

Create a class that extends `AzureQueueMessage`. You can add any properties you like, as long as they can be serialized to JSON (the library does this behind the scenes) and the final JSON object is no larger than 48KB.

```csharp
public class TestAzureQueueMessage : AzureQueueMessage
{
    public string Name { get; set; }
    public int Value { get; set; }
}
```

Initialize the `IAzureQueue<TestAzureQueueMessage>` class passing your Azure Storage credentials. Here we are using the Azure Storage emulator credentials to create a queue called `test`.

```csharp
IAzureQueue<TestAzureQueueMessage> testMessageQueue = new AzureQueue<TestAzureQueueMessage>(CloudStorageAccount.DevelopmentStorageAccount, "test");
```

You can now create instances of your model and feed them into the queue object.

```csharp
var testMessage = new TestAzureQueueMessage
{
    Name = "One",
    Value = 1
};
testMessageQueue.AddMessage(testMessage);
```

You can poll your queue for new messages. 

```chsharp
var popTestMessage = testMessageQueue.GetMessage();
//Do something with the message
popTestMessage.Delete();
```

>You need to explicitly delete each queue message after you are finished with it. Queue messages become invisible in the queue when you read them for a small amount of time and reappear if they are not deleted explicitly. The default visibility timeout is 30 sec, but you can specify another amount in the IAzureQueue constructor.

Other operations that you can perform on the queue object include:

* Set a different retry policy
```csharp
public void SetRetryPolicy(IRetryPolicy retryPolicy)
```
* Clear the queue
```csharp
public void Clear()
```
* Update the contents of a message
```csharp
public void UpdateMessage(T message)
```
* Get the approximate message count of the queue
```csharp
public int RetrieveApproximateMessageCount()
```
* Get a batch of queue messages
```csharp
public List<T> GetMessages(int maxMessagesToReturn)
```

## Azure Table Storage

> Stop worrying about batch operations as they are handled internally. Querying is easier too.

Create a class that extends `TableEntity` and add any number of properties up to 255.

```csharp
public class TestTableEntity : TableEntity
{
    public string Name { get; set; }
    public int Price { get; set; }
}
```

Initialize the `IAzureTable<TestTableEntity>` class passing your Azure Storage credentials. Here we are using the Azure Storage emulator credentials to create a table called `test`.

```csharp
IAzureTable<TestTableEntity> testTable = new AzureTable<TestTableEntity>(CloudStorageAccount.DevelopmentStorageAccount, "test");
```

You can now create instances of your model and feed them into the table object. These models are called **entities**.

```csharp
var entity = new TestTableEntity
{
    PartitionKey = "partitionkey",
    RowKey = Guid.NewGuid().ToString(),
    Name = "testname",
    Price = 10
};
testTable.Insert(entity);
```

You can make a bulk insert by passing an `IEnumerable<TestTableEntity>` to the `testTable.Insert` method. **Normally, Azure's limit is 100 entities per insert/delete operation, but the library handles the operations internally so you don't have to worry about it.**

Using LINQ, you can easily run queues at your table.

```csharp
var retrievedEntities = testTable.Query.Where(x => x.PartitionKey == "partitionkey").ToList();
```

Finally, you can delete any number of entities at once

```csharp
testTable.Delete(retrievedEntities);
```

## Azure Blob Storage

> Blobs can normally be used to store any type of file. This library only handles a specific blob file type: object models. It accepts combinations of names and objects and stores them at a specified container. The objects can be requested back with their name. **The best practice is to put only one type of objects in each container to avoid type errors.**

Behind the scenes the library serializes the objects to JSON format and saves them to a blob container as text files. I have found this usage ideal as a cost-effective alternative to various caching mechanisms.

You can use any object that JSON.NET can serialize and you don't have any serious file size limitations (1tb per blob). For the sake of the example we will create a `TestBlobObject` class.

```csharp
public class TestBlobObject
{
    public string Name { get; set; }
    public int Price { get; set; }
}
```

Initialize the `IAzureBlobObjectContainer<TestBlobObject>` class passing your Azure Storage credentials. Here we are using the Azure Storage emulator credentials to create a blob container called `test`.

```csharp
IAzureBlobObjectContainer<TestBlobObject> testObjectContainer = new AzureBlobObjectContainer<TestBlobObject>(CloudStorageAccount.DevelopmentStorageAccount, "test");
```

You can now create instances of your model and feed them into the container object. These models are called **blobs**. Here we are passing a new test object to the container, and we are naming it `testmessage1`. We will use the same name to retrieve the blob later on.

```csharp
var testMessage = new TestBlobObject
{
    Name = "Two",
    Price = 250
};
testObjectContainer.AddOrReplace(testMessage, "testmessage1");
```

We can then retrieve the blob using the name we have set for it.

```csharp
var retrieved = testObjectContainer.Get("testmessage1");
```

We can check if a blob exists prior to retrieving it.

```csharp
bool exists = testObjectContainer.Exists("testmessage1");
```

We can receive a list of the names of the blobs that exist in a container.

```csharp
var names = testObjectContainer.ListNames();
```

We can find out the size of a blob.

```csharp
long size = testObjectContainer.Size("testmessage1");
```

We can find out the Date and Time that the blob was last modified.

```csharp
DateTime modified = testObjectContainer.LastModified("testmessage1");
```

Finally we can delete a blob that we don't need any more.

```csharp
testObjectContainer.Delete("testmessage1");
```

Created by [Anastasios Piotopoulos](http://drinkbird.com/)

## License
View [License](https://github.com/drinkbird/DrinkBird.Tools.AzureStorage/blob/master/LICENSE.md)

Inspired by [this guide](http://msdn.microsoft.com/en-us/library/hh871440.aspx), written by Microsoft Patterns & Practices Team.
