using DotPulsar;
using DotPulsar.Abstractions;

namespace C8yServices.MqttService;

/// <summary>
/// Wrapper around DotPulsar producer providing additional metadata and control.
/// </summary>
public class PulsarProducer : IPulsarProducer
{
  private readonly IProducer<byte[]> _producer;
  private bool _disposed;

  public string Tenant { get; }
  public string Topic { get; }
  public DateTime CreatedAt { get; }

  internal PulsarProducer(string tenant, string topic, IProducer<byte[]> producer)
  {
    Tenant = tenant;
    Topic = topic;
    CreatedAt = DateTime.UtcNow;
    _producer = producer;
  }

  /// <summary>
  /// Gets the underlying DotPulsar producer for advanced operations.
  /// </summary>
  public IProducer<byte[]> Producer => _producer;

  /// <summary>
  /// Sends a message to the topic.
  /// </summary>
  public ValueTask<MessageId> SendAsync(MessageMetadata metadata, byte[] payload, CancellationToken cancellationToken = default)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    return _producer.Send(metadata, payload, cancellationToken);
  }

  public async ValueTask DisposeAsync()
  {
    if (_disposed)
      return;

    await _producer.DisposeAsync().ConfigureAwait(false);
    _disposed = true;
    GC.SuppressFinalize(this);
  }
}
