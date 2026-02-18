using DotPulsar.Abstractions;

namespace C8yServices.MqttService;

public interface IPulsarService
{
  string Tenant { get; }
  
  /// <summary>
  /// Gets the topic name for consuming messages from devices.
  /// Format: persistent://&lt;tenantID&gt;/mqtt/from-device
  /// </summary>
  string FromDeviceTopic { get; }
  
  /// <summary>
  /// Gets the topic name for sending messages to devices.
  /// Format: persistent://&lt;tenantID&gt;/mqtt/to-device
  /// </summary>
  string ToDeviceTopic { get; }
  
  /// <summary>
  /// Creates a consumer for the specified subscription and topic.
  /// If the subscription already exists on the server, connects to it.
  /// If not, creates a new subscription starting from the latest message.
  /// </summary>
  IPulsarConsumer CreateConsumer(string subscription, string topic);
  
  /// <summary>
  /// Creates a producer for the specified topic.
  /// </summary>
  IPulsarProducer CreateProducer(string topic);
  
  /// <summary>
  /// Gets all active consumers managed by this service.
  /// </summary>
  IReadOnlyList<IPulsarConsumer> GetActiveConsumers();
}
