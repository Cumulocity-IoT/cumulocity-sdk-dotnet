

using System;

using C8yServices.Authentication.Common;
using C8yServices.Extensions.Hosting;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace C8yServices.Authentication.OAuth;

public static class BasicAuthenticationExtensions
{
  public static AuthenticationBuilder AddOAuthAuthentication<TVerifier>(this AuthenticationBuilder builder)
    where TVerifier : IAuthenticationVerifier
    => builder.AddOAuthAuthentication<TVerifier>(OAuthAuthenticationDefaults.AuthenticationScheme, _ => { });

  public static AuthenticationBuilder AddOAuthAuthentication<TVerifier>(this AuthenticationBuilder builder, Action<OAuthAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddOAuthAuthentication<TVerifier>(OAuthAuthenticationDefaults.AuthenticationScheme, configureOptions);

  public static AuthenticationBuilder AddOAuthAuthentication<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuthAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddOAuthAuthentication<TVerifier>(authenticationScheme, configureOptions, ServiceLifetime.Transient);

  public static AuthenticationBuilder AddOAuthAuthentication<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuthAuthenticationOptions> configureOptions, ServiceLifetime verifierLifetime)
      where TVerifier : IAuthenticationVerifier
  {
    builder.Services.AddCurrentUserService();
    builder.Services.AddCurrentTenantService();
    builder.Services.TryAdd(new ServiceDescriptor(typeof(IAuthenticationVerifier), typeof(TVerifier), verifierLifetime));
    return builder.AddOAuthAuthentication(authenticationScheme, configureOptions);
  }

  /// <see cref="OAuthAuthenticationHandler"/> expects IAuthenticationVerifier and ICurrentUserService to be registered
  private static AuthenticationBuilder AddOAuthAuthentication(
    this AuthenticationBuilder builder,
    string authenticationScheme,
    Action<OAuthAuthenticationOptions> configureOptions)
  {
    return builder.AddScheme<OAuthAuthenticationOptions, OAuthAuthenticationHandler>(authenticationScheme, configureOptions);
  }
}