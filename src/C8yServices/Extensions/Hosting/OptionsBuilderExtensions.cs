

using C8yServices.ConfigurationValidation;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace C8yServices.Extensions.Hosting;

public static class OptionsBuilderExtensions
{
  public static OptionsBuilder<TOptions> ValidateUsingFluentValidateOptions<TOptions>(this OptionsBuilder<TOptions> builder)
    where TOptions : class
  {
    builder.Services.AddSingleton<IValidateOptions<TOptions>>(provider => new FluentValidateOptions<TOptions>(provider.GetRequiredService<IValidator<TOptions>>()));

    return builder;
  }
}