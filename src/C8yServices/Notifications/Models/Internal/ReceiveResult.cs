using System.Diagnostics.CodeAnalysis;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class ReceiveResult
{
  public ReceiveResult(bool close, ReadOnlyMemory<byte> data)
  {
    Close = close;
    Data = data;
  }

  public bool Close { get; }

  public ReadOnlyMemory<byte> Data { get; }
}