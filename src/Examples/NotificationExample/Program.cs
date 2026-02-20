using System.Diagnostics;
using System.Reflection;


using C8yServices.Extensions.HealthAndMetrics;
using C8yServices.Extensions.Hosting;
using C8yServices.Subscriptions;

using NotificationExample;
using NotificationExample.Services;

var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<Startup>(int.TryParse(Environment.GetEnvironmentVariable("SERVER_PORT"), out var portNumber) ? portNumber : 8080)
  .ConfigureServices((context, services) => services
    .AddC8YConfigurationFromCumulocityPlatform()  // adds IConfiguration<C8YConfiguration> to be used for accessing bootstrapping information
    .AddNotifications(context.Configuration) // adds INotificationServiceProvider to be used to create 
    .AddCumulocityCoreLibraryProvider() // adds ICumulocityCoreLibraryProvider to be used for accessing subscribed subtenants 
    .AddSingleton<INotificationManagerService, NotificationManagerService>()
    .AddSingleton<DataFeedHandler>())
  .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting example microservice...");
logger.LogInformation("########## {AssemblyName} ##########", typeof(Program).Assembly.GetName().Name);
logger.LogInformation("AssemblyVersion: {AssemblyVersion}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);

var bootstrapService = host.Services.GetRequiredService<IServiceCredentialsFactory>(); // fetch the core cumulocity library factory
await bootstrapService.InitOrRefresh(); // initialize current service users at startup
host.Services.GetRequiredService<ServiceCredentialsRefreshJob>().Start(); // start process to periodically check for new subscriptions

await host.RunAsync();