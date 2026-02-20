namespace C8yServices.Subscriptions;

public interface IServiceCredentialsFactoryHelper
{
  Task<IEnumerable<ServiceCredentials>> GetApiCredentials(CancellationToken token = default);
}
