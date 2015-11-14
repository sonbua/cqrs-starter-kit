using System;
using System.Collections;

namespace Cafe.Core
{
    public interface IEventStore
    {
        IEnumerable LoadEventsFor<TAggregate>(Guid id);

        void SaveEventsFor<TAggregate>(Guid id, int eventsLoaded, ArrayList newEvents);
    }
}
