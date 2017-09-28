
using System;
using System.Xml;
using Enferno.StormApiClient;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Customers;
using Enferno.StormApiClient.Expose;
using Enferno.StormApiClient.Shopping;

namespace Enferno.Web.StormUtils.InternalRepository
{
    public class Repository : IRepository
    {
        private readonly Func<IAccessClient> accessClientFactoryMethod;

        public Repository()
        {
        }

        public Repository(Func<IAccessClient> factoryMethod)
        {
            this.accessClientFactoryMethod = factoryMethod;
        }

        public IAccessClient GetBatch()
        {
            return CreateAccessClient();
        }

        public void ChangePassword(string loginName, string oldPwd, string newPwd1, string newPwd2, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                api.CustomerProxy.ChangePassword(loginName, oldPwd, newPwd1, newPwd2, CultureCode(cultureCode));
            }
        }

        public Customer CreateCustomer(Customer customer, int? accountId = null, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.CustomerProxy.CreateCustomer(customer, AccountId(accountId), CultureCode(cultureCode));
            }
        }

        public Application GetApplication(string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.ApplicationProxy.GetApplication(CultureCode(cultureCode));
            }
        }

        public Customer GetCustomerByEmail(string email, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.CustomerProxy.GetCustomerByEmail(email, CultureCode(cultureCode));
            }
        }

        public Customer GetCustomerByKey(Guid key, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.CustomerProxy.GetCustomerByKey(key, CultureCode(cultureCode));
            }
        }

        public Customer GetCustomerByLoginName(string loginName, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.CustomerProxy.GetCustomerByLoginName(loginName, CultureCode(cultureCode));
            }
        }

        public Customer Login(string loginName, string password, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.CustomerProxy.Login(loginName, password, CultureCode(cultureCode));
            }
        }

        public void SendPasswordReminder(string loginName, int? accountId = null, string cultureCode = null)
        {
            using (var api = CreateAccessClient())
            {
                api.CustomerProxy.SendPasswordReminder(loginName, AccountId(accountId), CultureCode(cultureCode));
            }
        }

        public Basket GetBasket(int basketId, string pricelistSeed = null, string cultureCode = null, int? currencyId = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.GetBasket(basketId, pricelistSeed, CultureCode(cultureCode), Currency(currencyId));
            }            
        }

        public Checkout GetCheckout(int basketId, string pricelistSeed = null, string cultureCode = null, int? currencyId = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.GetCheckout(basketId, pricelistSeed, CultureCode(cultureCode), Currency(currencyId));
            }
        }

        public Checkout UpdateBuyer(int basketId, Customer customer, int? accountId = null, string pricelistSeed = null, string cultureCode = null, int? currencyId = null)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.UpdateBuyer(basketId, customer, accountId.GetValueOrDefault(1), pricelistSeed, CultureCode(cultureCode), Currency(currencyId));
            }
        }

        public PaymentResponse PaymentCallback(NameValues paymentParameters)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.PaymentCallback(paymentParameters);
            }
        }

        public PaymentResponse PaymentCallback2(NameValues paymentParameters)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.PaymentCallback2(paymentParameters);
            }
        }

        public Basket PaymentCancel(Basket basket)
        {
            using (var api = CreateAccessClient())
            {
                return api.ShoppingProxy.PaymentCancel(basket);
            } 
        }

        private static string CultureCode(string cultureCode)
        {
            return !string.IsNullOrWhiteSpace(cultureCode) ? cultureCode : StormContext.CultureCode;
        }

        private static string Currency(int? currencyId)
        {
            return currencyId.HasValue ? GetNullableInt(currencyId) : XmlConvert.ToString(StormContext.CurrencyId);
        }

        private static string AccountId(int? accountId)
        {
            return accountId.HasValue ? GetNullableInt(accountId) : StormContext.AccountId.HasValue ? StormContext.AccountId.ToString() : "1";
        }

        private static string GetNullableInt(int? i)
        {
            return i.HasValue ? i.ToString() : null;
        }

        private IAccessClient CreateAccessClient()
        {
            return accessClientFactoryMethod != null ? accessClientFactoryMethod() : new AccessClient();
        }
    }
}
