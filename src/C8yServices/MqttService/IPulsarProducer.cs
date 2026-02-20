using DotPulsar;
using DotPulsar.Abstractions;

namespace C8yServices.MqttService;

/// <summary>
/// Provides a wrapper around DotPulsar producer with additional metadata and control operations.
/// </summary>
public interface IPulsarProducer : IAsyncDisposable
{
    /// <summary>
    /// Gets the tenant identifier for this producer.
    /// </summary>
    string Tenant { get; }

    /// <summary>
    /// Gets the topic this producer is publishing to.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// Gets the UTC timestamp when this producer was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the underlying DotPulsar producer for advanced operations.
    /// </summary>
    IProducer<byte[]> Producer { get; }

    /// <summary>
    /// Sends a message to the topic.
    /// </summary>
    ValueTask<MessageId> SendAsync(MessageMetadata metadata, byte[] payload, CancellationToken cancellationToken = default);
}
