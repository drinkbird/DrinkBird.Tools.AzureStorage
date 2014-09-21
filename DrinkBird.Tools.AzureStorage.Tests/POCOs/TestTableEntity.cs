using Microsoft.WindowsAzure.Storage.Table;

namespace DrinkBird.Tools.AzureStorage.Tests
{
    public class TestTableEntity : TableEntity
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
