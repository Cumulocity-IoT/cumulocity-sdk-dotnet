using Microsoft.Extensions.Configuration;

namespace C8yServices.Configuration;

/// <summary>
/// <c>C8YConfiguration</c> contains the Cumulocity platform configuration like base URL, bootstrap credentials,...
/// </summary>
public sealed class C8YConfiguration
{
  /// <summary>
  /// configuration section
  /// </summary>
  public const string Section = "C8Y";

  /// <summary>
  /// base URL of the Cumulocity platform
  /// </summary>
  [ConfigurationKeyName(name: "BaseUrl")]
  public Uri? BaseUrl { get; init; }

  /// <summary>
  /// bootstrap tenant ID
  /// </summary>
  [ConfigurationKeyName(name: "Bootstrap_Tenant")]
  public string BootstrapTenant { get; init; } = string.Empty;

  /// <summary>
  /// bootstrap username
  /// </summary>
  [ConfigurationKeyName(name: "Bootstrap_Username")]
  public string BootstrapUsername { get; init; } = string.Empty;

  /// <summary>
  /// bootstrap password
  /// </summary>
  [ConfigurationKeyName(name: "Bootstrap_Password")]
  public string BootstrapPassword { get; init; } = string.Empty;

  [ConfigurationKeyName(name: "BaseUrl_Pulsar")]
  public Uri? BaseUrlPulsar { get; init; }


  /// <summary>
  /// Cumulocity is providing the environment variables with only one "_" after the section, dotnet default expects "__" after the section
  /// </summary>
  public static C8YConfiguration FromCumulocityPlatform() =>
    new()
    {
      BaseUrl = new Uri(GetVariable("C8Y_BASEURL")),
      BootstrapPassword = GetVariable("C8Y_BOOTSTRAP_PASSWORD"),
      BootstrapTenant = GetVariable("C8Y_BOOTSTRAP_TENANT"),
      BootstrapUsername = GetVariable("C8Y_BOOTSTRAP_USER"),
      BaseUrlPulsar = new Uri(GetVariable("C8Y_BASEURL_PULSAR"))
    };

  private static string GetVariable(string name)
  {
    var value = Environment.GetEnvironmentVariable(name);

    return value ?? throw new ArgumentException($"{nameof(value)} is null.");
  }
}