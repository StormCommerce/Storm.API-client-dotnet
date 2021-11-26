
using Enferno.Public.Caching;
using Enferno.Public.InversionOfControl;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Expose;
using Enferno.StormApiClient.OAuth2;
using Enferno.StormApiClient.Products;
using Enferno.StormApiClient.Shopping;
using Enferno.StormApiClient.Test.Builders;
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
        private IServiceFactory factory;
        private IOAuth2TokenResolver oAuth2TokenResolver;
        private IOAuth2CredentialsProvider oAuth2CredentialsProvider;

        [TestInitialize]
        public void Initialize()
        {
            oAuth2CredentialsProvider = MockRepository.GenerateMock<IOAuth2CredentialsProvider>();
            oAuth2CredentialsProvider.Stub(p => p.GetOAuth2Credentials()).Return(Builder.OAuth2Credentials);
            IoC.RegisterInstance(typeof(IOAuth2CredentialsProvider), oAuth2CredentialsProvider);
            factory = MockRepository.GenerateMock<IServiceFactory>();
            oAuth2TokenResolver = MockRepository.GenerateMock<IOAuth2TokenResolver>();
            oAuth2TokenResolver.Stub(r => r.GetToken(Arg<OAuth2Credentials>.Is.Anything))
                .Return(Task.FromResult(Builder.OAuth2Token.Build()));
        }

        [TestMethod, TestCategory("UnitTest")]
        [Description("Test creation of cacheable proxy for ApplicationServiceClient")]
        public void CreateTest1()
        {
            //Arrange
            // Act
            using (var client = CreateSutWithRealFactory())
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
            using (var client = CreateSutWithRealFactory())
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

            factory.Stub(x => x.CreateProxy<ExposeService, ExposeServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ExposeServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(exposeService);

            // Act
            using (var client = CreateSutWithMockFactory())
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

            factory.Stub(x => x.CreateProxy<ApplicationService, ApplicationServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ApplicationServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(svc);

            // Act
            using (var client = CreateSutWithMockFactory())
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
            svc.Stub(x => x.GetProduct(4711, "1,2,3", null, null, null, null, "sv-SE", "2")).IgnoreArguments().Return(new Product { Id = 4711 });

            factory.Stub(x => x.CreateProxy<ProductService, ProductServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ProductServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(svc);

            // Act
            using (var client = CreateSutWithMockFactory())
            {
                var product = client.ProductProxy.GetProduct(4711, "1,2,3", null, null, null, null, "sv-SE", "2");

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

            factory.Stub(x => x.CreateProxy<ExposeService, ExposeServiceClient>(Arg<bool>.Is.Anything, Arg<ICacheManager>.Is.Anything, Arg<string>.Is.Anything, ref Arg<ExposeServiceClient>.Ref(Is.Anything(), null).Dummy))
                .Return(exposeService);

            // Act
            using (var client = CreateSutWithMockFactory())
            {
                client.RegisterRequest("1", new UpdateBasketItemRequest());
                client.ProcessRequests();
            }

            // Assert
            Assert.Fail("Should not get here. ProcessRequest throws exeption.");
        }

        private AccessClient CreateSutWithMockFactory() => new AccessClient(factory, oAuth2TokenResolver, oAuth2CredentialsProvider);

        private AccessClient CreateSutWithRealFactory() => new AccessClient(new ServiceFactory(), oAuth2TokenResolver, oAuth2CredentialsProvider);
    }
}
