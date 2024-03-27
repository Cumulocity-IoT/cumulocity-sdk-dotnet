using System;
using System.Diagnostics;
using System.Reflection;

using C8yServices.Bootstrapping;
using C8yServices.Extensions.Hosting;
using C8yServices.HealthAndMetrics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RestControllerExample;
using RestControllerExample.Services;

var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<Startup>(int.TryParse(Environment.GetEnvironmentVariable("SERVER_PORT"), out var portNumber) ? portNumber : 80)
  .ConfigureServices((context, services) => services
    .AddC8YConfigurationFromCumulocityPlatform()  // adds IConfiguration<C8YConfiguration> to be used for accessing bootstrapping information
    .AddCumulocityCoreLibraryProvider() // adds IRootCumulocityApiProvider to be used for accessing subscribed subtenants
    .AddSingleton<IExampleUserService, ExampleUserService>()
    .AddSingleton<IExampleManagedObjectService, ExampleManagedObjectService>()) // adds custom logic to dependencies
  .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting example microservice...");
logger.LogInformation("########## {AssemblyName} ##########", typeof(Program).Assembly.GetName().Name);
logger.LogInformation("AssemblyVersion: {AssemblyVersion}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);

var bootstrapService = host.Services.GetRequiredService<ICumulocityCoreLibrayFactory>(); // fetch the bootstrap service from the services
await bootstrapService.InitOrRefresh(); // initialize current service users at startup
host.Services.GetRequiredService<CumulocityCoreLibrayFactoryCredentialRefresh>().Start(); // start process to periodically check for new subscriptions

await host.RunAsync();