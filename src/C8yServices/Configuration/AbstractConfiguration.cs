using Microsoft.Extensions.Configuration;

namespace C8yServices.Configuration;

/// <summary>
/// abstract base class for configuration classes
/// </summary>
public abstract class AbstractConfiguration
{
  /// <summary>
  /// this method can be overridden to do further initializations (is called by <c>GetConfiguration</c>)
  /// </summary>
  /// <param name="configuration">instance of <c>IConfiguration</c></param>
  protected virtual void InitConfiguration(IConfiguration configuration) { }

  /// <summary>
  /// creates a new configuration instance of given type by given <c>IConfiguration</c> instance and section name
  /// </summary>
  /// <typeparam name="T">type of the configuration to get</typeparam>
  /// <param name="configuration">instance of <c>IConfiguration</c> to create the configuration from</param>
  /// <param name="section">name of the section to read the configuration from</param>
  /// <returns>an instance of a configuration of given type</returns>
  /// <exception cref="ArgumentNullException"></exception>
  protected static T GetConfiguration<T>(IConfiguration configuration, string section) where T : AbstractConfiguration, new()
  {
    if (configuration == null)
      throw new ArgumentNullException(nameof(configuration));

    T? configurationT = null;
    try
    {
      configurationT = configuration.GetSection(section).Get<T>();
    }
    catch
    {
      // ignore exceptions, create a new instance
    }

    configurationT ??= new();
    configurationT.InitConfiguration(configuration);
    return configurationT;
  }
}