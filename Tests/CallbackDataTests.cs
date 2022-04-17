using NUnit.Framework;
using RedmineTelegram;

namespace Tests
{
    public class CallbackDataTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void Test1()
        {
            string callbackData = "2 321321 Jijobes Azazaz 346543";
            CallbackData data = CallbackData.GetFromString(callbackData);
            Assert.AreEqual("Jijobes Azazaz 346543", data.AdditionalData);
            Assert.AreEqual(321321, data.TargetIssueId);
            Assert.AreEqual((CallbackDataCommand)2, data.Command);
            Assert.AreEqual(callbackData, data.ToString());
        }
    }
}
