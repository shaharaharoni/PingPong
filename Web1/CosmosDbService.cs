using Microsoft.Azure.Cosmos;

namespace Web1
{
    public class CosmosDbService : IStorageService
    {
        public bool isInitialized;
        public Container container;

        public CosmosDbService(CosmosClient cosmosDbClient, string databaseName, string containerName)
        {
            container = cosmosDbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddAsync(Item item)
        {
            await container.CreateItemAsync(item, new PartitionKey(item.Id));
        }

        public async Task DeleteAsync(string id)
        {
            await container.DeleteItemAsync<Item>(id, new PartitionKey(id));
        }

        public async Task<Item> GetAsync(string id)
        {
            try
            {
                ItemResponse<Item> response = await container.ReadItemAsync<Item>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task UpdateAsync(string id, Item item)
        {
            await container.UpsertItemAsync(item, new PartitionKey(id));
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            var queryDefinition = new QueryDefinition("SELECT * FROM c");
            var iterator = container.GetItemQueryIterator<Item>(queryDefinition);

            var results = new List<Item>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }

        public static async Task<CosmosDbService> InitializeCosmosClientInstance(IConfigurationSection configurationSection)
        {
            var databaseName = configurationSection["DatabaseName"];
            var containerName = configurationSection["ContainerName"];
            var account = configurationSection["Account"];
            var key = configurationSection["Key"];

            var client = new CosmosClient(account, key);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            var cosmosDbService = new CosmosDbService(client, databaseName, containerName);
            cosmosDbService.isInitialized = true;
            return cosmosDbService;
        }
    }
}
