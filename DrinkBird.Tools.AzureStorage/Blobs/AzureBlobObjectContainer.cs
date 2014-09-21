using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace DrinkBird.Tools.AzureStorage
{
    public class AzureBlobObjectContainer<T> : AzureBlobContainer<T>, IAzureBlobObjectContainer<T> where T : class
    {
        protected string MimeType { get; private set; }

        public AzureBlobObjectContainer(CloudStorageAccount account) 
            : this(account, typeof(T).Name.ToLowerInvariant())
        {
        }

        public AzureBlobObjectContainer(CloudStorageAccount account, string containerName)
            : base(account, containerName)
        {
            MimeType = "application/json";
        }

        public override T Get(string name)
        {
            name = name.Replace("/", "");
            var blob = Container.GetBlockBlobReference(name);
            if (!blob.Exists()) return null;
            var serializedObject = blob.DownloadText(Encoding.UTF8);
            return GetDeserializedObject(serializedObject);
        }

        public override void AddOrReplace(T obj, string name)
        {
            name = name.Replace("/", "");
            var blob = Container.GetBlockBlobReference(name);
            blob.Properties.ContentType = MimeType;
            var serializedObject = GetSerializedObject(obj);
            blob.UploadText(serializedObject, Encoding.UTF8);
        }

        public override void Add(T obj, string name)
        {
            name = name.Replace("/", "");
            if (Exists(name)) throw new InvalidOperationException("Blob cannot be added because the name already exists.");
            AddOrReplace(obj, name);
        }

        public override void Delete(string name)
        {
            name = name.Replace("/", "");
            var blob = Container.GetBlockBlobReference(name);
            blob.DeleteIfExists();
        }

        public override DateTime LastModified(string name)
        {
            name = name.Replace("/", "");
            if (!Exists(name)) throw new InvalidOperationException(string.Format("The blob [{0}] does not exist.", name));

            var blob = Container.GetBlockBlobReference(name);
            blob.FetchAttributes();

            return blob.Properties.LastModified != null
                ? blob.Properties.LastModified.Value.DateTime
                : DateTime.MinValue;
        }

        public override long Size(string name)
        {
            name = name.Replace("/", "");
            if (!Exists(name)) throw new InvalidOperationException(string.Format("The blob [{0}] does not exist.", name));
            
            var blob = Container.GetBlockBlobReference(name);
            blob.FetchAttributes();

            return blob.Properties.Length;
        }

        public override List<string> ListNames()
        {
            var blobs = Container.ListBlobs().OfType<CloudBlockBlob>();
            return blobs.Select(x => x.Name).ToList();
        }
        
        private static string GetSerializedObject(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private T GetDeserializedObject(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }
    }
}
