

using System;

using C8yServices.Authentication.Common;
using C8yServices.Extensions.Hosting;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace C8yServices.Authentication.Bearer;

public static class BearerAuthenticationExtensions
{
  public static AuthenticationBuilder AddBearerToken<TVerifier>(this AuthenticationBuilder builder)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBearerToken<TVerifier>(BearerAuthenticationDefaults.AuthenticationScheme, _ => { });

  public static AuthenticationBuilder AddBearerToken<TVerifier>(this AuthenticationBuilder builder, Action<BearerAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBearerToken<TVerifier>(BearerAuthenticationDefaults.AuthenticationScheme, configureOptions);

  public static AuthenticationBuilder AddBearerToken<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, Action<BearerAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBearerToken<TVerifier>(authenticationScheme, displayName: BearerAuthenticationDefaults.AuthenticationScheme, configureOptions: configureOptions);

  public static AuthenticationBuilder AddBearerToken<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BearerAuthenticationOptions> configureOptions)
    where TVerifier : IAuthenticationVerifier
    => builder.AddBearerToken<TVerifier>(authenticationScheme, displayName, configureOptions, ServiceLifetime.Transient);

  public static AuthenticationBuilder AddBearerToken<TVerifier>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BearerAuthenticationOptions> configureOptions, ServiceLifetime verifierLifetime)
    where TVerifier : IAuthenticationVerifier
  {
    builder.Services.AddCurrentUserService();
    builder.Services.TryAdd(new ServiceDescriptor(typeof(IAuthenticationVerifier), typeof(TVerifier), verifierLifetime));
    return builder.AddBearerToken(authenticationScheme, displayName, configureOptions);
  }

  /// <see cref="BearerAuthenticationHandler"/> expects ICurrentUserService and IAuthenticationVerifier to be registered
  private static AuthenticationBuilder AddBearerToken(
    this AuthenticationBuilder builder,
    string authenticationScheme,
    string displayName,
    Action<BearerAuthenticationOptions> configureOptions)
  {
    return builder.AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
  }
}
