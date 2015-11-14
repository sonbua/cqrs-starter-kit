using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Cafe.Core;

namespace Cafe.Infrastructure
{
    public class InMemoryEventStore : IEventStore
    {
        private class Stream
        {
            public ArrayList Events;
        }

        private readonly ConcurrentDictionary<Guid, Stream> _store = new ConcurrentDictionary<Guid, Stream>();

        public IEnumerable LoadEventsFor<TAggregate>(Guid id)
        {
            // Get the current event stream; note that we never mutate the
            // Events array so it's safe to return the real thing.
            Stream s;

            if (_store.TryGetValue(id, out s))
            {
                return s.Events;
            }

            return new ArrayList();
        }

        public void SaveEventsFor<TAggregate>(Guid aggregateId, int eventsLoaded, ArrayList newEvents)
        {   
            // Get or create stream.
            var s = _store.GetOrAdd(aggregateId, _ => new Stream());

            // We'll use a lock-free algorithm for the update.
            while (true)
            {
                // Read the current event list.
                var eventList = s.Events;

                // Ensure no events persisted since us.
                var prevEvents = eventList?.Count ?? 0;

                if (prevEvents != eventsLoaded)
                    throw new Exception("Concurrency conflict; cannot persist these events");

                // Create a new event list with existing ones plus our new
                // ones (making new important for lock free algorithm!)
                var newEventList = eventList == null
                    ? new ArrayList()
                    : (ArrayList)eventList.Clone();
                newEventList.AddRange(newEvents);

                // Try to put the new event list in place atomically.
                if (Interlocked.CompareExchange(ref s.Events, newEventList, eventList) == eventList)
                    break;
            }
        }
    }
}
