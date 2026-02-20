using DotPulsar.Abstractions;

namespace C8yServices.MqttService;

/// <summary>
/// Provides a wrapper around DotPulsar consumer with additional metadata and control operations.
/// </summary>
public interface IPulsarConsumer : IAsyncDisposable
{
	/// <summary>
	/// Gets the tenant identifier for this consumer.
	/// </summary>
	string Tenant { get; }

	/// <summary>
	/// Gets the topic this consumer is subscribed to.
	/// </summary>
	string Topic { get; }

	/// <summary>
	/// Gets the subscription name for this consumer.
	/// </summary>
	string SubscriptionName { get; }

	/// <summary>
	/// Gets the UTC timestamp when this consumer was created.
	/// </summary>
	DateTime CreatedAt { get; }

	/// <summary>
	/// Gets the underlying DotPulsar consumer for advanced operations.
	/// </summary>
	IConsumer<byte[]> Consumer { get; }

	/// <summary>
	/// Seeks to the earliest available message in the topic.
	/// This resets the subscription to start consuming from the beginning.
	/// </summary>
	Task SeekToEarliest(CancellationToken cancellationToken = default);

	/// <summary>
	/// Seeks to the latest message in the topic.
	/// This skips all pending messages and starts consuming from the next new message.
	/// </summary>
	Task SeekToLatest(CancellationToken cancellationToken = default);

	/// <summary>
	/// Unsubscribes and removes the subscription from the Pulsar broker.
	/// This deletes all pending messages in the subscription.
	/// </summary>
	Task Unsubscribe(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an async enumerable of messages from this consumer.
	/// </summary>
	IAsyncEnumerable<IMessage<byte[]>> Messages(CancellationToken cancellationToken = default);

	/// <summary>
	/// Acknowledges a message as successfully processed.
	/// </summary>
	Task Acknowledge(IMessage<byte[]> message, CancellationToken cancellationToken = default);
}
