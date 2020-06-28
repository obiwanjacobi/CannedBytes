namespace CannedBytes.Media
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// An abstract base class for implementing timers.
    /// </summary>
    public abstract class Timer : UnmanagedDisposableBase
    {
        /// <summary>A handle to this instance used in the callback procedure.</summary>
        private GCHandle _instanceHandle;

        /// <summary>The unique id of this timer instance.</summary>
        private uint _timerId;

        /// <summary>Timer capabilities.</summary>
        private static TimerCaps TimerCaps;

        /// <summary>
        /// Fetches the timer capabilities.
        /// </summary>
        private static void LoadTimerCaps()
        {
            var result = NativeMethods.timeGetDevCaps(ref TimerCaps, NativeMethods.TimerCapsSize);

            if (result != NativeMethods.TIMERR_NOERROR)
            {
                throw new TimerException(String.Format(Properties.Resources.Timer_FailedToGetCaps, result));
            }
        }

        /// <summary>
        /// Gets the maximum period time in milliseconds.
        /// </summary>
        public static long MaxPeriod
        {
            get
            {
                if (TimerCaps.PeriodMax == 0)
                {
                    LoadTimerCaps();
                }

                return TimerCaps.PeriodMax;
            }
        }

        /// <summary>
        /// Gets the minimum period time in milliseconds.
        /// </summary>
        public static long MinPeriod
        {
            get
            {
                if (TimerCaps.PeriodMax == 0)
                {
                    LoadTimerCaps();
                }

                return TimerCaps.PeriodMin;
            }
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="timerMode">The mode of the timer.</param>
        protected Timer(TimerMode timerMode)
        {
            _mode = timerMode;
            _instanceHandle = GCHandle.Alloc(this, GCHandleType.Weak);
        }

        /// <summary>
        /// Gets an indication if the timer is running (true) or stopped (false).
        /// </summary>
        public bool IsRunning
        {
            get { return _timerId != 0; }
        }

        /// <summary>Backing field for the <see cref="Period"/> property.</summary>
        private uint _period;

        /// <summary>
        /// Gets or sets the period (recurrence) in milliseconds the timer will fire.
        /// </summary>
        /// <remarks>
        /// The value has to lie between <see cref="MinPeriod"/> and <see cref="MaxPeriod"/>.
        /// Has no effect when the <see cref="Mode"/> is set to <see cref="TimerMode.OneShot"/>.
        /// </remarks>
        public long Period
        {
            get
            {
                return _period;
            }

            set
            {
                ThrowIfDisposed();
                ThrowIfRunning();
                Check.IfArgumentOutOfRange(value, MinPeriod, MaxPeriod, nameof(Period));

                _period = (uint)value;
            }
        }

        /// <summary>Backing field for the <see cref="Resolution"/> property.</summary>
        private uint _resolution;

        /// <summary>
        /// Gets or sets the resolution of the timer in milliseconds.
        /// </summary>
        public long Resolution
        {
            get
            {
                return _resolution;
            }

            set
            {
                ThrowIfDisposed();
                ThrowIfRunning();
                Check.IfArgumentOutOfRange(value, 0, uint.MaxValue, nameof(Resolution));

                _resolution = (uint)value;
            }
        }

        /// <summary>Backing field for the <see cref="Mode"/> property.</summary>
        private TimerMode _mode;

        /// <summary>
        /// Gets or sets the mode of the timer.
        /// </summary>
        public TimerMode Mode
        {
            get
            {
                return _mode;
            }

            protected set
            {
                ThrowIfDisposed();
                ThrowIfRunning();

                _mode = value;
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public virtual void StartTimer()
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _timerId = NativeMethods.timeSetEvent(
                           _period,
                           _resolution,
                           TimerProcedure,
                           ToIntPtr(),
                           (uint)Mode | NativeMethods.TIME_KILL_SYNCHRONOUS);

            if (_timerId == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new TimerException("Could not create the timer.", new Win32Exception(errorCode));
            }
        }

        /// <summary>
        /// Stops the timer, no effect if timer is not running.
        /// </summary>
        public virtual void StopTimer()
        {
            if (IsRunning)
            {
                var result = NativeMethods.timeKillEvent(_timerId);

                if (result == NativeMethods.MMSYSERR_INVALPARAM)
                {
                    throw new TimerException("The timer identification is invalid (already closed?).");
                }

                _timerId = 0;
            }

            ThrowIfDisposed();
        }

        /// <summary>
        /// Returns an <see cref="IntPtr"/> that represents the instance's this reference.
        /// </summary>
        /// <returns>Returns the instance <see cref="IntPtr"/>.</returns>
        /// <remarks>Dereference using <see cref="GCHandle"/>.</remarks>
        public IntPtr ToIntPtr()
        {
            ThrowIfDisposed();

            return GCHandle.ToIntPtr(_instanceHandle);
        }

        /// <summary>
        /// Disposes the object instance.
        /// </summary>
        /// <param name="disposeKind">The type if resources to dispose.</param>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (!IsDisposed && _timerId != 0)
            {
                var result = NativeMethods.timeKillEvent(_timerId);
                _timerId = 0;

                if (result != NativeMethods.TIMERR_NOERROR)
                {
                    Console.WriteLine(Properties.Resources.Timer_FailedToKillTimer);
                }
            }
        }

        /// <summary>
        /// Throws an exception if the timer is running.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the timer is running.</exception>
        protected void ThrowIfRunning()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The timer is running.");
            }
        }

        /// <summary>
        /// Derived classes override this method which is called when the timer fires.
        /// </summary>
        protected virtual void OnTimerExpired()
        {
            if (_mode == TimerMode.OneShot)
            {
                StopTimer();
            }
        }

        /// <summary>
        /// We maintain a reference to the callback procedure so the GC wont take it.
        /// </summary>
        private static readonly NativeMethods.TimerProc TimerProcedure = OnTimerCallback;

        /// <summary>
        /// Called back from the Win32 API when the timer fires.
        /// </summary>
        /// <param name="timerId">The id of the timer.</param>
        /// <param name="msg">Not used.</param>
        /// <param name="userData">A reference to the <see cref="Timer"/> object.</param>
        /// <param name="parameter1">Not used.</param>
        /// <param name="parameter2">Not used.</param>
        private static void OnTimerCallback(uint timerId, uint msg, IntPtr userData, IntPtr parameter1, IntPtr parameter2)
        {
            try
            {
                var instanceHandle = GCHandle.FromIntPtr(userData);

                if (instanceHandle.Target != null)
                {
                    var timer = (Timer)instanceHandle.Target;
                    Debug.Assert(timer._timerId == timerId, "Callback provided an invalid instance or timerId.");

                    timer.OnTimerExpired();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}