using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal abstract record Subscription : IEqualityOperators<Subscription, Subscription, bool>
{
  protected Subscription(string name, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null)
  {
    Name = name;
    FragmentsToCopy = fragmentsToCopy ?? new List<string>();
    NonPersistent = nonPersistent; 
  }

  public string Name { get; }
  public IReadOnlyCollection<string> FragmentsToCopy { get; }
  public bool? NonPersistent { get; }
}