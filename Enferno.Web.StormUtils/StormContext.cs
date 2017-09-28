
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Specialized;
using Enferno.StormApiClient.Applications;
using Enferno.Web.StormUtils.InternalRepository;
using Customers = Enferno.StormApiClient.Customers;

namespace Enferno.Web.StormUtils
{
    #region Internal and helper classes

    public interface IHttpContextWrapper
    {
        HttpCookieCollection ResponseCookies { get; }
        HttpCookieCollection RequestCookies { get; }
        IDictionary Items { get; }
        bool IsSsl { get; }
    }

    public class HttpContextWrapper : IHttpContextWrapper
    {
        public HttpCookieCollection ResponseCookies => HttpContext.Current.Response.Cookies;

        public HttpCookieCollection RequestCookies => HttpContext.Current.Request.Cookies;

        public IDictionary Items => HttpContext.Current.Items;

        public bool IsSsl => HttpContext.Current.Request.Url.Scheme == "https";
    }

    public class SessionCookie : CookieBase
    {
        public bool CookieLoaded;

        public readonly List<int> PriceListIds;
        public int? ConfirmedBasketId;
        public string ReferUrl;
        public bool IsInitialRequest;

        public SessionCookie()
        {
            PriceListIds = new List<int>();
            IsInitialRequest = true;
        }

        public override string ToString()
        {
            return $"{IsInitialRequest}:{string.Join(":", PriceListIds.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray())}:{ConfirmedBasketId}:{ReferUrl}:{base.ToString()}";
        }
    }

    public class PersistedCookie : CookieBase
    {
        public string LoginName;
        public int? AccountId;
        public int? CustomerId;
        public int? CompanyId;
        public int? DivisionId;
        public int? BasketId;
        public int CurrencyId;
        public string CultureCode;
        public int SalesAreaId;
        public bool? ShowPricesIncVat;
        public bool? IsPrivate;
        public int? ReferId;
        public bool? RememberMe;

        public PersistedCookie(Application application)
        {
            ShowPricesIncVat = ((StormConfigurationSection)ConfigurationManager.GetSection("stormSettings/storm")).ShowPricesIncVatInternal;
            IsPrivate = true;
            RememberMe = true;
            CurrencyId = application.Currencies.Default.Id;
            CultureCode = application.Cultures.Default.Code;
            SalesAreaId = application.SalesAreas.Default.Id;
        }
 
        public override string ToString()
        {
            return $"{LoginName}:{AccountId}:{CustomerId}:{CompanyId}:{DivisionId}:{BasketId}:{CurrencyId}:{CultureCode}:{SalesAreaId}:{ShowPricesIncVat}:{IsPrivate}:{ReferId}:{""}:{RememberMe}:{base.ToString()}";
        }
    }

    #endregion

    public interface IStormContext
    {
        string LoginName { get; set; }
        int? AccountId { get; set; }
        int? CustomerId { get; set; }
        int? CompanyId { get; set; }
        int? DivisionId { get; set; }
        int? BasketId { get; set; }
        int? ReferId { get; set; }
        string ReferUrl { get; set; }
        bool RememberMe { get; set; }
        int? ConfirmedBasketId { get; set; }
        string CultureCode { get; set; }
        int CurrencyId { get; set; }
        List<int> PriceListIds { get; }
        string PriceListIdSeed { get; }
        int SalesAreaId { get; set; }
        bool? ShowPricesIncVat { get; set; }
        bool IsInitialRequest { get; set; }
        NameValueCollection SessionItems { get; }
        NameValueCollection PersistedItems { get; }
        IStormConfigurationSection Configuration { get; }
        void LoginUser(string loginName);
        void LoginUser(string loginName, bool rememberMe);
        void LogoutUser();
        object GetItem(string name);
        void SetItem(string name, object item);
        void AddCookie(HttpCookie cookie);
        HttpCookie GetCookie(string name);
        void Refresh();
        void LoadCookie();
        void SaveCookie();
        bool IsSsl { get; }
    }

    public class StormContext : IStormContext
    {
        private readonly IRepository repository;
        private readonly IHttpContextWrapper httpContextWrapper;

