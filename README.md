# Storm API .NET
Storm API is a client library for easy integration against the Storm API. This is used when building applications that calls Storm API. It wraps WCF calls to Storm API and provides support for caching and batch calls.

*Requirements*
* Enferno.Public
* Enterprise library logging application block 6.x

Documentation for Storm API can be found at https://storm.io/docs/storm-api

Watch this project to follow our releases.

## How to use
### OAuth2 authentication
In order to setup OAuth2 authentication `IOAuth2CredentialsProvider` interface has to be implemented.
Example:
```
public class OAuth2CredentialsProvider : IOAuth2CredentialsProvider
{
    public OAuth2Credentials GetOAuth2Credentials()
        return new OAuth2Credentials(
            clientId: "11111111-1111-1111-1111-111111111111",
            clientSecretId: "22222222-2222-2222-2222-222222222222",
            scope: "lab",
            applicationId: 1);
    }
}
```

Than register the implementation in the IoC:
```
IoC.RegisterType<IOAuth2CredentialsProvider, OAuth2CredentialsProvider>();
```