using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.OAuth2
{
    public class OAuth2AppSettingsCredentialsProvider : IOAuth2CredentialsProvider
    {
        private OAuth2Credentials credentials;

        public OAuth2AppSettingsCredentialsProvider()
        {

        }

        public OAuth2Credentials GetOAuth2Credentials()
        {
            return credentials ?? (credentials = new OAuth2Credentials(
                ConfigurationManager.AppSettings["API.OAuth2.ClientId"],
                ConfigurationManager.AppSettings["API.OAuth2.ClientSecret"],
                ConfigurationManager.AppSettings["API.OAuth2.Scope"]
            ));
        }

        public int ApplicationId
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["API.ApplicationId"]);
            }
          
        }


    }

    public interface IOAuth2CredentialsProvider
    {
        OAuth2Credentials GetOAuth2Credentials();
        int ApplicationId { get; }
        
    }

}
