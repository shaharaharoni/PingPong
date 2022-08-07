using Azure.Data.Tables;

namespace Web1
{
    public class TableStorageService : IStorageService
    {
        public bool isInitialized;
        private TableClient _tableClient;

        public TableStorageService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public async Task AddAsync(Item item)
        {
            //Table Storage stores entities and not items, so convert from item to entity
            TableEntity tableEntity = item.getEntity();
            await _tableClient.AddEntityAsync(tableEntity);
        }

        public async Task DeleteAsync(string id)
        {
            await _tableClient.DeleteEntityAsync(id, id);
        }

        public async Task<Item> GetAsync(string id)
        {
            //Table Storage stores entities and not items, so convert from entity to item to maintain interface
            TableEntity tableEntity = await _tableClient.GetEntityAsync<TableEntity>(id, id);
            Item item = convertToItem(tableEntity);
            return item;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            var response = _tableClient.QueryAsync<TableEntity>();
            var results = new List<Item>();

            await foreach (TableEntity tableEntity in response)
            {
                Item item = convertToItem(tableEntity);
                results.Add(item);
            }
            return results;
        }

        public async Task UpdateAsync(string id, Item item)
        {
            TableEntity tableEntity = item.getEntity();
            await _tableClient.UpsertEntityAsync<TableEntity>(tableEntity);
        }

        private static Item convertToItem(TableEntity tableEntity)
        {
            string itemId = tableEntity.GetString("id");
            string itemName = tableEntity.GetString("name");
            string itemValue = tableEntity.GetString("value");
            Item item = new Item(itemId, itemName, itemValue);
            return item;
        }


        public static async Task<TableStorageService> InitializeTableClientInstance(IConfigurationSection configurationSection)
        {
            TableServiceClient serviceClient = new TableServiceClient(configurationSection["StorageConnectionString"]);

            TableClient tableClient = serviceClient.GetTableClient(configurationSection["TableName"]);
            await tableClient.CreateIfNotExistsAsync();
            var tableStorageService = new TableStorageService(tableClient);
            tableStorageService.isInitialized = true;
            return tableStorageService;
        }
    }
}
