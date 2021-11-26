using Enferno.Public.Caching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Enferno.StormApiClient.OAuth2
{
    public class CacheableOAuth2TokenResolver : IOAuth2TokenResolver
    {
        private const string CacheKey = "OAuth2Token";
        private readonly OAuth2TokenResolver oAuth2TokenResolver;
        private readonly ICacheManager cacheManager;
        private readonly string cacheName;

        public CacheableOAuth2TokenResolver(
            string cacheName,
            OAuth2TokenResolver oAuth2TokenResolver)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentException($"'{nameof(cacheName)}' cannot be null or whitespace.", nameof(cacheName));
            }

            this.cacheName = cacheName;
            this.oAuth2TokenResolver = oAuth2TokenResolver ?? throw new ArgumentNullException(nameof(oAuth2TokenResolver));
            cacheManager = CacheManager.Instance;
        }

        public async Task<OAuth2Token> GetToken(OAuth2Credentials parameters)
        {
            OAuth2Token token;

            if (cacheManager.TryGet<OAuth2Token>(cacheName, CacheKey, out token)
                && !token.IsExpired)
            {
                return token;
            }

            token = await oAuth2TokenResolver.GetToken(parameters).ConfigureAwait(false);

            cacheManager.Add(cacheName, CacheKey, token);

            return token;
        }
    }
}
