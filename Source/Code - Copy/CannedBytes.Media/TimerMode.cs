namespace CannedBytes.Media
{
    /// <summary>
    /// The mode in which the timer operates.
    /// </summary>
    public enum TimerMode
    {
        /// <summary>
        /// The timer is fired only once.
        /// </summary>
        OneShot = 0x0000,

        /// <summary>
        /// The timer is fired periodically.
        /// </summary>
        Periodic = 0x0001
    }
}