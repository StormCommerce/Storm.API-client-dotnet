using Enferno.StormApiClient.OAuth2;

namespace Enferno.StormApiClient.Test.Builders
{
    public class OAuth2TokenBuilder : AbstractBuilder<OAuth2Token>
    {
        private string accessToken;
        private string tokenType;
        private long expiresInSeconds;
        private string scope;

        public OAuth2TokenBuilder()
        {
            accessToken = "access token";
            tokenType = "token type";
            expiresInSeconds = 0;
            scope = "scope";
        }

        public OAuth2TokenBuilder WithAccessToken(string accessToken)
        {
            this.accessToken = accessToken;

            return this;
        }
        public OAuth2TokenBuilder WithTokenType(string tokenType)
        {
            this.tokenType = tokenType;

            return this;
        }
        public OAuth2TokenBuilder WithExpiresInSeconds(long expiresInSeconds)
        {
            this.expiresInSeconds = expiresInSeconds;

            return this;
        }
        public OAuth2TokenBuilder WithScope(string scope)
        {
            this.scope = scope;

            return this;
        }

        public override OAuth2Token Build()
        {
            return new OAuth2Token(
                accessToken,
                tokenType,
                expiresInSeconds,
                scope);
        }
    }
}
