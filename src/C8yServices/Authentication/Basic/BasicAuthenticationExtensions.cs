using C8yServices.Authentication.Common;
using C8yServices.Extensions.Hosting;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace C8yServices.Authentication.Basic;

public static class BasicAuthenticationExtensions
{
  public static AuthenticationBuilder AddBasicAuthentication<TVerifier>(this AuthenticationBuilder builder)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBasicAuthentication<TVerifier>(BasicAuthenticationDefaults.AuthenticationScheme, _ => { });

  public static AuthenticationBuilder AddBasicAuthentication<TVerifier>(this AuthenticationBuilder builder, Action<BasicAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBasicAuthentication<TVerifier>(BasicAuthenticationDefaults.AuthenticationScheme, configureOptions);

  public static AuthenticationBuilder AddBasicAuthentication<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, Action<BasicAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBasicAuthentication<TVerifier>(authenticationScheme, displayName: BasicAuthenticationDefaults.AuthenticationScheme, configureOptions: configureOptions);

  public static AuthenticationBuilder AddBasicAuthentication<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BasicAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBasicAuthentication<TVerifier>(authenticationScheme, displayName, configureOptions, ServiceLifetime.Transient);

  public static AuthenticationBuilder AddBasicAuthentication<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BasicAuthenticationOptions> configureOptions, ServiceLifetime verifierLifetime)
      where TVerifier : IAuthenticationVerifier
  {
    builder.Services.AddCurrentUserService();
    builder.Services.TryAdd(new ServiceDescriptor(typeof(IAuthenticationVerifier), typeof(TVerifier), verifierLifetime));
    return builder.AddBasicAuthentication(authenticationScheme, displayName, configureOptions);
  }

  /// <see cref="BasicAuthenticationHandler"/> expects ICurrentUserService and IAuthenticationVerifier to be registered
  private static AuthenticationBuilder AddBasicAuthentication(
    this AuthenticationBuilder builder,
    string authenticationScheme,
    string displayName,
    Action<BasicAuthenticationOptions> configureOptions)
  {
    return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
  }
}
