namespace CannedBytes.Media
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// P/Invoke declarations for Win32 timer API.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        /// <summary>Flags to not call the <see cref="TimerProc"/> when the timer is closed.</summary>
        public const uint TIME_KILL_SYNCHRONOUS = 0x0100;

        /// <summary>Not used. For documentation.</summary>
        public const uint TIME_CALLBACK_FUNCTION = 0x0000;

        /// <summary>Error code for an invalid parameter.</summary>
        public const uint MMSYSERR_INVALPARAM = 11;

        /// <summary>Error code for no error.</summary>
        public const uint TIMERR_NOERROR = 0x0000;

        /// <summary>
        /// Called by the timer API when the timer expires.
        /// </summary>
        /// <param name="id">The id of the timer instance.</param>
        /// <param name="msg">Not used.</param>
        /// <param name="user">A user defined parameter.</param>
        /// <param name="parameter1">Not used.</param>
        /// <param name="parameter2">Not used.</param>
        public delegate void TimerProc(uint id, uint msg, IntPtr user, IntPtr parameter1, IntPtr parameter2);

        /// <summary>
        /// Retrieves the timer capabilities.
        /// </summary>
        /// <param name="caps">A reference to the <see cref="TimerCaps"/> structure.</param>
        /// <param name="sizeOfTimerCaps">The size of the structure in bytes.</param>
        /// <returns>Returns an error code.</returns>
        [DllImport("winmm.dll")]
        public static extern uint timeGetDevCaps(ref TimerCaps caps, uint sizeOfTimerCaps);

        /// <summary>
        /// Kills the timer.
        /// </summary>
        /// <param name="id">The identification of the timer instance.</param>
        /// <returns>Returns an error code.</returns>
        [DllImport("winmm.dll")]
        public static extern uint timeKillEvent(uint id);

        /// <summary>
        /// The timeSetEvent function starts a specified timer event. The multimedia timer runs in its own thread.
        /// After the event is activated, it calls the specified callback function or sets or pulses the specified event object.
        /// </summary>
        /// <param name="delay">Event delay, in milliseconds. If this value is not in the range
        /// of the minimum and maximum event delays supported by the timer, the function returns an error.</param>
        /// <param name="resolution">Resolution of the timer event, in milliseconds. The resolution increases with
        /// smaller values; a resolution of 0 indicates periodic events should occur with the greatest possible accuracy.
        /// To reduce system overhead, however, you should use the maximum value appropriate for your application.</param>
        /// <param name="proc">Pointer to a callback function that is called once upon expiration of a single event or
        /// periodically upon expiration of periodic events.</param>
        /// <param name="user">User-supplied callback data.</param>
        /// <param name="mode">Timer event type.</param>
        /// <returns>Returns an identifier for the timer event if successful or an error otherwise. This function returns
        /// NULL if it fails and the timer event was not created. This identifier is also passed to the callback function.</returns>
        [DllImport("winmm.dll")]
        public static extern uint timeSetEvent(uint delay, uint resolution, TimerProc proc, IntPtr user, uint mode);

        /// <summary>The size in bytes of the <see cref="TimerCaps"/> structure.</summary>
        public static readonly uint TimerCapsSize = (uint)Marshal.SizeOf(typeof(TimerCaps));
    }
}