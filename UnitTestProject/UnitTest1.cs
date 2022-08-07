using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Telerik.JustMock;
using Web1;
using Web1.Controllers;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace TestProject2
{
    [TestClass]
    public class UnitTesting
    {
        private IConfiguration configuration;
        private CosmosDbService? cosmos;
        private TableStorageService? table;
        private Container? container;

        public void InitConfiguration()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").AddEnvironmentVariables().Build();
        }

        [TestInitialize]
        public void Initialize()
        {
            InitConfiguration();
        }

        [TestMethod]
        public void TestCosmosInitialize()
        {
            IConfigurationSection section = configuration.GetSection("CosmosDb");
            cosmos = CosmosDbService.InitializeCosmosClientInstance(section).GetAwaiter().GetResult();
            Assert.IsTrue(cosmos.isInitialized);
        }

        [TestMethod]
        public void TestTableStorageInitialize()
        {
            IConfigurationSection section = configuration.GetSection("Table");
            table = TableStorageService.InitializeTableClientInstance(section).GetAwaiter().GetResult();
            Assert.IsTrue(table.isInitialized);
        }

        [TestMethod]
        public void TestController()
        {
            string connectionString = configuration.GetConnectionString("RedisConnectionString");
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionString);
            IDistributedCache cache = (IDistributedCache)connection.GetDatabase();

            IDistributedCache mockCache = Mock.Create<IDistributedCache>(); 
            ItemsController controller = new ItemsController(cosmos, cache);
            //Mock.Arrange(() => controller.cache.Get("abc")).Returns(null);
        }

        //[TestMethod]
        //public void TestCosmosAdd()
        //{
        //    Item item = new Item("123", "shahar", "intern");
        //    Mock.Arrange(() => cosmos.AddAsync(item)).Returns(true);
        //}
    }
}
