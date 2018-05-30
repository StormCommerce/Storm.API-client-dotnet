
using Enferno.Public.Caching;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Expose;
using Enferno.StormApiClient.Products;
using Enferno.StormApiClient.Shopping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;
using Rhino.Mocks.Constraints;

using System;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.Test
{
    [TestClass]
    public class AccessClientTest
    {
        [TestMethod, TestCategory("UnitTest")]
        [Description("Test creation of cacheable proxy for ApplicationServiceClient")]
        public void CreateTest1()
        {
            //Arrange
            // Act
            using (var client = new AccessClient())
            {
                // Assert
                Assert.IsInstanceOfType(client.ApplicationProxy, typeof(ApplicationService));
                Assert.AreEqual(client.ApplicationProxy.GetType(), typeof(ApplicationServiceClient));
                Assert.IsNotInstanceOfType(client.ApplicationProxy, typeof(ApplicationServiceClient));
            }
        }
        [TestMethod, TestCategory("UnitTest")]
        [Description("Test creation of real proxy for ApplicationServiceClient. Non cached")]
        public void CreateTest2()
        {
            //Arrange
            // Act
            using (var client = new AccessClient())
            {
                client.UseCache = false;
                // Assert
                Assert.IsInstanceOfType(client.ApplicationProxy, typeof(ApplicationService));
                Assert.IsInstanceOfType(client.ApplicationProxy, typeof(ApplicationServiceClient));
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ExposeProcessRequestTest1()
        {
            //Arrange
            var exposeService = MockRepository.GenerateMock<ExposeService>();
            exposeService.Stub(x => x.Process(null)).IgnoreArguments().Return(new ResponseList { new UpdateBasketItemResponse { Result = new Basket() } });

            var factory = MockRepository.GenerateMock<IServiceFactory>();
            factory.Stub(x => x.CreateProxy<ExposeService, ExposeServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ExposeServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(exposeService);

            // Act
            using (var client = new AccessClient(factory))
            {
                client.RegisterRequest("1", new UpdateBasketItemRequest());
                client.ProcessRequests();

                Basket result;
                client.TryGet("1", out result);

                // Assert
                Assert.IsNotNull(result);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetApplicationAsyncTest1()
        {
            //Arrange
            var svc = MockRepository.GenerateMock<ApplicationService>();
            svc.Stub(async x => await x.GetApplicationAsync(null)).IgnoreArguments().Return(Task.FromResult(new Application { Id = 1 }));

            var factory = MockRepository.GenerateMock<IServiceFactory>();
            factory.Stub(x => x.CreateProxy<ApplicationService, ApplicationServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ApplicationServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(svc);

            // Act
            using (var client = new AccessClient(factory))
            {
                var task = client.ApplicationProxy.GetApplicationAsync("sv");
                task.Wait();
                var application = task.Result;

                // Assert
                Assert.IsNotNull(application);
                Assert.AreEqual(1, application.Id);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetProductTest1()
        {
            //Arrange
            var svc = MockRepository.GenerateMock<ProductService>();
            svc.Stub(x => x.GetProduct(4711,"1,2,3", null, null, null, null, "sv-SE", "2")).IgnoreArguments().Return(new Product {Id = 4711});

            var factory = MockRepository.GenerateMock<IServiceFactory>();
            factory.Stub(x => x.CreateProxy<ProductService, ProductServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ProductServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(svc);

            // Act
            using (var client = new AccessClient(factory))
            {
                var product = client.ProductProxy.GetProduct(4711,"1,2,3", null, null, null, null, "sv-SE", "2");

                // Assert
                Assert.IsNotNull(product);
                Assert.AreEqual(4711, product.Id);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        [ExpectedException(typeof(ApplicationException))]
        public void ExceptionWhenProcessingRequestsTest1()
        {
            //Arrange
            var exposeService = MockRepository.GenerateMock<ExposeService>();
            exposeService.Stub(x => x.Process(null)).IgnoreArguments().Throw(new ApplicationException());

            var factory = MockRepository.GenerateMock<IServiceFactory>();
            factory.Stub(x => x.CreateProxy<ExposeService, ExposeServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ExposeServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(exposeService);

            // Act
            using (var client = new AccessClient(factory))
            {
                client.RegisterRequest("1", new UpdateBasketItemRequest());
                client.ProcessRequests();
            }

            // Assert
            Assert.Fail("Should not get here. ProcessRequest throws exeption.");
        }
    }
}
