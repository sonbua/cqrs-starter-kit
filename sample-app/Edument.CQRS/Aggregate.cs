using System;
using System.Collections;

namespace Cafe.Core
{
    /// <summary>
    /// Aggregate base class, which factors out some common infrastructure that
    /// all aggregates have (ID and event application).
    /// </summary>
    public class Aggregate
    {
        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        public int EventsLoaded { get; private set; }

        // TODO: make set non-public
        /// <summary>
        /// The unique ID of the aggregate.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Enumerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="events"></param>
        public void ApplyEvents(IEnumerable events)
        {
            foreach (var @event in events)
            {
                GetType().GetMethod("ApplyOneEvent")
                         .MakeGenericMethod(@event.GetType())
                         .Invoke(this, new object[] {@event});
            }
        }

        /// <summary>
        /// Applies a single event to the aggregate.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        public void ApplyOneEvent<TEvent>(TEvent @event)
        {
            var applier = this as IApplyEvent<TEvent>;

            if (applier == null)
            {
                throw new InvalidOperationException($"Aggregate {GetType().Name} does not know how to apply event {@event.GetType().Name}");
            }

            applier.Apply(@event);
            EventsLoaded++;
        }
    }
}
