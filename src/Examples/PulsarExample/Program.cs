using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using C8yServices.Extensions.HealthAndMetrics;
using C8yServices.Extensions.Hosting;
using C8yServices.Subscriptions;

using PulsarExample;
using PulsarExample.Services;

[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Example code - simplicity over performance")]

// Build the host with Pulsar services and health endpoint
var host = Host.CreateDefaultBuilder(args)
  .ConfigureHealthAndMetrics<Startup>(int.TryParse(Environment.GetEnvironmentVariable("SERVER_PORT"), out var portNumber) ? portNumber : 8080)
  .ConfigureServices((context, services) => services
    .AddC8YConfigurationFromCumulocityPlatform()
    .AddPulsarServices()
  )
  .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Pulsar Interactive Console Example...");
logger.LogInformation("########## {AssemblyName} ##########", typeof(Program).Assembly.GetName().Name);
logger.LogInformation("AssemblyVersion: {AssemblyVersion}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);

var bootstrapService = host.Services.GetRequiredService<IServiceCredentialsFactory>();
await bootstrapService.InitOrRefresh();
host.Services.GetRequiredService<ServiceCredentialsRefreshJob>().Start();

// Start the web host in the background
_ = host.RunAsync();

logger.LogInformation("Initialized. Waiting for tenant subscriptions...");
await Task.Delay(2000); // Give time for initial subscriptions

// Run the interactive console on the main thread
var consoleService = new PulsarConsoleService(host.Services, logger);
await consoleService.RunAsync();

logger.LogInformation("Shutting down...");
await host.StopAsync();
