namespace CannedBytes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Helper class to manage event delegates for multiple events.
    /// </summary>
    /// <typeparam name="TUserData">The type of user data available per registration.</typeparam>
    /// <remarks>If an object support a lot of events or does special processing per event,
    /// it is more efficient to use this collection.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Still 'Collection' seems to best describe it.")]
    public class EventHandlerCollection<TUserData> : DisposableBase
    {
        /// <summary>An empty result.</summary>
        private IEnumerable<EventHandlerRegistration<TUserData>> empty = new List<EventHandlerRegistration<TUserData>>();

        /// <summary>A map of event names to event registrations.</summary>
        private Dictionary<string, List<EventHandlerRegistration<TUserData>>> registrations =
            new Dictionary<string, List<EventHandlerRegistration<TUserData>>>();

        /// <summary>
        /// Retrieves all registrations for a specific <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Can be null or empty.</param>
        /// <returns>Never returns null. An empty list is returned if the <paramref name="eventName"/> was not found.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I see no options.")]
        public IEnumerable<EventHandlerRegistration<TUserData>> GetEventRegistrations(string eventName)
        {
            List<EventHandlerRegistration<TUserData>> regList = null;

            if (this.registrations.TryGetValue(eventName, out regList))
            {
                return regList;
            }

            return this.empty;
        }

        /// <summary>
        /// Adds an event registration to the collection for the specified <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Can be null or empty.</param>
        /// <param name="handler">The actual event handler delegate. Must not be null.</param>
        /// <returns>Returns the registration object that contains the user data. Never returns null.</returns>
        public EventHandlerRegistration<TUserData> AddEventRegistration(string eventName, Delegate handler)
        {
            Check.IfArgumentNull(handler, "handler");

            List<EventHandlerRegistration<TUserData>> regList = null;

            if (!this.registrations.TryGetValue(eventName, out regList))
            {
                regList = new List<EventHandlerRegistration<TUserData>>();
                this.registrations.Add(eventName, regList);
            }

            Contract.Assume(regList != null);
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
            Check.IfArgumentNull(handler, "handler");

            List<EventHandlerRegistration<TUserData>> regList = null;

            if (this.registrations.TryGetValue(eventName, out regList))
            {
                var reg = (from temp in regList
                           where (Delegate)temp.Handler == (Delegate)handler
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
            get { return this.registrations.Keys; }
        }

        /// <summary>
        /// Disposes the object by checking all user data objects and calling
        /// <see cref="IDisposable.Dispose"/> if they implement it.
        /// </summary>
        /// <param name="disposeKind">The type of resources to dispose.</param>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            foreach (var regList in this.registrations.Values)
            {
                foreach (var reg in regList)
                {
                    var disposable = reg.UserData as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}