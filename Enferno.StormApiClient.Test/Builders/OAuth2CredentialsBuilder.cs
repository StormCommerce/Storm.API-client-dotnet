using Enferno.StormApiClient.OAuth2;

namespace Enferno.StormApiClient.Test.Builders
{
    public class OAuth2CredentialsBuilder : AbstractBuilder<OAuth2Credentials>
    {
        private string clientId;
        private string clientSecretId;
        private string scope;
        private int applicationId;

        public OAuth2CredentialsBuilder()
        {
            clientId = "Client Id";
            clientSecretId = "Client Secret Id";
            scope = "Scope";
            applicationId = 1;
        }

        public OAuth2CredentialsBuilder WithClientId(string clientId)
        {
            this.clientId = clientId;

            return this;
        }
        public OAuth2CredentialsBuilder WithClientSecretId(string clientSecretId)
        {
            this.clientSecretId = clientSecretId;

            return this;
        }
        public OAuth2CredentialsBuilder WithScope(string scope)
        {
            this.scope = scope;

            return this;
        }
        public OAuth2CredentialsBuilder WithApplicationId(int applicationId)
        {
            this.applicationId = applicationId;

            return this;
        }

        public override OAuth2Credentials Build()
        {
            return new OAuth2Credentials(
                clientId,
                clientSecretId,
                scope,
                applicationId);
        }
    }
}
