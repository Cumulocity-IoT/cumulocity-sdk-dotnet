using System.Diagnostics.CodeAnalysis;

using C8yServices.Notifications.Services;

namespace C8yServices.Notifications.Models;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed class WithHandlerRegisterNotification
{
  public WithHandlerRegisterNotification(RegisterNotification registerNotification, IDataFeedHandler dataFeedHandler)
  {
    RegisterNotification = registerNotification;
    DataFeedHandler = dataFeedHandler;
  }

  public RegisterNotification RegisterNotification { get; }

  public IDataFeedHandler DataFeedHandler { get; }
}