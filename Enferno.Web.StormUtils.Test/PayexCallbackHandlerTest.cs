using System.IO;
using System.Web;
using Enferno.StormApiClient.Shopping;
using Enferno.Web.StormUtils.InternalRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class PayexCallbackHandlerTest : TestBase
    {
        [TestMethod, TestCategory("UnitTest")]
        public void ProcessValidRequestTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var basket = new Basket { Id = 1, };

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.GetBasket(1)).IgnoreArguments().Return(basket);
            repository.Stub(x => x.PaymentCallback(null)).IgnoreArguments().Return(new PaymentResponse { Status = "OK", StatusDescription = "This was OK." });

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);
            StormContext.BasketId = 1;

            var handler = new PayexCallbackHandler(repository);
            var request = new HttpContext(
                new HttpRequest("", application.Url, "orderRef=123456"),
                new HttpResponse(new StringWriter()));

            // Act
            handler.ProcessRequest(request);

            // Assert
            Assert.AreEqual(1, StormContext.ConfirmedBasketId);
            Assert.AreEqual(null, StormContext.BasketId);
            repository.AssertWasNotCalled(x => x.PaymentCancel(null), o => o.IgnoreArguments());
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ProcessRequestWithoutBasketTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var handler = new PayexCallbackHandler(repository);
            var request = new HttpContext(
                new HttpRequest("", application.Url, "orderRef=123456"),
                new HttpResponse(new StringWriter()));

            // Act
            handler.ProcessRequest(request);

            // Assert
            repository.AssertWasNotCalled(x => x.GetBasket(1), o => o.IgnoreArguments());
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ProcessRequestWithInvalidParametersTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var basket = new Basket { Id = 1, };

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.GetBasket(1)).IgnoreArguments().Return(basket);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);
            StormContext.BasketId = 1;

            var handler = new PayexCallbackHandler(repository);
            var request = new HttpContext(
                new HttpRequest("", application.Url, "xx=123456"),
                new HttpResponse(new StringWriter()));

            // Act
            handler.ProcessRequest(request);

            // Assert
            repository.AssertWasCalled(x => x.PaymentCancel(null), o => o.IgnoreArguments());
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ProcessValidRequestWithFailedResponseTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var basket = new Basket { Id = 1, };

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.GetBasket(1)).IgnoreArguments().Return(basket);
            repository.Stub(x => x.PaymentCallback(null)).IgnoreArguments().Return(new PaymentResponse { Status = "FAIL", StatusDescription = "This was NOT OK." });

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);
            StormContext.BasketId = 1;

            var handler = new PayexCallbackHandler(repository);
            var request = new HttpContext(
                new HttpRequest("", application.Url, "orderRef=123456"),
                new HttpResponse(new StringWriter()));

            // Act
            handler.ProcessRequest(request);

            // Assert
            repository.AssertWasCalled(x => x.PaymentCancel(null), o => o.IgnoreArguments());
        }
    }
}
