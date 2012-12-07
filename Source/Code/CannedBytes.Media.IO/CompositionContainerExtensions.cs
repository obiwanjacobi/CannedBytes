using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

namespace CannedBytes.Media.IO
{
    public static class CompositionContainerExtensions
    {
        public static FileChunkReader CreateFileChunkReader(this CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null, "container");

            var context = container.GetService<ChunkFileContext>();

            if (context == null)
            {
                throw new InvalidOperationException("FileContext export was not found in the CompositionContainer.");
            }

            var reader = new FileChunkReader(context);
            return reader;
        }

        public static T GetService<T>(this CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            return container.GetExportedValue<T>();
        }

        public static void AddInstance<T>(this CompositionContainer container, T instance)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(instance != null, "instance");

            var batch = new CompositionBatch();
            batch.AddExportedValue<T>(instance);
            container.Compose(batch);
        }
    }
}