# CumulocityServices
## Introduction
This SDK offers developers c8y functionalities that are commonly used across solutions. It is using the [open api generated library](https://github.com/SoftwareAG/cumulocity-clients-dotnet) which is also available on github and builds additional functionality on top of that.
Developers using this SDK should be familiar with the basic concepts of cumulocity microservice development: https://cumulocity.com/guides/microservice-sdk/concept/
## Functionalities
### Bootstrapping for multi tenant microservices
Bootstrapping means that there is a specific bootstrap user for each applicaton. This user can be used to retrieve service users for every tenant that the microservice is subscribed on. This SDK offers an build in mechanism that periodically checks for new subscriptions and adds them to some provider, which can be used by the developer to access them. 

Following code is used to add the necessary components to the services so we can access them later:
```
var host = Host.CreateDefaultBuilder(args)
  .ConfigureServices((context, services) => services
    // adds configuration to be used for accessing bootstrapping information
    .AddC8YConfigurationFromCumulocityPlatform()  
    // adds ICumulocityCoreLibraryProvider to be used for accessing subscribed subtenants 
    .AddCumulocityCoreLibraryProvider()) 
  .Build();
```

As you can see, we are also adding a configuration. For this, we need the following env variables set:
```
C8Y_BASEURL=<baseUrlOfTheTenant>
C8Y_BOOTSTRAP_TENANT=<tenantWhereTheMicroserviceIsDeployed>
C8Y_BOOTSTRAP_USER=<usernameOfTheBootstrapuser>
C8Y_BOOTSTRAP_PASSWORD=<passwordOfTheBootstrapuser>
```

To initialise the service users once on startup (or whenever you like), the following code can be used:
```
// fetch the bootstrap service from the services
var bootstrapService = host.Services.GetRequiredService<ICumulocityCoreLibrayFactory>(); 
// initialize current service users at startup
await bootstrapService.InitOrRefresh(); 
```

To start the process to periodically update the service users, the following code can be used:
```
// start process to periodically check for new subscriptions
host.Services.GetRequiredService<CumulocityCoreLibrayFactoryCredentialRefresh>().Start(); 
```

To access the ICumulocityCoreLibrary for subscribed tenants you have to use the "ICumulocityCoreLibraryProvider". The interface looks like this:
```
// get tenantIds of all subscribed tenants
IReadOnlyCollection<string> GetAllSubscribedTenants();
// get ICumulocityCoreLibrary for a specific tenant
ICumulocityCoreLibrary? GetForTenant(string tenantId);
// used by the credential refresh task to update credentials
void UpdateCumulocityApiCredentials(IEnumerable<Credentials> credentials);
// can be used to register an event handler when a new subscription has been added
event EventHandler<string> SubscriptionAddedEventHandler;
// can be used to register an event handler when a subscription has been removed
event EventHandler<string> SubscriptionRemovedEventHandler;
```
Note: An example on how to use this can be found in [SubscriptionHandlingExample project](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/tree/main/src/Examples/SubscriptionHandlingExample).

### Authentication
Following commonly used authentication handlers are implemented in this SDK and can be used like the following:
```
// basic auth
services.AddAuthentication().AddBasicAuthentication<AuthenticationVerifier>(); 
// oauth
services.AddAuthentication().AddOAuthAuthentication<AuthenticationVerifier>();  
// bearer token
services.AddAuthentication().AddBearerToken<AuthenticationVerifier>();
```

To use one ore more of these handlers in your controller, you have to annotate the controller like this:
```
[ApiController]
[Authorize(AuthenticationSchemes = $"{BasicAuthenticationDefaults.AuthenticationScheme},{OAuthAuthenticationDefaults.AuthenticationScheme}")] 
public sealed class ExampleController : ControllerBase
```

The user principal contains information about the username, tenant and the c8y roles assigned to the user. It can be accessed inside the controller like this:
```
var userName = User.GetC8yUsername();
var tenant = User.GetC8yTenant();
var roles = User.GetC8yRoles();
```

### Inventory Api Helper
The InventoryApiHelper offers common functionalities when using the inventory api to e.g. fetch all managed objects of a specific type. To achieve that it is extending "ICumulocityCoreLibrary" (see [InventoryApiHelper.cs](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/blob/main/src/C8yServices/Inventory/InventoryApiHelper.cs)).  
Example usage:
```
var managedObjectsOfType = ICumulocityCoreLibrary.RequestAllByQueryAsync<ManagedObject>(query: "type eq 'myType');
```
Please find more detailed examples in the [RestControllerExample project](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/tree/main/src/Examples/RestControllerExample).

### Notification2 integration
With the notification2 integration we are able to create subscriptions and listen to them. Basic information about this can be found [here](https://www.cumulocity.com/api/core/#tag/About-notifications-2.0).  
The handling with the c8y api (see [Notification2 API](https://www.cumulocity.com/api/core/#tag/Notification-2.0-API)) is handled internally, this SDK offers following functionality in an interface:
```
public interface INotificationService : IAsyncDisposable
{
  /// <summary>
  /// Creates or gets subscription in Cumulocity, creates new token and start listening on the handler.
  /// </summary>
  Task<OneOf<Success, TenantSubscriptionError, ApiError>> Register(string tenantId, WithHandlerRegisterNotification withHandlerRegisterNotification, CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops listening on the handler and unsubscribe token in Cumulocity. The subscription is still existing afterwards.
  /// </summary>
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> Unregister(string tenantId, string subscriptionName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops listening on the handler and subscribe token in Cumulocity. This also deletes the subscription in Cumulocity.
  /// </summary>
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets web socket state.
  /// </summary>
  OneOf<WebSocketState, NotFound> GetWebSocketState(string tenantId, string subscriptionName);
}
```
To receive notifcations, you need a implementation of an [IDataFeedHandler](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/tree/main/src/Notifications/Services/IDataFeedHandler.cs) which basically reacts on the notification and contains the business logic of the implementer.

Please find a more detailed example in the [NotificationExampleProject](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/tree/main/src/Examples/NotificationExample), which also contains an example for an *IDataFeedHandler*.

**Caution: Subscriptions (only the ones that are persistent) can take up a lot of resources when not handled properly. So if there is no consumer anymore, the subscription should always be deleted. It is not sufficient to unregister from it because (unless its non persistent) then the backlog for the subscriptions will still be increased until the consumer registers again.**

### Health and metrics
HealthAndMetricsStartup per default offers two endpoints:
- a standard health endpoint (/data/health)
- some default metrics like memory usage and so on (/data/metrics)

To use the default HealthAndMetricsStartup in an project where you don't use any other REST endpoints you can use it like this:
```
var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<HealthAndMetricsStartup>() // optionally you can set a port, default is 5000
```

If you are using additional REST endpoints you can extend the default HealthAndMetricsStartup and configure your own Startup like this:
```
var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<MyStartup>() // optionally you can set a port, default is 5000
```
```
public class MyStartup : HealthAndMetricsStartup
{
    // ...

    override protected void AdditionalConfiguration(IApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
    {
        // my additional configuration
    }

    override protected void AdditionalServiceConfiguration(IServiceCollection services)
    {
        // my additional configuration
    }

    // ...
}
```
To add your own health checks, you can do this:
```
var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<HealthAndMetricsStartup>()
  .AddHealthCheck<CumulocityApiHealthCheck>("cumulocityApi")
```
The CumulocityApiHealthCheck is also included in this SDK and simply tries to access the cumulocity api for its check.

Please find more detailed examples in the [RestControllerExample project](https://github.com/SoftwareAG/cumulocity-sdk-dotnet/tree/main/src/Examples/RestControllerExample).

### Functionalities to be implemented
Following functionalities are not yet implemented but will be added in the future:
- operation handling
- realtime notification handling (Notification 2.0)

## CICD
TBD

## Contribution Guidelines
If you've spotted something that doesn't work as you'd expect, or if you have a new feature you'd like to add, we're happy to accept contributions and bug reports.

For bug reports, please raise an issue directly in this repository by selecting the issues tab and then clicking the new issue button. Please ensure that your bug report is as detailed as possible and allows us to reproduce your issue easily.

In the case of new contributions, please create a new branch from the latest version of main. When your feature is complete and ready to evaluate, raise a new pull request.

--- 

These tools are provided as-is and without warranty or support. They do not constitute part of the Software AG product suite. Users are free to use, fork and modify them, subject to the license agreement. While Software AG welcomes contributions, we cannot guarantee to include every contribution in the master project.

For more information you can Ask a Question in the [TECH Community Forums](https://tech.forums.softwareag.com/tag/Cumulocity-IoT).

Contact us at [TECHcommunity](mailto:Communities@softwareag.com?subject=Github/SoftwareAG) if you have any questions.
