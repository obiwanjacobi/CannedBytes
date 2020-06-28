namespace CannedBytes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class to manage event delegates for multiple events.
    /// </summary>
    /// <typeparam name="TUserData">The type of user data available per registration.</typeparam>
    /// <remarks>If an object support a lot of events or does special processing per event,
    /// it is more efficient to use this collection.</remarks>
    public class EventHandlerCollection<TUserData> : DisposableBase
    {
        /// <summary>A map of event names to event registrations.</summary>
        private readonly Dictionary<string, List<EventHandlerRegistration<TUserData>>> _registrations =
            new Dictionary<string, List<EventHandlerRegistration<TUserData>>>();

        /// <summary>
        /// Retrieves all registrations for a specific <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Can be null or empty.</param>
        /// <returns>Never returns null. An empty list is returned if the <paramref name="eventName"/> was not found.</returns>
        public IEnumerable<EventHandlerRegistration<TUserData>> GetEventRegistrations(string eventName)
        {
            if (_registrations.TryGetValue(eventName, out List<EventHandlerRegistration<TUserData>> regList))
            {
                return regList;
            }

            return Enumerable.Empty<EventHandlerRegistration<TUserData>>();
        }

        /// <summary>
        /// Adds an event registration to the collection for the specified <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Can be null or empty.</param>
        /// <param name="handler">The actual event handler delegate. Must not be null.</param>
        /// <returns>Returns the registration object that contains the user data. Never returns null.</returns>
        public EventHandlerRegistration<TUserData> AddEventRegistration(string eventName, Delegate handler)
        {
            Check.IfArgumentNull(handler, nameof(handler));

            if (!_registrations.TryGetValue(eventName, out List<EventHandlerRegistration<TUserData>> regList))
            {
                regList = new List<EventHandlerRegistration<TUserData>>();
                _registrations.Add(eventName, regList);
            }

            var reg = new EventHandlerRegistration<TUserData>(handler);
            regList.Add(reg);

            return reg;
        }

        /// <summary>
        /// Removes an event registration from the collection for the specified <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Can be null or empty.</param>
        /// <param name="handler">Must not be null.</param>
        /// <returns>Returns the registration object that is no longer in the collection.</returns>
        public EventHandlerRegistration<TUserData> RemoveEventRegistration(string eventName, Delegate handler)
        {
            Check.IfArgumentNull(handler, nameof(handler));

            if (_registrations.TryGetValue(eventName, out List<EventHandlerRegistration<TUserData>> regList))
            {
                var reg = (from temp in regList
                           where temp.Handler == handler
                           select temp).FirstOrDefault();

                if (reg != null)
                {
                    regList.Remove(reg);
                }

                return reg;
            }

            return null;
        }

        /// <summary>
        /// Gets a list of all the event names.
        /// </summary>
        /// <remarks>Note that event names can be null or empty.</remarks>
        public IEnumerable<string> EventNames
        {
            get { return _registrations.Keys; }
        }

        /// <summary>
        /// Disposes the object by checking all user data objects and calling
        /// <see cref="IDisposable.Dispose"/> if they implement it.
        /// </summary>
        /// <param name="disposeKind">The type of resources to dispose.</param>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            foreach (var regList in _registrations.Values)
            {
                foreach (var reg in regList)
                {
                    if (reg.UserData is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}