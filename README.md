# Storm API .NET
**Important note: This component has reached it's end of development and will not be developed further by Norce Commerce. Feel free to branch it and use as a base in your own projects.**

Storm API is a client library for easy integration against the Storm API. This is used when building applications that calls Storm API. It wraps WCF calls to Storm API and provides support for caching and batch calls.

*Requirements*
* Enferno.Public
* Enterprise library logging application block 6.x

Documentation for Storm API can be found at https://docs.norce.io/api-reference/

Watch this project to follow our releases.

## What's new?
	Fixes in 2.5.5
		
		Fixed a bug where pricelists where not sent correctly in ExportHelper.

 	Fixes in 2.5.4

		Updated to latest Model

		* Added new Endpoints
		- CloseBasketForPostPurchaseUpsell
		- InsertItemsPostPurchase
		- IsPostPurchasePossbile
		- ListFlagGroups
		- ListParametricGroups
		- InsertCompanyPaymentMethod2
		- GetCompanyPaymentMethod

		* Added new Entities
		- CompanyPaymentMethod
		- FlagGroupList
		- ParametricGroupList


		* Added field to Entity
		- CommodityCode in Product
		- RelationsMetadata in ProductRelations
		- RelationsMetadata in VariantRelations
		- IsGlobal in CategoryParametric
		- IsGlobal in ParametricInfo

## How to use OAUTH2

To use OAUTH2 you need 4 settings.

    * Client Id 
    * Client Secret
    * Scope
    * Application Id.

- Client Id and Secret is created in Storm Admin. At https://admin.storm.io/admin/settings/user under the OAUTH tab.

- Scope tells us if you are trying to access lab/qa/production data. (This will not point the request to a specific environment, it will only say that you gives the client access to the specified environment.)
    Existing scopes: lab / qa / production

- Application Id is the application that you want to use.


To read these settings we use the Interface IOAuth2CredentialsProvider registed with DI.
You can create your own if you want, or you can use our OAuth2AppSettingsCredentialsProvider.


Our OAuth2AppSettingsCredentialsProvider will read the settings from the appsettings in web.config.

The AccessClient will for each request check IOAuth2CredentialsProvider.ApplicationId to see what application it should use.
    You can override this by using the applicationId parameter when creating an AccessClient.
    So with help of these(parameter in AccessClient,your own IOAuth2CredentialsProvider) you can use multiple applications.

*** If you are using Enferno.Web.StormUtils with multiple applications you must create your own IOAuth2CredentialsProvider. ***
  




** Checklist of changes that may have to be updated after an upgrade.

- Register IOAuth2CredentialsProvider to DI
Ex: in web.config
      <register type="IOAuth2CredentialsProvider" mapTo="OAuth2AppSettingsCredentialsProvider">
        <lifetime type="singleton" /> 
      </register>

Ex: via code:
    IoC.RegisterType<IOAuth2CredentialsProvider, OAuth2AppSettingsCredentialsProvider>();


- If you are using OAuth2AppSettingsCredentialsProvider add these appsettings to your web.config
     
      appSettings:

      API.OAuth2.ClientId     : clientId guid.
      API.OAuth2.ClientSecret : clientsecret guid.
      API.OAuth2.Scope        : lab/qa/production
      API.ApplicationId       : applicationId you want to use


- Update Endpoints in web.config

    attributes under client.endpoint:
        remove: behaviorConfiguration
        change: bindingConfiguration from SOAP -> Auth2-SOAP

    add a new binding under bindings.wsHttpBinding with name "Auth2-SOAP".

    <binding name="Auth2-SOAP" maxReceivedMessageSize="10000000" >
        <security mode="Transport">
        <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
        <message clientCredentialType="None"  />
        </security>
    </binding>



    Remove SOAP under bindings.wsHttpBinding.

    Remove endpointBehaviors: CertificateBehavior.

- If you are using Enferno.Web.StormUtils with multiple applications you must create your own IOAuth2CredentialsProvider.
