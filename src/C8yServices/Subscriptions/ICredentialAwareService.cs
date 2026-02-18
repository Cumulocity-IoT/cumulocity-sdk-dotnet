namespace C8yServices.Subscriptions;

/// <summary>
/// Marker interface for services that need to be initialized before credential update events fire.
/// Services implementing this interface will be automatically resolved and initialized
/// when <see cref="IServiceCredentialsFactory.InitOrRefresh"/> is called.
/// </summary>
public interface ICredentialAwareService
{
}
