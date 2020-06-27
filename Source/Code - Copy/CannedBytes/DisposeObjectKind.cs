namespace CannedBytes
{
    /// <summary>
    /// Identifies what kind of resources should be disposed in a call to Dispose().
    /// </summary>
    public enum DisposeObjectKind
    {
        /// <summary>
        /// From a Finalizer: Only dispose the unmanaged resources.
        /// </summary>
        UnmanagedResourcesOnly,

        /// <summary>
        /// From Dispose(): Dispose both managed and unmanaged resources.
        /// </summary>
        ManagedAndUnmanagedResources
    }
}