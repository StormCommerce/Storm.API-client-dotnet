
using Enferno.StormApiClient.Expose;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.StormApiClient.Test
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        public void GetCacheKeyTest()
        {
            var request = new GetBasketRequest
            {
                Id = 4711,
                PricelistSeed = "1,2",
                CultureCode = "sv",
                CurrencyId = "2"
            };

            var key = request.GetCacheKey();

            Assert.AreEqual("GetBasket:4711:1,2:sv:2:", key);
        }

        [TestMethod]
        public void GetCacheKeyTest2()
        {
            var request = new GetBasketRequest
            {
                Id = 4711,
                PricelistSeed = "1,2",
                CultureCode = "sv",
                CurrencyId = "2"
            };

            var key = request.GetCacheKey("XX");

            Assert.AreEqual("GetBasket:XX:4711:1,2:sv:2:", key);
        }
    }
}
