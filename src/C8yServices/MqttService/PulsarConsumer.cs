using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;

namespace C8yServices.MqttService;

/// <summary>
/// Wrapper around DotPulsar consumer providing additional metadata and control.
/// </summary>
public class PulsarConsumer : IPulsarConsumer
{
	private readonly IConsumer<byte[]> _consumer;
	private bool _disposed;

	public string Tenant { get; }
	public string Topic { get; }
	public string SubscriptionName { get; }
	public DateTime CreatedAt { get; }

	internal PulsarConsumer(string tenant, string topic, string subscriptionName, IConsumer<byte[]> consumer)
	{
		Tenant = tenant;
		Topic = topic;
		SubscriptionName = subscriptionName;
		CreatedAt = DateTime.UtcNow;
		_consumer = consumer;
	}

	/// <summary>
	/// Gets the underlying DotPulsar consumer for advanced operations.
	/// </summary>
	public IConsumer<byte[]> Consumer => _consumer;

	/// <summary>
	/// Seeks to the earliest available message in the topic.
	/// This resets the subscription to start consuming from the beginning.
	/// </summary>
	public async Task SeekToEarliest(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		await _consumer.Seek(MessageId.Earliest, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Seeks to the latest message in the topic.
	/// This skips all pending messages and starts consuming from the next new message.
	/// </summary>
	public async Task SeekToLatest(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		await _consumer.Seek(MessageId.Latest, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Unsubscribes and removes the subscription from the Pulsar broker.
	/// This deletes all pending messages in the subscription.
	/// </summary>
	public async Task Unsubscribe(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		await _consumer.Unsubscribe(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets an async enumerable of messages from this consumer.
	/// </summary>
	public IAsyncEnumerable<IMessage<byte[]>> Messages(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		return _consumer.Messages(cancellationToken);
	}

	/// <summary>
	/// Acknowledges a message as successfully processed.
	/// </summary>
	public async Task Acknowledge(IMessage<byte[]> message, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		await _consumer.Acknowledge(message, cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		await _consumer.DisposeAsync().ConfigureAwait(false);
		_disposed = true;
		GC.SuppressFinalize(this);
	}
}