        #region Singleton stuff

        private static volatile IStormContext instance;
        private static readonly object SyncRoot = new object();

        protected StormContext()
        {
            repository = new Repository();
            httpContextWrapper = new HttpContextWrapper();
        }

        public StormContext(IRepository repository, IHttpContextWrapper httpContextWrapper)
        {
            this.repository = repository;
            this.httpContextWrapper = httpContextWrapper;
        }

        public static void SetInstance(IStormContext newInstance)
        {
            instance = newInstance;
        }

        private static IStormContext Instance
        {
            get
            {
                if (instance != null) return instance;
                lock (SyncRoot)
                {
                    if (instance != null) return instance;
                    {
                        instance = new StormContext();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region IStormContext implementation

        string IStormContext.LoginName
        {
            get { return StormPersisted.LoginName; }
            set { StormPersisted.LoginName = value; }
        }

        int? IStormContext.AccountId
        {
            get { return StormPersisted.AccountId; }
            set { StormPersisted.AccountId = value; }
        }

        int? IStormContext.CustomerId
        {
            get { return StormPersisted.CustomerId; }
            set { StormPersisted.CustomerId = value; }
        }

        int? IStormContext.CompanyId
        {
            get { return StormPersisted.CompanyId; }
            set { StormPersisted.CompanyId = value; }
        }

        int? IStormContext.DivisionId
        {
            get { return StormPersisted.DivisionId; }
            set { StormPersisted.DivisionId = value; }
        }

        int? IStormContext.BasketId
        {
            get { return StormPersisted.BasketId; }
            set { StormPersisted.BasketId = value; }
        }

        int? IStormContext.ReferId
        {
            get { return StormPersisted.ReferId; }
            set { StormPersisted.ReferId = value; }
        }

        string IStormContext.ReferUrl
        {
            get { return StormSession.ReferUrl; }
            set { StormSession.ReferUrl = value; }
        }

        bool IStormContext.RememberMe
        {
            get { return StormPersisted.RememberMe.GetValueOrDefault(true); }
            set { StormPersisted.RememberMe = value; }
        }

        int? IStormContext.ConfirmedBasketId
        {
            get { return StormSession.ConfirmedBasketId; }
            set { StormSession.ConfirmedBasketId = value; }
        }

        void IStormContext.LoginUser(string loginName)
        {
            LoginUser(loginName, true);
        }

        void IStormContext.LoginUser(string loginName, bool rememberMe)
        {
            var customer = string.IsNullOrWhiteSpace(loginName) ? null : repository.GetCustomerByLoginName(loginName, CultureCode);

            StormPersisted.LoginName = customer?.Account?.LoginName;
            StormPersisted.AccountId = customer?.Account?.Id;
            StormPersisted.CompanyId = customer?.Companies != null && customer.Companies.Count > 0 ? customer.Companies[0].Id : null;
            StormPersisted.CustomerId = customer?.Id;
            StormPersisted.RememberMe = customer != null && rememberMe;

            if (StormPersisted.BasketId == null || customer == null) return;

            var basket = repository.GetBasket(StormPersisted.BasketId.Value, PriceListIdSeed, CultureCode, CurrencyId);
            if (customer.Id != null && (basket == null || (basket.CustomerId.HasValue && basket.CustomerId.Value != customer.Id.Value)))
            {
                StormPersisted.BasketId = null;
            }
            else
            {
                basket.CompanyId = StormPersisted.CompanyId;
                customer.Companies?.RemoveAll(c => c.Id != basket.CompanyId);
                if (basket.Id.HasValue) repository.UpdateBuyer(basket.Id.Value, customer, StormPersisted.AccountId, PriceListIdSeed, CultureCode, CurrencyId);
            }
        }

        void IStormContext.LogoutUser()
        {
            if (StormPersisted.BasketId != null)
            {
                var basket = repository.GetBasket(StormPersisted.BasketId.Value, null, CultureCode, CurrencyId);
                if (basket != null)
                {
                    var key = Guid.NewGuid();
                    var customer = new Customers.Customer
                    {
                        Key = key,
                        Account = new Customers.Account { Key = key },
                        DeliveryAddresses = new Customers.AddressList(),
                        Companies = new Customers.CompanyList()
                    };
                    if (basket.Id.HasValue) repository.UpdateBuyer(basket.Id.Value, customer, StormPersisted.AccountId, PriceListIdSeed, CultureCode, CurrencyId);
                }
            }

            StormPersisted.LoginName = null;
            StormPersisted.AccountId = null;
            StormPersisted.CompanyId = null;
            StormPersisted.CustomerId = null;
        }

        string IStormContext.CultureCode
        {
            get { return StormPersisted.CultureCode ?? System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; }
            set { StormPersisted.CultureCode = value; }
        }

        int IStormContext.CurrencyId
        {
            get { return StormPersisted.CurrencyId; }
            set { StormPersisted.CurrencyId = value; }
        }

        List<int> IStormContext.PriceListIds => StormSession.PriceListIds;

        string IStormContext.PriceListIdSeed
        {
            get
            {
                if (PriceListIds == null || PriceListIds.Count == 0) return null;
                return string.Join(",", PriceListIds.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray());
            }
        }

        int IStormContext.SalesAreaId
        {
            get { return StormPersisted.SalesAreaId; }
            set { StormPersisted.SalesAreaId = value; }
        }

        bool? IStormContext.ShowPricesIncVat
        {
            get { return StormPersisted.ShowPricesIncVat; }
            set { StormPersisted.ShowPricesIncVat = value; }
        }

        bool IStormContext.IsInitialRequest
        {
            get { return StormSession.IsInitialRequest; }
            set { StormSession.IsInitialRequest = value; }
        }

        NameValueCollection IStormContext.SessionItems => StormSession.Items;

        NameValueCollection IStormContext.PersistedItems => StormPersisted.Items;

        IStormConfigurationSection IStormContext.Configuration => (StormConfigurationSection)ConfigurationManager.GetSection("stormSettings/storm");

        object IStormContext.GetItem(string name)
        {
            return httpContextWrapper.Items[name];
        }

        void IStormContext.SetItem(string name, object item)
        {
            httpContextWrapper.Items[name] = item;
        }

        void IStormContext.AddCookie(HttpCookie cookie)
        {
            if (httpContextWrapper.RequestCookies.AllKeys.Contains(cookie.Name)) httpContextWrapper.RequestCookies.Set(cookie);
            else httpContextWrapper.RequestCookies.Add(cookie);

            if (httpContextWrapper.ResponseCookies.AllKeys.Contains(cookie.Name)) httpContextWrapper.ResponseCookies.Set(cookie);
            else httpContextWrapper.ResponseCookies.Add(cookie);
        }

        HttpCookie IStormContext.GetCookie(string name)
        {
            return httpContextWrapper.RequestCookies[name];
        }       
       
        void IStormContext.Refresh()
        {
            StormPersisted.SetDirty();
        }

        void IStormContext.LoadCookie()
        {
            var application = repository.GetApplication(Configuration.DefaultCulture);

            new SessionCookie().LoadCookie(stormSessionKey);
            new PersistedCookie(application).LoadCookie(stormPersistedKey);
            StormSession.CookieLoaded = true;

            if (IsValidCookie(application)) return;

            var cookie = GetCookie(stormPersistedKey);
            cookie.Value = "";
            LoadCookie();
        }

        private bool IsValidCookie(Application application)
        {
            return
                application.Cultures.List.Select(c => c.Code.ToLower()).Contains(CultureCode.ToLower()) &&
                application.SalesAreas.List.Select(c => c.Id).Contains(SalesAreaId) &&
                application.Currencies.List.Select(c => c.Id).Contains(CurrencyId);
        }

        void IStormContext.SaveCookie()
        {
            if (!StormSession.CookieLoaded) return;

            StormSession.SaveCookie(stormSessionKey, null);
            StormPersisted.SaveCookie(stormPersistedKey, DateTime.Now.AddDays(Configuration.CookieExpirationDays));
        }

        bool IStormContext.IsSsl => httpContextWrapper.IsSsl;
        #endregion

        #region Static stuff

        public static string LoginName
        {
            get { return Instance.LoginName; }
            set { Instance.LoginName = value; }
        }

        public static int? AccountId
        {
            get { return Instance.AccountId; }
            set { Instance.AccountId = value; }
        }

        public static int? CustomerId
        {
            get { return Instance.CustomerId; }
            set { Instance.CustomerId = value; }
        }

        public static int? CompanyId
        {
            get { return Instance.CompanyId; }
            set { Instance.CompanyId = value; }
        }

        public static int? DivisionId
        {
            get { return Instance.DivisionId; }
            set { Instance.DivisionId = value; }
        }

        public static int? BasketId
        {
            get { return Instance.BasketId; }
            set { Instance.BasketId = value; }
        }

        public static int? ReferId
        {
            get { return Instance.ReferId; }
            set { Instance.ReferId = value; }
        }

        public static string ReferUrl
        {
            get { return Instance.ReferUrl; }
            set { Instance.ReferUrl = value; }
        }

        public static bool RememberMe
        {
            get { return Instance.RememberMe; }
            set { Instance.RememberMe = value; }
        }

        public static int? ConfirmedBasketId
        {
            get { return Instance.ConfirmedBasketId; }
            set { Instance.ConfirmedBasketId = value; }
        }

        public static void LoginUser(string loginName)
        {
            Instance.LoginUser(loginName, true);
        }

        public static void LoginUser(string loginName, bool rememberMe)
        {
            Instance.LoginUser(loginName, rememberMe);
        }

        public static void LogoutUser()
        { 
            Instance.LogoutUser();
        }

        public static string CultureCode
        {
            get { return Instance.CultureCode; }
            set { Instance.CultureCode = value; }
        }

        public static int CurrencyId
        {
            get { return Instance.CurrencyId; }
            set { Instance.CurrencyId = value; }
        }

        public static List<int> PriceListIds
        {
            get { return Instance.PriceListIds; }
        }

        public static string PriceListIdSeed
        {
            get { return Instance.PriceListIdSeed; }
        }

        public static int SalesAreaId
        {
            get { return Instance.SalesAreaId; }
            set { Instance.SalesAreaId = value; }
        }

        public static bool? ShowPricesIncVat
        {
            get { return Instance.ShowPricesIncVat; }
            set { Instance.ShowPricesIncVat = value; }
        }

        public static bool IsInitialRequest
        {
            get { return Instance.IsInitialRequest; }
            set { Instance.IsInitialRequest = value; }
        }

        public static NameValueCollection SessionItems
        {
            get { return Instance.SessionItems; }
        }

        public static NameValueCollection PersistedItems
        {
            get { return Instance.PersistedItems; }
        }

        public static IStormConfigurationSection Configuration
        {
            get { return Instance.Configuration; }
        }

        public static void SetItem(string name, object item)
        {
            Instance.SetItem(name, item);
        }

        public static void AddCookie(HttpCookie cookie)
        {
            Instance.AddCookie(cookie);
        }

        public static HttpCookie GetCookie(string name)
        {
            return Instance.GetCookie(name);
        }

        public static void Refresh()
        {
            Instance.Refresh();
        }

        public static void SaveCookie()
        {
            Instance.SaveCookie();
        }

        public static void LoadCookie()
        {
            Instance.LoadCookie();
        }

        public static bool IsSsl => Instance.IsSsl;
        
        private const string stormSessionKey = "StormSession";
        private static SessionCookie StormSession
        {
            get
            {
                if (Instance.GetItem(stormSessionKey) == null) Instance.LoadCookie();
                return (SessionCookie)Instance.GetItem(stormSessionKey);
            }
        }

        private const string stormPersistedKey = "StormPersisted";
        private static PersistedCookie StormPersisted
        {
            get
            {
                if (Instance.GetItem(stormPersistedKey) == null) Instance.LoadCookie();
                return (PersistedCookie)Instance.GetItem(stormPersistedKey);
            }
        }
        #endregion
    }
}
