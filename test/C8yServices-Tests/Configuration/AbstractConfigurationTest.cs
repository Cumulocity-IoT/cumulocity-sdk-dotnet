using System.Globalization;

using Microsoft.Extensions.Configuration;

namespace C8yServices.Configuration;

public class AbstractConfigurationTest
{
  public const string ValueName = "testName";
  public const string ValueHostname = "www.test.local";
  public const int ValueTimeout = 120;

  [Fact(DisplayName = "test without configuration instance (null)")]
  public void TestNullConfig()
  {
    Assert.Throws<ArgumentNullException>(() => TestConfiguration.GetConfiguration(null));
  }


  [Fact(DisplayName = "test with an empty configuration")]
  public void TestEmptyConfig()
  {
    var config = TestConfiguration.GetConfiguration(GetConfiguration(new()));

    Assert.NotNull(config);
    Assert.Equal(TestConfiguration.DefaultName, config.Name);
    Assert.Equal(TestConfiguration.DefaultHostname, config.HostName);
    Assert.Equal(TestConfiguration.DefaultTimeout, config.Timeout);
  }

  [Fact(DisplayName = "test with an partial configuration")]
  public void TestPartialConfig()
  {
    var config = TestConfiguration.GetConfiguration(GetConfiguration(new()
    {
      { $"{TestConfiguration.Section}:HOST_NAME", ValueHostname }
    }));

    Assert.NotNull(config);
    Assert.Equal(TestConfiguration.DefaultName, config.Name);
    Assert.Equal(ValueHostname, config.HostName);
    Assert.Equal(TestConfiguration.DefaultTimeout, config.Timeout);
  }

  [Fact(DisplayName = "test with a complete configuration")]
  public void TestConfig()
  {
    var config = TestConfiguration.GetConfiguration(GetConfiguration(new()
    {
      { $"{TestConfiguration.Section}:NAME", ValueName },
      { $"{TestConfiguration.Section}:HOST_NAME", ValueHostname },
      { $"{TestConfiguration.Section}:TIMEOUT", ValueTimeout.ToString(CultureInfo.InvariantCulture) }
    }));

    Assert.NotNull(config);
    Assert.Equal(ValueName, config.Name);
    Assert.Equal(ValueHostname, config.HostName);
    Assert.Equal(ValueTimeout, config.Timeout);
  }

  private static IConfiguration GetConfiguration(Dictionary<string, string?> keyValuePairs)
  {
    return new ConfigurationBuilder()
        .AddInMemoryCollection(keyValuePairs)
        .Build();
  }
}

public class TestConfiguration : AbstractConfiguration
{
  public const string Section = "TEST";
  public const string DefaultName = "unknown";
  public const string DefaultHostname = "";
  public const int DefaultTimeout = 30;

  public string? Name { get; set; }

  [ConfigurationKeyName("HOST_NAME")]
  public string? HostName { get; set; }

  public int Timeout { get; set; }

  public static TestConfiguration GetConfiguration(IConfiguration? configuration) => GetConfiguration<TestConfiguration>(configuration!, Section);

  protected override void InitConfiguration(IConfiguration? configuration)
  {
    Name ??= DefaultName;
    HostName ??= DefaultHostname;
    Timeout = Timeout >= DefaultTimeout ? Timeout : DefaultTimeout;
  }
}