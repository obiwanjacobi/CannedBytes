namespace CannedBytes.Media
{
    using System.Threading;

    /// <summary>
    /// Increments a <see cref="Ticks"/> count each time the timer fires.
    /// </summary>
    public class TickTimer : Timer
    {
        /// <summary>
        /// Constructs a new instance as a periodic timer.
        /// </summary>
        public TickTimer()
            : base(TimerMode.Periodic)
        {
        }

        /// <summary>
        /// Increments the <see cref="Ticks"/> count (thread-safe).
        /// </summary>
        protected override void OnTimerExpired()
        {
            Interlocked.Increment(ref this.ticks);

            // don't need to call the base class
            // 'cause it only handles one-shot mode.
        }

        /// <summary>Backing field for the <see cref="Ticks"/> property.</summary>
        private long ticks;

        /// <summary>
        /// Gets the tick count.
        /// </summary>
        public long Ticks
        {
            get { return this.ticks; }
        }

        /// <summary>
        /// Resets the tick count to zero. Timer must be stopped.
        /// </summary>
        public void Reset()
        {
            ThrowIfRunning();

            Interlocked.Exchange(ref this.ticks, 0);
        }
    }
}