using Enferno.Public.Caching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Enferno.Public.Logging;

namespace Enferno.StormApiClient.OAuth2
{
    public class CacheableOAuth2TokenResolver : IOAuth2TokenResolver
    {
        private const string CacheKey = "OAuth2Token";
        private readonly OAuth2TokenResolver oAuth2TokenResolver;
        private readonly string cacheName;
        private object lockObj;
        private static OAuth2Token cachedToken;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public CacheableOAuth2TokenResolver(
            string cacheName,
            OAuth2TokenResolver oAuth2TokenResolver)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentException($"'{nameof(cacheName)}' cannot be null or whitespace.", nameof(cacheName));
            }

            //     this.cacheName = cacheName;
            this.oAuth2TokenResolver = oAuth2TokenResolver ?? throw new ArgumentNullException(nameof(oAuth2TokenResolver));
            //   cacheManager = CacheManager.Instance;
        }

        public async Task<OAuth2Token> GetToken(OAuth2Credentials parameters)
        {


            if (cachedToken != null && !cachedToken.IsExpired)
            {
                Log.LogEntry
                    .Categories("TokenDebug")
                    .Message("Get token from cache")
                    .Property("AccessToken", cachedToken?.AccessToken)
                    .Property("ExpirationDate", cachedToken?.ExpirationDate)
                    .Property("IsExpired", cachedToken?.IsExpired)
                    .Property("Lifetime", cachedToken?.Lifetime)
                    .Property("Scope", cachedToken?.Scope)
                    .Property("TokenType", cachedToken?.TokenType)
                    .WriteVerbose();
                return cachedToken;
            }
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {

                if (cachedToken != null && !cachedToken.IsExpired)
                {
                    return cachedToken;
                }
                cachedToken = await oAuth2TokenResolver.GetToken(parameters).ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return cachedToken;
        }
    }
}
