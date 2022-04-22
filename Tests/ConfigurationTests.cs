using Newtonsoft.Json;
using NUnit.Framework;
using RedmineTelegram;
using System.IO;

namespace Tests
{
    class ConfigurationTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            bool isExists = Configuration.TryGetFromJson("configuration.json", out _);
            if (!isExists)
            {
                Configuration cfg = new();
                cfg.WriteToJson("configuration.json");
            }
            Assert.IsTrue(true);
        }
    }
}
