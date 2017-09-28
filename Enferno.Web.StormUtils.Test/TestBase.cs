using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Customers;
using Enferno.StormApiClient.Expose;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    public abstract class TestBase
    {
        protected static Application CreateDefaultApplication()
        {
            var defaultCountry = new Country { Code = "SE", Id = 1, Name = "SE" };
            var defaultCurrency = new Currency { Code = "SEK", Id = 1, Name = "SEK" };
            var defaultCulture = new Culture { Code = "sv-SE", Name = "sv-SE" };
            var defaultRole = new Role { Id = 1, Name = "DefaultRole", Description = "Default description" };
            var defaultSalesArea = new SalesArea { Id = 1, Name = "DefaultSalesArea" };

            return new Application
            {
                Id = 1,
                ParentId = 1,
                Key = Guid.NewGuid(),
                Name = "Test application",
                Url = @"http://www.test.se",
                SenderEmailAddress = "test@test.se",
                Client = new Client { Id = 1, Key = Guid.NewGuid(), Name = "TestClient" },
                Authorizations = new IdValues { new IdValue { Id = 1, Value = "DefaultAuthorization" } },
                Countries = new Countries { Default = defaultCountry, List = new CountryList { defaultCountry } },
                Currencies = new Currencies { Default = defaultCurrency, List = new CurrencyList { defaultCurrency } },
                Cultures = new Cultures { Default = defaultCulture, List = new CultureList { defaultCulture } },
                Pricelists = new Pricelists { Default = 1, Ids = new[] { 1 } },
                Roles = new Roles { Default = defaultRole, List = new RoleList { defaultRole } },
                SalesAreas = new SalesAreas { Default = defaultSalesArea, List = new SalesAreaList { defaultSalesArea } }
            };
        }

        protected static Customer CreateDefaultCustomer()
        {
            var login = "kalle.anka@ankeborg.se";

            var customer = new Customer
            {
                Id = 1,
                Key = Guid.NewGuid(),
                Email = login,
                FirstName = "Kalle",
                LastName = "Anka",
                Account = new Account { LoginName = login, Id = 2, Roles = new[] {1}, Authorizations = new IdValues()}
            };
            return customer;
        }

        protected static IHttpContextWrapper CreateEmptyHttpContextMock()
        {
            var requestCookies = new HttpCookieCollection();
            var responseCookies = new HttpCookieCollection();
            var items = new Dictionary<string, object>();

            var httpContextWrapper = MockRepository.GenerateMock<IHttpContextWrapper>();
            httpContextWrapper.Stub(x => x.ResponseCookies).Return(responseCookies);
            httpContextWrapper.Stub(x => x.RequestCookies).Return(requestCookies);
            httpContextWrapper.Stub(x => x.Items).Return(items);
            httpContextWrapper.Stub(x => x.IsSsl).Return(false);
            return httpContextWrapper;
        }

        protected static IHttpContextWrapper CreateRealHttpContextMock()
        {
            var context = new HttpContext(new HttpRequest("", "http://tempuri.org", ""),new HttpResponse(new StringWriter()));

            var httpContextWrapper = MockRepository.GenerateMock<IHttpContextWrapper>();
            httpContextWrapper.Stub(x => x.ResponseCookies).Return(context.Response.Cookies);
            httpContextWrapper.Stub(x => x.RequestCookies).Return(context.Request.Cookies);
            httpContextWrapper.Stub(x => x.Items).Return(context.Items);
            httpContextWrapper.Stub(x => x.IsSsl).Return(false);
            return httpContextWrapper;
        }
    }
}
