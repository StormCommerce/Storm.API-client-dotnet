using System;
using System.Threading.Tasks;
using Enferno.Public.Caching;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Enferno.StormApiClient.Test
{
    [TestClass]
    public class CacheableProxyTests
    {
        [TestMethod]
        public void GetNonExistningCustomerAndCreateNewShouldAddToCacheTest()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const string email = "patrik@attentia.se";
            const string cultureCode = "sv-SE";

            var cacheManager = new CacheManager();
            var cache = new InMemoryCache(cacheName);
            cacheManager.AddCache(cache);

            var customer = new Customer{ Id = 1, Email = email};
            
            //Arrange
            var createdCustomer = customer;
            var customerProxy = MockRepository.GenerateMock<CustomerServiceClient>();
            customerProxy.Stub(x => x.GetCustomerByEmail(email, cultureCode)).IgnoreArguments().Return(null).Repeat.Once();
            customerProxy.Stub(x => x.GetCustomerByEmail(email, cultureCode)).IgnoreArguments().Return(createdCustomer);
            customerProxy.Stub(x => x.CreateCustomer(createdCustomer, email, cultureCode)).IgnoreArguments().Return(createdCustomer);

            var factory = new ServiceFactory();
            var cacheableProxy = factory.CreateProxy<CustomerService, CustomerServiceClient>(true, cacheManager,cacheName, ref customerProxy);
            
            var client = MockRepository.GenerateMock<IAccessClient>();
            client.Stub(x => x.CustomerProxy).Return(cacheableProxy);

            // Act
            customer = client.CustomerProxy.GetCustomerByEmail(email, cultureCode);
            Assert.IsNull(customer, "Should be null first time");

            customer = client.CustomerProxy.CreateCustomer(createdCustomer, "1", cultureCode);
            Assert.IsNotNull(customer, "Should not be null after create");

            customer = client.CustomerProxy.GetCustomerByEmail(email, cultureCode);
            Assert.IsNotNull(customer, "Should not be null second time");
        }

        [TestMethod]
        public void GetCustomerThenUpdateAndGetAgainShouldGetUpdatedCustomerFromCacheTest()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const string cultureCode = "sv-SE";

            var cacheManager = new CacheManager();
            var cache = new InMemoryCache(cacheName);
            cacheManager.AddCache(cache);

            var customer = new Customer { Id = 1, Email = "patrik@attentia.se" };

            //Arrange
            var updatedCustomer = new Customer { Id = 1, Email = "patrik2@attentia.se" };
            var customerProxy = MockRepository.GenerateMock<CustomerServiceClient>();
            customerProxy.Stub(x => x.GetCustomer(1, cultureCode)).IgnoreArguments().Return(customer).Repeat.Once();
            customerProxy.Stub(x => x.UpdateCustomer2(updatedCustomer, "abc123", 4711, cultureCode)).IgnoreArguments().Return(updatedCustomer);

            var factory = new ServiceFactory();
            var cacheableProxy = factory.CreateProxy<CustomerService, CustomerServiceClient>(true, cacheManager, cacheName, ref customerProxy);

            var client = MockRepository.GenerateMock<IAccessClient>();
            client.Stub(x => x.CustomerProxy).Return(cacheableProxy);

            // Act
            customer = client.CustomerProxy.GetCustomer(1, cultureCode);
            Assert.IsNotNull(customer, "Should have an existing");

            customer = client.CustomerProxy.UpdateCustomer2(updatedCustomer, "abc123", 4711, cultureCode);
            Assert.IsNotNull(customer, "Should not be null after create");

            var result = client.CustomerProxy.GetCustomer(1, cultureCode);
            Assert.IsNotNull(result, "Should have value from cache.");
            Assert.AreSame(updatedCustomer, result);
        }
    }
}
