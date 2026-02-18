using DotPulsar.Abstractions;

using System.Text;

namespace C8yServices.MqttService;

public class PulsarJsonBasicAuth : IAuthentication
{
  private readonly byte[] _authData;

  public PulsarJsonBasicAuth(string tenantId, string username, string password)
  {
      // Cumulocity Pulsar basic auth requires: "userId:password" format (colon-separated)
      // NOT JSON! The Java client's AuthenticationBasic extracts from JSON but sends as "userId:password"
      // See: https://github.com/apache/pulsar/blob/main/pulsar-client/src/main/java/org/apache/pulsar/client/impl/auth/AuthenticationDataBasic.java
      var userId = $"{tenantId}/{username}";
      var authString = $"{userId}:{password}";
      _authData = Encoding.UTF8.GetBytes(authString);
  }

  // Cumulocity requires "basic" authentication method as per documentation
  // See: https://cumulocity.com/docs/device-integration/mqtt-service/#pulsar-authentication
  public string AuthenticationMethodName => "basic";

  public ValueTask<byte[]> GetAuthenticationData(CancellationToken cancellationToken)
  {
    return new ValueTask<byte[]>(_authData);
  }

  public static ValueTask DisposeAsync() => ValueTask.CompletedTask;
}