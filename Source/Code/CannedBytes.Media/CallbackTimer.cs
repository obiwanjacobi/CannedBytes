namespace CannedBytes.Media
{
    using System;
#if NET4
    using System.Windows.Threading;
#endif

    /// <summary>
    /// Implements a timer that calls back when it fires.
    /// </summary>
    public sealed class CallbackTimer : Timer
    {
        /// <summary>Event name for the callback event.</summary>
        private const string CallbackEventName = "cb";

        /// <summary>Backing field for the callback.</summary>
        private readonly EventHandlerCollection<CallbackRegistration> _callbacks =
            new EventHandlerCollection<CallbackRegistration>();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="mode">The timer mode.</param>
        public CallbackTimer(TimerMode mode)
            : base(mode)
        {
            Period = Timer.MinPeriod;
        }

        /// <summary>
        /// The 'normal' callback event.
        /// </summary>
        public event EventHandler Callback
        {
            add { _callbacks.AddEventRegistration(CallbackEventName, value); }
            remove { _callbacks.RemoveEventRegistration(CallbackEventName, value); }
        }

        /// <summary>
        /// Adds a callback handler to the timer with specified <paramref name="divider"/> and <paramref name="dispatcher"/>.
        /// </summary>
        /// <param name="handler">Must not be null.</param>
        /// <param name="divider">A divider used to lower callback frequency. Use zero (0) or one (1) when not used.</param>
        /// <param name="dispatcher">An optional dispatcher object to synchronize callbacks to the UI thread. Can be null.</param>
#if NET4
        public void AddCallbackHandler(EventHandler handler, int divider, Dispatcher dispatcher)
#else
        public void AddCallbackHandler(EventHandler handler, int divider)
#endif
        {
            var reg = _callbacks.AddEventRegistration(CallbackEventName, handler);

            if (divider <= 0)
            {
                divider = 1;
            }

            reg.UserData = new CallbackRegistration
            {
#if NET4
                Dispatcher = dispatcher,
#endif
                DividerCount = divider,
                RunningDivider = divider
            };
        }

        /// <summary>
        /// Manages counting down dividers and invoking handlers.
        /// </summary>
        /// <param name="registration">The event handler callback registration. Must not be null.</param>
        private void InvokeEventHandler(EventHandlerRegistration<CallbackRegistration> registration)
        {
            Check.IfArgumentNull(registration, nameof(registration));

            if (registration.UserData != null)
            {
                if (registration.UserData.DividerCount > 1)
                {
                    registration.UserData.RunningDivider--;

                    if (registration.UserData.RunningDivider != 0)
                    {
                        // exit, divider not counted down.
                        return;
                    }

                    registration.UserData.RunningDivider = registration.UserData.DividerCount;
                }
#if NET4
                if (registration.UserData.Dispatcher != null)
                {
                    registration.UserData.Dispatcher.BeginInvoke(
                        new Action(() => { InvokeHandlerDirect(registration); }));
                    return;
                }
#endif
            }

            InvokeHandlerDirect(registration);
        }

        /// <summary>
        /// Directly invokes the handler (no synchronization).
        /// </summary>
        /// <param name="registration">Must not be null.</param>
        private void InvokeHandlerDirect(EventHandlerRegistration<CallbackRegistration> registration)
        {
            try
            {
                registration.Handler?.DynamicInvoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                registration.UserData.LastError = e;
            }
        }

        /// <summary>
        /// Event that fires just before the timer starts.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Helper to call the <see cref="Started"/> event.
        /// </summary>
        private void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that fires just after the timer was stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Helper to call the <see cref="Stopped"/> event.
        /// </summary>
        private void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the timer fires, invokes the callback.
        /// </summary>
        protected override void OnTimerExpired()
        {
            try
            {
                foreach (var reg in _callbacks.GetEventRegistrations(CallbackEventName))
                {
                    InvokeEventHandler(reg);
                }
            }
            finally
            {
                base.OnTimerExpired();
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public override void StartTimer()
        {
            try
            {
                if (!IsRunning)
                {
                    OnStarted();
                }
            }
            finally
            {
                base.StartTimer();
            }
        }

        /// <summary>
        /// Stops the timer, no effect when timer is not running.
        /// </summary>
        public override void StopTimer()
        {
            bool wasRunning = IsRunning;

            base.StopTimer();

            if (wasRunning)
            {
                OnStopped();
            }
        }

        /// <summary>
        /// Callback information.
        /// </summary>
        private class CallbackRegistration
        {
#if NET4
            /// <summary>An optional dispatcher to synchronize calling into the UI thread.</summary>
            public Dispatcher Dispatcher { get; set; }
#endif

            /// <summary>A divider that is used for lower callback frequency.</summary>
            public long DividerCount { get; set; }

            /// <summary>The current running divider value.</summary>
            public long RunningDivider { get; set; }

            /// <summary>Last error that occurred during handler invocation.</summary>
            public Exception LastError { get; set; }
        }
    }
}