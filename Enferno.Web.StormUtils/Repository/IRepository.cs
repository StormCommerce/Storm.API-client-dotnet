using System;
using Enferno.StormApiClient;
using Enferno.StormApiClient.Applications;
using Enferno.StormApiClient.Customers;
using Enferno.StormApiClient.Expose;
using Enferno.StormApiClient.Shopping;

namespace Enferno.Web.StormUtils.InternalRepository
{
    public interface IRepository
    {
        IAccessClient GetBatch();

        void ChangePassword(string loginName, string oldPwd, string newPwd1, string newPwd2, string cultureCode = null);
        Customer CreateCustomer(Customer customer, int? accountId = null, string cultureCode = null);
        Application GetApplication(string cultureCode = null);
        Customer GetCustomerByEmail(string email, string cultureCode = null);
        Customer GetCustomerByKey(Guid key, string cultureCode = null);
        Customer GetCustomerByLoginName(string loginName, string cultureCode = null);
        Customer Login(string loginName, string password, string cultureCode = null);
        void SendPasswordReminder(string loginName, int? accountid = null, string cultureCode = null);
        Basket GetBasket(int basketId, string pricelistSeed = null, string cultureCode = null, int? currencyId = null);
        Checkout GetCheckout(int basketId, string pricelistSeed = null, string cultureCode = null, int? currencyId = null);
        Checkout UpdateBuyer(int basketId, Customer customer, int? accountId = null, string pricelistSeed = null, string cultureCode = null, int? currencyId = null);
        PaymentResponse PaymentCallback(NameValues paymentParameters);
        PaymentResponse PaymentCallback2(NameValues paymentParameters);
        Basket PaymentCancel(Basket basket);
    }
}
