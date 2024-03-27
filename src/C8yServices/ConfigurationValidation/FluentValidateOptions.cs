

using System.Linq;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace C8yServices.ConfigurationValidation;

public sealed class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
  private readonly IValidator<TOptions> _validator;

  public FluentValidateOptions(IValidator<TOptions> validator) => _validator = validator;

  public ValidateOptionsResult Validate(string? name, TOptions options)
  {
    var validationResults = _validator.Validate(options);
    if (validationResults.IsValid)
    {
      return ValidateOptionsResult.Success;
    }
    var failures = validationResults.Errors.Select(static failure => $"Property: {failure.PropertyName}, ErrorMessage: {failure.ErrorMessage}");

    return ValidateOptionsResult.Fail(failures);
  }
}