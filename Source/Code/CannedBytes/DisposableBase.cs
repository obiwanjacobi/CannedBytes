namespace CannedBytes
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Implements the Dispose implementation pattern as a base class.
    /// </summary>
    /// <remarks>
    /// see also http://obiwanjacobi.blogspot.nl/2006/12/two-layers-of-disposability.html
    /// Derived classes that introduce unmanaged resources must implement a finalizer and
    /// call Dispose(false) in that finalizer. All resources (managed and unmanaged) are
    /// disposed inside the Dispose(bool) method.
    /// </remarks>
    public abstract class DisposableBase : IDisposable
    {
        /// <summary>
        /// Call to dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(DisposeObjectKind.ManagedAndUnmanagedResources);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Called either from <see cref="M:Dispose"/> or the Finalizer (not in this base class)
        /// to dispose of this instance.
        /// </summary>
        /// <param name="disposeKind">Indicates what type of resources to dispose of.</param>
        /// <remarks>Derived classes override to Dispose their members.</remarks>
        /// <example>
        /// <code>
        /// if (!IsDisposed)
        /// {
        ///     if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources)
        ///     {
        ///         // dispose managed resources
        ///     }
        ///     // dispose unmanaged resources
        /// }
        /// </code>
        /// </example>
        protected abstract void Dispose(DisposeObjectKind disposeKind);

        /// <summary>
        /// Gets a value indicating if this instance has been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Throws an exception if the instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        protected virtual void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          Properties.Resources.DisposableBase_ObjectDisposed,
                          GetType().FullName);

                throw new ObjectDisposedException(msg);
            }
        }
    }
}