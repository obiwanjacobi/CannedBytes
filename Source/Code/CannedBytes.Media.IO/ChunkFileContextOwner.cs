namespace CannedBytes.Media.IO
{
    public abstract class ChunkFileContextOwner : DisposableBase
    {
        protected ChunkFileContextOwner(ChunkFileContext context)
        {
            Check.IfArgumentNull(context, nameof(context));
            Context = context;
        }

        public ChunkFileContext Context { get; private set; }

        public ChunkFileContext Detach()
        {
            var context = Context;
            Context = null;
            return context;
        }

        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources &&
                Context != null)
            {
                Detach().Dispose();
            }
        }
    }
}
