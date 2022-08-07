using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Web1
{
    public class Item
    {
        public Item(string id, string name, string value)
        {
            Id = id;
            Name = name;
            Value = value;
            _tableEntity = new TableEntity(Id, Id)
            {
                {"id", Id },
                {"name", Name},
                {"value", Value}
            };
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        public TableEntity getEntity()
        {
            return _tableEntity;
        }

        private TableEntity _tableEntity;


        //public string PartitionKey { get; set; }
        //public string RowKey { get; set; }
        //public DateTimeOffset? Timestamp { get; set; }
        //public ETag ETag { get; set; }
    }
}