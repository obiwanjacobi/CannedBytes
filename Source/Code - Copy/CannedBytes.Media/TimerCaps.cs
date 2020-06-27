namespace CannedBytes.Media
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Unmanaged structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct TimerCaps
    {
        /// <summary>
        /// The maximum period in milliseconds.
        /// </summary>
        public uint PeriodMin;

        /// <summary>
        /// The minimum period in milliseconds.
        /// </summary>
        public uint PeriodMax;
    }
}