using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

using C8yServices.Configuration;
using C8yServices.MqttService;
using C8yServices.Subscriptions;

using DotPulsar.Abstractions;
using DotPulsar.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PulsarExample.Services;

/// <summary>
/// Interactive console service for demonstrating Pulsar operations.
/// Shows how to create/remove consumers and producers, send and receive messages.
/// </summary>
[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Example code")]
public class PulsarConsoleService
{
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly Dictionary<string, IPulsarConsumer> _consumers = new();
    private readonly object _consoleLock = new();
    private readonly string _subscriptionName;
    private bool _running = true;

    public PulsarConsoleService(IServiceProvider services, ILogger logger)
    {
        _services = services;
        _logger = logger;
        
        // Get subscription name from configuration or use assembly name as fallback
        var config = services.GetRequiredService<IOptions<C8YConfiguration>>().Value;
        _subscriptionName = config.ApplicationName 
            ?? Assembly.GetEntryAssembly()?.GetName().Name 
            ?? "pulsar-consumer";
        
        logger.LogInformation("Using subscription name: {SubscriptionName}", _subscriptionName);
    }

    public async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("  Cumulocity Pulsar/MQTT Interactive Console");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();
        Console.WriteLine($"Subscription Name: {_subscriptionName}");
        Console.WriteLine();
        Console.WriteLine("IMPORTANT: Pulsar consumers persist on the server side!");
        Console.WriteLine("Always remove consumers when done to prevent message accumulation.");
        Console.WriteLine();

        // Auto-initialize consumers for all available tenants
        await AutoInitializeConsumers();

        while (_running)
        {
            DisplayMenu();
            string? choice;
            lock (_consoleLock)
            {
                choice = Console.ReadLine()?.Trim();
            }

            try
            {
                switch (choice)
                {
                    case "1":
                        await CreateConsumer();
                        break;
                    case "2":
                        await RemoveConsumer();
                        break;
                    case "3":
                        await SendMessage();
                        break;
                    case "4":
                        await ReceiveMessages();
                        break;
                    case "5":
                        await ManageSubscription();
                        break;
                    case "6":
                        ListActiveResources();
                        break;
                    case "7":
                        await CleanupAndExit();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing operation");
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (_running)
            {
                Console.WriteLine("\nPress Enter to continue...");
                lock (_consoleLock)
                {
                    Console.ReadLine();
                }
            }
        }
    }

    private static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("\n=== Pulsar Operations Menu ===");
        Console.WriteLine("1. Create Consumer (from-device)");
        Console.WriteLine("2. Remove Consumer ⚠️  IMPORTANT");
        Console.WriteLine("3. Send Message (to-device)");
        Console.WriteLine("4. Receive Messages (interactive)");
        Console.WriteLine("5. Manage Subscription (seek/reset)");
        Console.WriteLine("6. List Active Resources");
        Console.WriteLine("7. Cleanup and Exit");
        Console.Write("\nChoice: ");
    }

    private Task AutoInitializeConsumers()
    {
        Console.WriteLine("--- Auto-Initializing Consumers ---");
        
        var pulsarServiceProvider = _services.GetRequiredService<IPulsarServiceProvider>();
        var tenants = pulsarServiceProvider.GetAllSubscribedTenants().ToList();

        if (tenants.Count == 0)
        {
            Console.WriteLine("No tenants available yet. Consumers can be created manually later.");
            return Task.CompletedTask;
        }

        Console.WriteLine($"Found {tenants.Count} tenant(s). Checking for existing subscriptions...");
        
        foreach (var tenant in tenants)
        {
            try
            {
                if (_consumers.ContainsKey(tenant))
                {
                    Console.WriteLine($"  Tenant {tenant}: Consumer already exists");
                    continue;
                }

                var pulsarService = pulsarServiceProvider.GetForTenant(tenant);
                if (pulsarService == null)
                {
                    Console.WriteLine($"  Tenant {tenant}: No Pulsar service available");
                    continue;
                }

                var consumer = pulsarService.CreateConsumer(_subscriptionName, pulsarService.FromDeviceTopic);
                _consumers[tenant] = consumer;
                Console.WriteLine($"  ✓ Tenant {tenant}: Consumer created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create consumer for tenant {Tenant}", tenant);
                Console.WriteLine($"  ✗ Tenant {tenant}: Failed - {ex.Message}");
            }
        }

        Console.WriteLine($"\nInitialization complete. {_consumers.Count} consumer(s) created.");
        Console.WriteLine("Use menu option 4 to start listening for messages.");
        Console.WriteLine();
        
        return Task.CompletedTask;
    }

    private Task CreateConsumer()
    {
        Console.WriteLine("\n--- Create Consumer ---");
        
        var tenant = SelectTenant();
        if (tenant == null) return Task.CompletedTask;

        var key = tenant;
        if (_consumers.ContainsKey(key))
        {
            Console.WriteLine($"Consumer already exists for tenant: {key}");
            return Task.CompletedTask;
        }

        var pulsarServiceProvider = _services.GetRequiredService<IPulsarServiceProvider>();
        var pulsarService = pulsarServiceProvider.GetForTenant(tenant);
        
        if (pulsarService == null)
        {
            Console.WriteLine($"No Pulsar service available for tenant: {tenant}");
            return Task.CompletedTask;
        }

        var consumer = pulsarService.CreateConsumer(_subscriptionName, pulsarService.FromDeviceTopic);
        _consumers[key] = consumer;

        Console.WriteLine($"✓ Consumer created for tenant: {key}");
        Console.WriteLine($"  Topic: {pulsarService.FromDeviceTopic}");
        Console.WriteLine($"  Subscription: {_subscriptionName}");
        Console.WriteLine("\n⚠️  Remember: This consumer now persists on the Pulsar server!");
        Console.WriteLine("    Messages will accumulate if not consumed or removed.");
        Console.WriteLine("    Use menu option 4 to start listening for messages.");
        
        return Task.CompletedTask;
    }

    private async Task RemoveConsumer()
    {
        Console.WriteLine("\n--- Remove Consumer ---");
        Console.WriteLine($"Subscription Name: {_subscriptionName}");
        Console.WriteLine("⚠️  IMPORTANT: Removing a consumer involves two steps:");
        Console.WriteLine("   1. Unsubscribe from the Pulsar broker (removes server-side subscription)");
        Console.WriteLine("   2. Dispose the local consumer instance (closes connection)");
        Console.WriteLine();

        if (_consumers.Count == 0)
        {
            Console.WriteLine("No active consumers.");
            return;
        }

        Console.WriteLine("\nActive consumers:");
        var index = 1;
        foreach (var key in _consumers.Keys)
        {
            Console.WriteLine($"{index}. {key}");
            index++;
        }

        Console.Write("\nSelect consumer number to remove: ");
        string? input;
        lock (_consoleLock)
        {
            input = Console.ReadLine();
        }
        if (!int.TryParse(input, out var choice) || choice < 1 || choice > _consumers.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var selectedKey = _consumers.Keys.ElementAt(choice - 1);
        var consumer = _consumers[selectedKey];

        try
        {
            // Unsubscribe from the Pulsar broker (removes subscription and accumulated messages)
            Console.WriteLine($"Unsubscribing from server...");
            await consumer.Unsubscribe();
            Console.WriteLine($"✓ Subscription removed from Pulsar broker");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{SelectedKey}] Failed to unsubscribe from broker. Consumer will be disposed anyway.", selectedKey);
            Console.WriteLine($"⚠️  Warning: Failed to unsubscribe from broker: {ex.Message}");
            Console.WriteLine("   Consumer will be disposed anyway.");
        }

        // Dispose the consumer (closes connection)
        await consumer.DisposeAsync();
        _consumers.Remove(selectedKey);

        Console.WriteLine($"✓ Consumer fully removed for tenant: {selectedKey}");
    }

    private async Task ReceiveMessages()
    {
        Console.WriteLine("\n--- Receive Messages (Interactive) ---");

        if (_consumers.Count == 0)
        {
            Console.WriteLine("No active consumers. Create a consumer first.");
            return;
        }

        Console.WriteLine("\nActive consumers:");
        var index = 1;
        foreach (var key in _consumers.Keys)
        {
            Console.WriteLine($"{index}. Tenant: {key}");
            index++;
        }

        Console.Write("\nSelect consumer number to listen on: ");
        string? input;
        lock (_consoleLock)
        {
            input = Console.ReadLine();
        }
        if (!int.TryParse(input, out var choice) || choice < 1 || choice > _consumers.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var selectedKey = _consumers.Keys.ElementAt(choice - 1);
        var consumer = _consumers[selectedKey];

        Console.WriteLine($"\nListening for messages on tenant: {selectedKey}");
        Console.WriteLine($"Subscription: {_subscriptionName}");
        
        // Check if interactive console is available
        bool isInteractive = !Console.IsInputRedirected;
        
        if (isInteractive)
        {
            Console.WriteLine("Press 'Q' to stop listening and return to menu...");
        }
        else
        {
            Console.WriteLine("Press Ctrl+C to stop listening...");
        }
        Console.WriteLine();

        using var cts = new CancellationTokenSource();
        
        // Start background task to monitor for 'Q' key press only if console is interactive
        Task? keyMonitorTask = null;
        if (isInteractive)
        {
            keyMonitorTask = Task.Run(() =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(intercept: true);
                            if (key.Key == ConsoleKey.Q || key.KeyChar == 'q' || key.KeyChar == 'Q')
                            {
                                lock (_consoleLock)
                                {
                                    Console.WriteLine("\n[Stopping...]");
                                }
#pragma warning disable CA1849 // Call async methods when in an async method
                                cts.Cancel();
#pragma warning restore CA1849
                                break;
                            }
                        }
                        // Short sleep to prevent CPU spinning
                        Thread.Sleep(50);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Console input is redirected or not available - monitor will not work
                    // User will need to use Ctrl+C to stop
                }
            }, cts.Token);
        }

        try
        {
            await foreach (var message in consumer.Messages(cts.Token))
            {
                try
                {
                    var dataSeq = message.Data;
                    byte[] data;
                    if (dataSeq.IsSingleSegment)
                    {
                        data = dataSeq.FirstSpan.ToArray();
                    }
                    else
                    {
                        data = new byte[dataSeq.Length];
                        dataSeq.CopyTo(data);
                    }

                    var json = Encoding.UTF8.GetString(data);
                    
                    lock (_consoleLock)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{selectedKey}] Message received:");
                        Console.WriteLine($"  {json}");
                        Console.WriteLine();
                    }

                    await consumer.Acknowledge(message, cts.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "[{SelectedKey}] Error processing message", selectedKey);
                    lock (_consoleLock)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when user presses 'Q'
        }
        finally
        {
            // Ensure cancellation token is set
            if (!cts.IsCancellationRequested)
            {
                await cts.CancelAsync();
            }
            
            // Wait for key monitor task to complete
            if (keyMonitorTask != null)
            {
                try
                {
                    await keyMonitorTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when task is cancelled
                }
            }
        }

        Console.WriteLine("\nStopped listening.");
    }

    private async Task SendMessage()
    {
        Console.WriteLine("\n--- Send Message ---");
        
        var tenant = SelectTenant();
        if (tenant == null) return;

        Console.Write("\nEnter message to send: ");
        string? message;
        lock (_consoleLock)
        {
            message = Console.ReadLine();
        }
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine("Message cannot be empty.");
            return;
        }

        // Get fresh service reference right before creating producer
        // to ensure we have valid credentials after any potential refresh
        var pulsarServiceProvider = _services.GetRequiredService<IPulsarServiceProvider>();
        var pulsarService = pulsarServiceProvider.GetForTenant(tenant);
        
        if (pulsarService == null)
        {
            Console.WriteLine($"No Pulsar service available for tenant: {tenant}");
            return;
        }

        // Create producer, send message, and dispose
        var producer = pulsarService.CreateProducer(pulsarService.ToDeviceTopic);
        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await producer.Send(messageBytes);
            Console.WriteLine($"\n✓ Message sent to tenant {tenant}");
            Console.WriteLine($"  Topic: {pulsarService.FromDeviceTopic}");
            Console.WriteLine($"  Content: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to tenant {Tenant}", tenant);
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
        finally
        {
            await producer.DisposeAsync();
        }
    }

    private async Task ManageSubscription()
    {
        Console.WriteLine("\n--- Manage Subscription ---");

        if (_consumers.Count == 0)
        {
            Console.WriteLine("No active consumers. Create a consumer first.");
            return;
        }

        Console.WriteLine("\nActive consumers:");
        var index = 1;
        foreach (var (key, consumer) in _consumers)
        {
            Console.WriteLine($"{index}. Tenant: {key}");
            Console.WriteLine($"   Topic: {consumer.Topic}");
            Console.WriteLine($"   Created: {consumer.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            index++;
        }

        Console.Write("\nSelect consumer number: ");
        string? input;
        lock (_consoleLock)
        {
            input = Console.ReadLine();
        }
        if (!int.TryParse(input, out var choice) || choice < 1 || choice > _consumers.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var selectedKey = _consumers.Keys.ElementAt(choice - 1);
        var selectedConsumer = _consumers[selectedKey];

        Console.WriteLine($"\nTenant: {selectedKey}");
        Console.WriteLine($"Subscription: {_subscriptionName}");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("1. Seek to Earliest (start from beginning)");
        Console.WriteLine("2. Seek to Latest (skip pending messages)");
        Console.WriteLine("3. Back to Menu");
        Console.Write("\nChoice: ");
        
        lock (_consoleLock)
        {
            input = Console.ReadLine();
        }

        try
        {
            switch (input)
            {
                case "1":
                    Console.WriteLine("Seeking to earliest message...");
                    await selectedConsumer.SeekToEarliest();
                    Console.WriteLine("✓ Subscription reset to earliest message.");
                    Console.WriteLine("  Next consume will start from the beginning of the topic.");
                    break;
                case "2":
                    Console.WriteLine("Seeking to latest message...");
                    await selectedConsumer.SeekToLatest();
                    Console.WriteLine("✓ Subscription advanced to latest message.");
                    Console.WriteLine("  All pending messages skipped. Will receive only new messages.");
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing subscription");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ListActiveResources()
    {
        Console.WriteLine("\n--- Active Resources ---");
        
        var pulsarServiceProvider = _services.GetRequiredService<IPulsarServiceProvider>();
        
        Console.WriteLine($"\nConsumers ({_consumers.Count}):");
        if (_consumers.Count == 0)
        {
            Console.WriteLine("  (none)");
        }
        else
        {
            foreach (var (key, consumer) in _consumers)
            {
                Console.WriteLine($"  - Tenant: {key}");
                Console.WriteLine($"    Topic: {consumer.Topic}");
                Console.WriteLine($"    Subscription: {consumer.SubscriptionName}");
                Console.WriteLine($"    Created: {consumer.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");

            }
        }

        var tenants = pulsarServiceProvider.GetAllSubscribedTenants().ToList();
        Console.WriteLine($"\nAvailable Tenants ({tenants.Count}):");
        if (tenants.Count == 0)
        {
            Console.WriteLine("  (none - waiting for subscriptions)");
        }
        else
        {
            foreach (var tenant in tenants)
            {
                Console.WriteLine($"  - {tenant}");
            }
        }
    }

    private async Task CleanupAndExit()
    {
        Console.WriteLine("\n--- Cleanup and Exit ---");
        Console.WriteLine("Cleaning up all resources...");

        // Unsubscribe and dispose all consumers
        foreach (var (key, consumer) in _consumers)
        {
            Console.WriteLine($"  Unsubscribing and disposing consumer: {key}");
            try
            {
                await consumer.Unsubscribe();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{Key}] Failed to unsubscribe during cleanup", key);
            }
            await consumer.DisposeAsync();
        }
        _consumers.Clear();

        Console.WriteLine("✓ Cleanup complete.");
        _running = false;
    }

    private string? SelectTenant()
    {
        var pulsarServiceProvider = _services.GetRequiredService<IPulsarServiceProvider>();
        var tenants = pulsarServiceProvider.GetAllSubscribedTenants().ToList();

        if (tenants.Count == 0)
        {
            Console.WriteLine("No tenants available. Waiting for subscriptions...");
            return null;
        }

        if (tenants.Count == 1)
        {
            Console.WriteLine($"Using tenant: {tenants[0]}");
            return tenants[0];
        }

        Console.WriteLine("\nAvailable tenants:");
        for (int i = 0; i < tenants.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tenants[i]}");
        }

        Console.Write("Select tenant number: ");
        string? input;
        lock (_consoleLock)
        {
            input = Console.ReadLine();
        }
        if (!int.TryParse(input, out var choice) || choice < 1 || choice > tenants.Count)
        {
            Console.WriteLine("Invalid selection.");
            return null;
        }

        return tenants[choice - 1];
    }
}
