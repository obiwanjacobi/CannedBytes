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

        public virtual ChunkFileContext Detach()
        {
            Context.Services.RemoveService(GetType());

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
