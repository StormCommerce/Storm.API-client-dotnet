namespace Enferno.StormApiClient.Test.Builders
{
    public class Builder
    {
        public static OAuth2CredentialsBuilder OAuth2Credentials => new OAuth2CredentialsBuilder();
        public static OAuth2TokenBuilder OAuth2Token => new OAuth2TokenBuilder();
    }
}
