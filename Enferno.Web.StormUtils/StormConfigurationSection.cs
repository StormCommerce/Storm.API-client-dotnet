
using System.Configuration;

namespace Enferno.Web.StormUtils
{
    public interface IStormConfigurationSection
    {
        bool ShowPricesIncVat { get; set; }
        bool IsPrivate { get; set; }
        string AssortmentIdSeed { get; set; }
        string ProductStatusIdSeed { get; set; }
        string ImageUrl { get; set; }
        int CookieExpirationDays { get; set; }
        string DefaultUrl { get; set; }
        string CheckoutUrl { get; set; }
        string ConfirmedUrl { get; set; }
        bool EncryptCookie { get; set; }
        string DefaultCulture { get; set; }
        int MaxNavigationLevels { get; set; }
        bool ProductCountAsVariants { get; set; }
        string PaymentReturnUrl { get; set; }
    }

    public class StormConfigurationSection : ConfigurationSection, IStormConfigurationSection
    {
        public bool ShowPricesIncVat
        {
            get { return ShowPricesIncVatInternal.GetValueOrDefault(true); }
            set { this["showPricesIncVat"] = value; }
        }

        [ConfigurationProperty("showPricesIncVat", DefaultValue = null, IsRequired = false)]
        public bool? ShowPricesIncVatInternal => (bool?)this["showPricesIncVat"];

        [ConfigurationProperty("isPrivate", DefaultValue = "true", IsRequired = false)]
        public bool IsPrivate
        {
            get { return (bool)this["isPrivate"]; }
            set { this["isPrivate"] = value; }
        }

        [ConfigurationProperty("assortmentIdSeed", DefaultValue = "1,2,3,4,5", IsRequired = false)]
        public string AssortmentIdSeed
        {
            get { return (string)this["assortmentIdSeed"]; }
            set { this["assortmentIdSeed"] = value; }
        }

        [ConfigurationProperty("productStatusIdSeed", DefaultValue = "1,2,3", IsRequired = false)]
        public string ProductStatusIdSeed
        {
            get { return (string)this["productStatusIdSeed"]; }
            set { this["productStatusIdSeed"] = value; }
        }

        [ConfigurationProperty("imageUrl", DefaultValue = "http://services.enferno.se/image/", IsRequired = false)]
        public string ImageUrl
        {
            get { return (string)this["imageUrl"]; }
            set { this["imageUrl"] = value; }
        }

        [ConfigurationProperty("cookieExpirationInDays", DefaultValue = "30", IsRequired = false)]
        public int CookieExpirationDays
        {
            get { return (int)this["cookieExpirationInDays"]; }
            set { this["cookieExpirationInDays"] = value; }
        }

        [ConfigurationProperty("defaultUrl", DefaultValue = "~/default.aspx", IsRequired = false)]
        public string DefaultUrl
        {
            get { return (string)this["defaultUrl"]; }
            set { this["defaultUrl"] = value; }
        }

        [ConfigurationProperty("checkoutUrl", DefaultValue = "~/checkout.aspx", IsRequired = false)]
        public string CheckoutUrl
        {
            get { return (string)this["checkoutUrl"]; }
            set { this["checkoutUrl"] = value; }
        }

        [ConfigurationProperty("confirmedUrl", DefaultValue = "~/confirmed.aspx", IsRequired = false)]
        public string ConfirmedUrl
        {
            get { return (string)this["confirmedUrl"]; }
            set { this["confirmedUrl"] = value; }
        }

        [ConfigurationProperty("encryptCookie", DefaultValue = "true", IsRequired = false)]
        public bool EncryptCookie
        {
            get { return (bool)this["encryptCookie"]; }
            set { this["encryptCookie"] = value; }
        }

        [ConfigurationProperty("defaultCulture", DefaultValue = "sv", IsRequired = false)]
        public string DefaultCulture
        {
            get { return (string)this["defaultCulture"]; }
            set { this["defaultCulture"] = value; }
        }

        [ConfigurationProperty("maxNavigationLevels", DefaultValue = "2", IsRequired = false)]
        public int MaxNavigationLevels
        {
            get { return (int)this["maxNavigationLevels"]; }
            set { this["maxNavigationLevels"] = value; }
        }
        [ConfigurationProperty("productCountAsVariants", DefaultValue = "false", IsRequired = false)]
        public bool ProductCountAsVariants
        {
            get { return (bool)this["productCountAsVariants"]; }
            set { this["productCountAsVariants"] = value; }
        }
        [ConfigurationProperty("paymentReturnUrl", DefaultValue = "", IsRequired = false)]
        public string PaymentReturnUrl
        {
            get { return (string)this["paymentReturnUrl"]; }
            set { this["paymentReturnUrl"] = value; }
        }
    }
}
