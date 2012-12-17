namespace CannedBytes.Media
{
    using System;

    /// <summary>
    /// Implements a timer that calls back when it fires.
    /// </summary>
    public sealed class CallbackTimer : Timer
    {
        /// <summary>Backing field for the callback.</summary>
        private EventHandler callback;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="mode">The timer mode.</param>
        /// <param name="callback">Must not be null.</param>
        public CallbackTimer(TimerMode mode, EventHandler callback)
            : base(mode)
        {
            Check.IfArgumentNull(callback, "callback");

            this.callback = callback;
            Period = Timer.MinPeriod;
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
            var handler = this.Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
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
            var handler = this.Stopped;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the timer fires, invokes the callback.
        /// </summary>
        protected override void OnTimerExpired()
        {
            try
            {
                if (this.callback != null)
                {
                    this.callback(this, EventArgs.Empty);
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
                    this.OnStarted();
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
                this.OnStopped();
            }
        }
    }
}