using Enferno.Public.Caching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Enferno.Public.Logging;

namespace Enferno.StormApiClient.OAuth2
{
    public interface IOAuth2TokenResolver
    {
        Task<OAuth2Token> GetToken(OAuth2Credentials parameters);
    }

    public class OAuth2TokenResolver : IOAuth2TokenResolver
    {
        private const string StormIdentityConnectTokenUrl = "https://identity.storm.io/1.0/connect/token";

        private readonly HttpClient httpClient;

        public OAuth2TokenResolver(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<OAuth2Token> GetToken(OAuth2Credentials parameters)
        {
            var requestBody = CreateRequestBody(parameters);
            var tokenRequest = CreateTokenRequest(requestBody);
            Log.LogEntry
                .Categories("TokenDebug")
                .Message("GetToken httpClient")
                .Property("AbsoluteUri", tokenRequest?.RequestUri?.AbsoluteUri)
                .WriteVerbose();
            var httpResponse = await httpClient.SendAsync(tokenRequest).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                Log.LogEntry
                    .Categories("TokenDebug")
                    .Message("GetToken httpClient Failed ")
                    .Property("AbsoluteUri", tokenRequest?.RequestUri?.AbsoluteUri)
                    .Property("IsSuccessStatusCode", httpResponse.IsSuccessStatusCode)
                    .WriteError();

                throw new HttpRequestException(await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

            }

            return GetToken(await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        private static HttpRequestMessage CreateTokenRequest(Dictionary<string, string> requestBody)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri(StormIdentityConnectTokenUrl),
                Content = new FormUrlEncodedContent(requestBody),
                Method = HttpMethod.Post
            };
        }

        private static Dictionary<string, string> CreateRequestBody(OAuth2Credentials parameters)
        {
            var requestBody = new Dictionary<string, string>
            {
                {"client_id", parameters.ClientId},
                {"client_secret", parameters.ClientSecretId},
                {"grant_type", "client_credentials"},
                {"scope", parameters.Scope}
            };

            return requestBody;
        }

        private static OAuth2Token GetToken(string content)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            Log.LogEntry
                .Categories("TokenDebug")
                .Message("GetToken Received")
                .Property("access_token", response["access_token"].ToString())
                .Property("token_type", response["token_type"].ToString())
                .Property("expires_in", response["expires_in"].ToString())
                .Property("scope", response["scope"].ToString())
                .WriteVerbose();
            return new OAuth2Token(
                response["access_token"].ToString(),
                response["token_type"].ToString(),
                (long)response["expires_in"],
                response["scope"].ToString());
        }
    }
}
