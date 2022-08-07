using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Web1;


namespace TestProject1
{
    [TestClass]
    public class UnitTesting
    {
        private int a;
        private CosmosDbService cosmos;

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").AddEnvironmentVariables().Build();
            return config;
        }

        [TestInitialize]
        public void initialize()
        {
            IConfigurationSection configuration = InitConfiguration().GetSection("CosmosDb");
            cosmos = CosmosDbService.InitializeCosmosClientInstance(configuration).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestCosmosInitialize()
        {
            Assert.IsTrue(!cosmos.IsNull());
            
        }

        [TestMethod]
        public void Test2()
        {
            Assert.IsTrue(a == 3);
        }
    }
}
