using Enferno.Public.Caching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

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

            var httpResponse = await httpClient.SendAsync(tokenRequest).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
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

            return new OAuth2Token(
                response["access_token"].ToString(),
                response["token_type"].ToString(),
                (long)response["expires_in"],
                response["scope"].ToString());
        }
    }
}
