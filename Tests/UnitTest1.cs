using NUnit.Framework;
using RedmineTelegram;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            InternalDatabase a = new();
            Assert.Pass();
        }
    }
}