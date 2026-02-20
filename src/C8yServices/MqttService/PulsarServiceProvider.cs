using C8yServices.Configuration;
using C8yServices.Subscriptions;
using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Options;

namespace C8yServices.MqttService;

public class PulsarServiceProvider : IPulsarServiceProvider, IDisposable
{
	private readonly Dictionary<string, IPulsarService> _services = new();
	private readonly Dictionary<string, IPulsarClient> _clients = new();
	private readonly C8YConfiguration _config;
	private bool _disposed;

	public PulsarServiceProvider(IOptions<C8YConfiguration> config, IServiceCredentialsFactory credentialsFactory)
	{
		_config = config.Value;
		credentialsFactory.ApiCredentialsUpdated += OnApiCredentialsUpdated;
	}

	private void OnApiCredentialsUpdated(object? sender, ServiceCredentials credentials)
	{
		// Check if we already have a service for this tenant
		// Only recreate if this is a new tenant (service doesn't exist yet)
		// This avoids unnecessary disposal/recreation every minute when credentials haven't changed
		if (_services.ContainsKey(credentials.Tenant))
		{
			// Service already exists - skip recreation
			return;
		}

		// Ensure Pulsar URL doesn't have trailing slash (can cause connection issues)
		if (_config.BaseUrlPulsar == null)
			throw new InvalidOperationException("BaseUrlPulsar must be configured.");
		
		// Build URL without trailing slash - Uri class normalizes and may add it back
		var builder = new UriBuilder(_config.BaseUrlPulsar);
		builder.Path = string.Empty; // Remove any path including "/"
		var pulsarUrl = builder.Uri;

		// Create new Pulsar client with updated credentials
		var clientBuilder = PulsarClient.Builder()
			.ServiceUrl(pulsarUrl)
			.Authentication(new PulsarJsonBasicAuth(credentials.Tenant, credentials.User, credentials.Password));
		var client = clientBuilder.Build();
		_clients[credentials.Tenant] = client;
		
		_services[credentials.Tenant] = new PulsarService(credentials.Tenant, client);
	}

	public IPulsarService? GetForTenant(string tenant)
	{
		return _services.TryGetValue(tenant, out var service) ? service : null;
	}

	public IEnumerable<string> GetAllSubscribedTenants()
	{
		return _services.Keys;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose all services first
			foreach (var service in _services.Values)
			{
				(service as IDisposable)?.Dispose();
			}
			_services.Clear();
			
			// Then dispose all Pulsar clients
			foreach (var client in _clients.Values)
			{
				client.DisposeAsync().AsTask().Wait();
			}
			_clients.Clear();
		}

		_disposed = true;
	}
}
