using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Enferno.StormApiClient.OAuth2
{
    public class OAuth2Token
    {
        public OAuth2Token(
            string accessToken,
            string tokenType,
            long expiresInSeconds,
            string scope)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(tokenType))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(tokenType));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(scope));
            }

            AccessToken = accessToken;
            TokenType = tokenType;
            Lifetime = TimeSpan.FromSeconds(expiresInSeconds)-TimeSpan.FromSeconds(60);
            ExpirationDate = DateTime.UtcNow + Lifetime;
            Scope = scope;
        }

        public string AccessToken { get; }
        public string TokenType { get; }
        public TimeSpan Lifetime { get; }
        public string Scope { get; }
        public DateTime ExpirationDate { get; }
        public bool IsExpired => ExpirationDate < DateTime.UtcNow;
    }
}
