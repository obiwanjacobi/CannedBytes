namespace CannedBytes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Threading;

    /// <summary>
    /// Registration information for an event handler.
    /// </summary>
    /// <typeparam name="TUserData">The type of user data to use.</typeparam>
    public sealed class EventHandlerRegistration<TUserData>
    {
        /// <summary>
        /// Constructs a new instance for the specified <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">Must not be null.</param>
        internal EventHandlerRegistration(Delegate handler)
        {
            Check.IfArgumentNull(handler, "handler");

            this.Handler = handler;
        }

        /// <summary>
        /// Gets the event handler delegate. Never null.
        /// </summary>
        public Delegate Handler { get; private set; }

        /// <summary>
        /// Gets or sets the user data (instance).
        /// </summary>
        public TUserData UserData { get; set; }

        /// <summary>
        /// Invokes the <see cref="Handler"/> with the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Usually 2 parameters: object sender and EventArgs (or derived) e.</param>
        public void InvokeHandler(params object[] parameters)
        {
            var handler = this.Handler;

            if (handler != null)
            {
                handler.DynamicInvoke(parameters);
            }
        }

        /// <summary>
        /// Invokes the <see cref="Handler"/> with the specified <paramref name="parameters"/> using the <paramref name="dispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">A dispatcher (WPF) object that synchronizes calling into the UI thread from another thread.</param>
        /// <param name="parameters">Usually 2 parameters: object sender and EventArgs (or derived) e.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public void InvokeHandler(Dispatcher dispatcher, params object[] parameters)
        {
            Check.IfArgumentNull(dispatcher, "dispatcher");

            var handler = this.Handler;

            if (handler != null)
            {
                dispatcher.Invoke(handler, parameters);
            }
        }
    }
}