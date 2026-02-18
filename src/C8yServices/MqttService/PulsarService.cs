using DotPulsar;
using DotPulsar.Abstractions;

namespace C8yServices.MqttService;

public class PulsarService : IPulsarService, IDisposable
{
  public string Tenant { get; }
  public string FromDeviceTopic => $"persistent://{Tenant}/mqtt/from-device";
  public string ToDeviceTopic => $"persistent://{Tenant}/mqtt/to-device";
  
  private readonly IPulsarClient _client;
  private readonly List<IPulsarConsumer> _consumers = [];
  private readonly List<IPulsarProducer> _producers = [];
  private bool _disposed;

  public PulsarService(string tenant, IPulsarClient client)
  {
    Tenant = tenant;
    _client = client;
  }

  public IPulsarConsumer CreateConsumer(string subscription, string topic)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    var options = new ConsumerOptions<byte[]>(subscription, topic, Schema.ByteArray);
    var dotPulsarConsumer = _client.CreateConsumer(options);
    var consumer = new PulsarConsumer(Tenant, topic, subscription, dotPulsarConsumer);
    _consumers.Add(consumer);
    return consumer;
  }

  public IReadOnlyList<IPulsarConsumer> GetActiveConsumers()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    return _consumers.AsReadOnly();
  }

  public IPulsarProducer CreateProducer(string topic)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    var options = new ProducerOptions<byte[]>(topic, Schema.ByteArray);
    var dotPulsarProducer = _client.CreateProducer(options);
    var producer = new PulsarProducer(Tenant, topic, dotPulsarProducer);
    _producers.Add(producer);
    return producer;
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
      foreach (var consumer in _consumers)
      {
        consumer.DisposeAsync().AsTask().Wait();
      }
      _consumers.Clear();

      foreach (var producer in _producers)
      {
        producer.DisposeAsync().AsTask().Wait();
      }
      _producers.Clear();
    }

    _disposed = true;
  }
}
