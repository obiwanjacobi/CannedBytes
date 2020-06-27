namespace CannedBytes
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Implements the Dispose implementation pattern for classes that own unmanaged resources.
    /// </summary>
    /// <remarks>
    /// see also http://obiwanjacobi.blogspot.nl/2006/12/two-layers-of-disposability.html
    /// </remarks>
    public abstract class UnmanagedDisposableBase : DisposableBase
    {
        /// <summary>
        /// Destructor called by the GC.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "We don't have Dispose(bool).")]
        ~UnmanagedDisposableBase()
        {
            Dispose(DisposeObjectKind.UnmanagedResourcesOnly);
        }
    }
}