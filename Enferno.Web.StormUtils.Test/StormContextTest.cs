using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.WebSockets;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Shopping;
using Enferno.Web.StormUtils.InternalRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class StormContextTest : TestBase
    {      
        [TestMethod]
        public void DefaultConfigurationTest()
        {
            // arrange: from cfg
            // assert
            Assert.AreEqual("1", StormContext.Configuration.AssortmentIdSeed);
            Assert.AreEqual("~/checkout.aspx", StormContext.Configuration.CheckoutUrl);
            Assert.AreEqual("~/confirmed.aspx", StormContext.Configuration.ConfirmedUrl);
            Assert.AreEqual(30, StormContext.Configuration.CookieExpirationDays);
            Assert.AreEqual("sv", StormContext.Configuration.DefaultCulture);
            Assert.AreEqual("~/default.aspx", StormContext.Configuration.DefaultUrl);
            Assert.AreEqual(false, StormContext.Configuration.EncryptCookie);
            Assert.AreEqual("http://services.enferno.se/image/", StormContext.Configuration.ImageUrl);
            Assert.AreEqual(true, StormContext.Configuration.IsPrivate);
            Assert.AreEqual(2, StormContext.Configuration.MaxNavigationLevels);
            Assert.AreEqual("http://stormstage.enferno.se/Message/Dibs/ReturnUrlRedirect.ashx", StormContext.Configuration.PaymentReturnUrl);
            Assert.AreEqual(false, StormContext.Configuration.ProductCountAsVariants);
            Assert.AreEqual("1,2,3,4", StormContext.Configuration.ProductStatusIdSeed);
            Assert.AreEqual(true, StormContext.Configuration.ShowPricesIncVat);

            Assert.IsNull(((StormConfigurationSection)ConfigurationManager.GetSection("stormSettings/storm")).ShowPricesIncVatInternal);
        }

        [TestMethod]
        public void EncryptCookieTrueTest()
        {
            //Arrange
            var ctx = MockRepository.GenerateMock<IStormContext>();
            var cfg = MockRepository.GenerateMock<IStormConfigurationSection>();

            cfg.Stub(x => x.EncryptCookie).Return(true);
            ctx.Stub(x => x.Configuration).Return(cfg);

            StormContext.SetInstance(ctx);

            // Assert
            Assert.IsTrue(StormContext.Configuration.EncryptCookie);
        }

        [TestMethod]
        public void GetCookieTest()
        {
            // Arrange
            var ctx = MockRepository.GenerateMock<IStormContext>();

            ctx.Stub(x => x.GetCookie("jalle")).IgnoreArguments().Return(new HttpCookie("jalle"));

            StormContext.SetInstance(ctx);

            // Act
            var cookie = StormContext.GetCookie("jalle");

            // Assert
            Assert.AreEqual("jalle", cookie.Name);
        }

        [TestMethod]
        public void GetLoginNameTest()
        {
            // Arrange
            var ctx = MockRepository.GenerateMock<IStormContext>();

            ctx.Stub(x => x.LoginName).Return("kalle.anka@ankeborg.se");

            StormContext.SetInstance(ctx);

            // Assert
            Assert.AreEqual("kalle.anka@ankeborg.se", StormContext.LoginName);
        }

        [TestMethod]
        public void LoginUserTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();

            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoginUser("kalle.anka@ankeborg.se");

            // Assert
            Assert.AreEqual("kalle.anka@ankeborg.se", StormContext.LoginName);
            Assert.AreEqual(1, StormContext.CustomerId);
            Assert.AreEqual(2, StormContext.AccountId);
        }
        
        [TestMethod]
        public void LoginUserWithBasketTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var basket = new Basket { Id = 1, };

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);           
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);            
            repository.Stub(x => x.GetBasket(1)).IgnoreArguments().Return(basket);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);
            StormContext.BasketId = 1;

            // Act
            StormContext.LoginUser("kalle.anka@ankeborg.se");

            // Assert
            repository.AssertWasCalled(x => x.UpdateBuyer(basket.Id.GetValueOrDefault(1), customer), o => o.IgnoreArguments());
        }       

        [TestMethod]
        public void LogoutUserTest1()
        {
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();

            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoginUser("kalle.anka@ankeborg.se");
            Assert.IsNotNull(StormContext.LoginName);
            StormContext.LogoutUser();

            // Assert
            Assert.IsNull(StormContext.LoginName);
            Assert.IsNull(StormContext.AccountId);
            Assert.IsNull(StormContext.CompanyId);
            Assert.IsNull(StormContext.CustomerId);         
        }

        [TestMethod]
        public void LogoutUserTest2()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var basket = new Basket { Id = 1, };

            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.GetBasket(1)).IgnoreArguments().Return(basket);

            var httpContextWrapper = CreateEmptyHttpContextMock();           
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);
            StormContext.BasketId = 1;

            // Act
            StormContext.LoginUser("kalle.anka@ankeborg.se");
            Assert.IsNotNull(StormContext.LoginName); 
            StormContext.LogoutUser();

            // Assert
            repository.AssertWasCalled(x => x.UpdateBuyer(basket.Id.GetValueOrDefault(1), customer), o => o.IgnoreArguments());
            Assert.IsNull(StormContext.LoginName);
            Assert.IsNull(StormContext.AccountId);
            Assert.IsNull(StormContext.CompanyId);
            Assert.IsNull(StormContext.CustomerId);
        }

        [TestMethod]
        public void LoadCookieEmptyCookieTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var repository = MockRepository.GenerateMock<IRepository>();
            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormSession") { Value = null });
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormPersisted") { Value = null });
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoadCookie();

            // Assert
            Assert.AreEqual("sv-SE", StormContext.CultureCode);
            Assert.AreEqual(1, StormContext.CurrencyId);
            Assert.AreEqual(1, StormContext.SalesAreaId);
        }

        [TestMethod]
        public void LoadCookieInvalidCookieWithRefreshTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var repository = MockRepository.GenerateMock<IRepository>();
            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormSession") { Value = null });
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormPersisted") { Value = "LoginName=!null!&AccountId=!null!&CustomerId=!null!&CompanyId=!null!&DivisionId=!null!&BasketId=!null!&CurrencyId=5&CultureCode=da-DK&SalesAreaId=3&ShowPricesIncVat=!null!&IsPrivate=True&ReferId=!null!&ReferUrl=!null!&RememberMe=True" });
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoadCookie();

            // Assert
            Assert.AreEqual("sv-SE", StormContext.CultureCode);
            Assert.AreEqual(1, StormContext.CurrencyId);
            Assert.AreEqual(1, StormContext.SalesAreaId);
        }

        [TestMethod]
        public void SaveCookieTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var repository = MockRepository.GenerateMock<IRepository>();
            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormSession") { Value = null });
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormPersisted") { Value = null });
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoadCookie();
            StormContext.BasketId = 1;
            StormContext.CustomerId = 123456;
            StormContext.LoginName = "kalle.anka@ankeborg.se";
            StormContext.SaveCookie();

            // Assert
            Assert.AreEqual("LoginName=kalle.anka@ankeborg.se&AccountId=!null!&CustomerId=123456&CompanyId=!null!&DivisionId=!null!&BasketId=1&CurrencyId=1&CultureCode=sv-SE&SalesAreaId=1&ShowPricesIncVat=!null!&IsPrivate=True&ReferId=!null!&RememberMe=True", httpContextWrapper.ResponseCookies["StormPersisted"].Value);
        }

        [TestMethod]
        public void LoadAndSaveWithBasketIdCookieTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var repository = MockRepository.GenerateMock<IRepository>();
            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateRealHttpContextMock();
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormSession") { Value = null });
            httpContextWrapper.RequestCookies.Add(new HttpCookie("StormPersisted") { Value = null });
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.LoadCookie();
            StormContext.BasketId = 4711;
            StormContext.SaveCookie();
            StormContext.LoadCookie();

            // Assert
            Assert.AreEqual(4711, StormContext.BasketId);
        }

        [TestMethod]
        public void CreateNoHttpStormContextTest()
        {
            // Arrange
            var application = new Application
            {
                Currencies = new Currencies { Default = new Currency { Id = 2 } },
                Cultures = new Cultures { Default = new Culture { Code = "da-DK" } },
                SalesAreas = new SalesAreas { Default = new SalesArea { Id = 1 } },
            };

            var httpContextWrapper = new NoHttpContext(application);
            var ctx = new StormContext(new Repository(), httpContextWrapper);
            StormContext.SetInstance(ctx);

            // Act
            StormContext.ShowPricesIncVat = true;

            // Assert
            Assert.AreEqual(true, StormContext.ShowPricesIncVat);
        }
    }

    internal class NoHttpContext : IHttpContextWrapper
    {
        private readonly HttpContext context = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));
        private readonly Dictionary<string, object> items = new Dictionary<string, object>();

        public IDictionary Items => items;

        public HttpCookieCollection RequestCookies => context.Request.Cookies;

        public HttpCookieCollection ResponseCookies => context.Response.Cookies;

        public NoHttpContext(Application application)
        {
            items.Add("StormSession", new SessionCookie());
            items.Add("StormPersisted", new PersistedCookie(application));
        }
        public bool IsSsl => false;
    }
}
