using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Extension method helpers for the <see cref="CompositionContainer"/>.
    /// </summary>
    public static class CompositionContainerExtensions
    {
        /// <summary>
        /// Creates a new <see cref="FileChunkReader"/> instance.
        /// </summary>
        /// <param name="container">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        public static FileChunkReader CreateFileChunkReader(this CompositionContainer container)
        {
            Contract.Requires(container != null);
            Contract.Ensures(Contract.Result<FileChunkReader>() != null);

            var context = container.GetService<ChunkFileContext>();

            if (context == null)
            {
                throw new InvalidOperationException("FileContext export was not found in the CompositionContainer.");
            }

            if (context.CompositionContainer == null)
            {
                context.CompositionContainer = container;
            }

            var reader = new FileChunkReader(context);
            return reader;
        }

        /// <summary>
        /// Returns an instance for the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The service type, usually an interface.</typeparam>
        /// <param name="container">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        /// <exception cref="ChunkFileException">Thrown when no instance was found.</exception>
        public static T GetService<T>(this CompositionContainer container) where T : class
        {
            Contract.Requires(container != null);
            Contract.Ensures(Contract.Result<T>() != null);

            var result = container.GetExportedValue<T>();

            if (result == null)
            {
                var msg = String.Format("The type '{0}' was not found in the container.", typeof(T).FullName);
                throw new ChunkFileException(msg);
            }

            return result;
        }

        /// <summary>
        /// Helper to add an instance to the container.
        /// </summary>
        /// <typeparam name="T">The type under which the <paramref name="instance"/> is registered.</typeparam>
        /// <param name="container">Must not be null.</param>
        /// <param name="instance">Must not be null.</param>
        public static void AddInstance<T>(this CompositionContainer container, T instance) where T : class
        {
            Contract.Requires(container != null);
            Contract.Requires(instance != null);
            Throw.IfArgumentNull(container, "container");
            Throw.IfArgumentNull(instance, "instance");

            var batch = new CompositionBatch();
            batch.AddExportedValue<T>(instance);
            container.Compose(batch);
        }
    }
}