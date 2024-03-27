using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SubscriptionHandlingExample.Models;

namespace SubscriptionHandlingExample.Services;
public interface ISubscriptionEventService
{
  Task<IReadOnlyList<SubscriptionAddedEvent>?> GetAllSubscriptionAddedEvents(CancellationToken cancellationToken);
  Task<IReadOnlyList<SubscriptionRemovedEvent>?> GetAllSubscriptionRemovedEvents(CancellationToken cancellationToken);
  Task<bool> RemoveAllSubscriptionAddedEvents(CancellationToken cancellationToken);
  Task<bool> RemoveAllSubscriptionRemovedEvents(CancellationToken cancellationToken);
  Task<SubscriptionAddedEvent?> CreateSubscriptionAddedEvent(string tenant, CancellationToken cancellationToken);
  Task<SubscriptionRemovedEvent?> CreateSubscriptionRemovedEvent(string tenant, CancellationToken cancellationToken);
}
