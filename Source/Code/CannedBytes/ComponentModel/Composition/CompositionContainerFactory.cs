namespace CannedBytes.ComponentModel.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides means to populate a MEF catalog and from that create a composition container.
    /// </summary>
    public class CompositionContainerFactory : DisposableBase
    {
        /// <summary>
        /// The composition catalog. Never null.
        /// </summary>
        private AggregateCatalog catalog = new AggregateCatalog();

        /// <summary>
        /// Removes all catalog definitions.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            Contract.Assume(this.catalog.Catalogs != null);
            this.catalog.Catalogs.Clear();
        }

        /// <summary>
        /// Adds these <paramref name="types"/> to the catalog.
        /// </summary>
        /// <param name="types">Must not be null.</param>
        public void AddTypes(params Type[] types)
        {
            Contract.Requires(types != null);
            Throw.IfArgumentNull(types, "types");
            ThrowIfDisposed();

            var cat = new TypeCatalog(types);

            Contract.Assume(this.catalog.Catalogs != null);
            this.catalog.Catalogs.Add(cat);
        }

        /// <summary>
        /// Adds all exported types in the <paramref name="assembly"/>
        /// to the catalog.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        public void AddAllMarkedTypesInAssembly(Assembly assembly)
        {
            Contract.Requires(assembly != null);
            Throw.IfArgumentNull(assembly, "assembly");
            ThrowIfDisposed();

            var cat = new AssemblyCatalog(assembly);

            Contract.Assume(this.catalog.Catalogs != null);
            this.catalog.Catalogs.Add(cat);
        }

        /// <summary>
        /// Adds all exported type in the <paramref name="assembly"/>
        /// of a specific <paramref name="contract"/> to the catalog.
        /// </summary>
        /// <param name="assembly">May be null. Assembly is then taken from the <paramref name="contract"/>.</param>
        /// <param name="contract">The export contract type. Must not be null.</param>
        public void AddMarkedTypesInAssembly(Assembly assembly, Type contract)
        {
            Contract.Requires(contract != null);
            Throw.IfArgumentNull(contract, "contract");
            ThrowIfDisposed();

            if (assembly == null)
            {
                assembly = contract.Assembly;
            }

            var result = from type in assembly.GetTypes()
                         from attr in type.GetCustomAttributes(typeof(ExportAttribute), true)
                         where attr != null
                         where ((ExportAttribute)attr).ContractType != null
                         where ((ExportAttribute)attr).ContractType.FullName == contract.FullName
                         select type;

            var cat = new TypeCatalog(result);

            Contract.Assume(this.catalog.Catalogs != null);
            this.catalog.Catalogs.Add(cat);
        }

        /// <summary>
        /// Adds all exported type in the <paramref name="assembly"/>
        /// of a specific <paramref name="contract"/> to the catalog.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        /// <param name="contract">The export contract. Must not be null or empty.</param>
        public void AddMarkedTypesInAssembly(Assembly assembly, string contract)
        {
            Contract.Requires(assembly != null);
            Contract.Requires(!String.IsNullOrEmpty(contract));
            Throw.IfArgumentNull(assembly, "assembly");
            Throw.IfArgumentNullOrEmpty(contract, "contract");
            ThrowIfDisposed();

            var result = from type in assembly.GetTypes()
                         from attr in type.GetCustomAttributes(typeof(ExportAttribute), true)
                         where ((ExportAttribute)attr).ContractName == contract
                         select type;

            var cat = new TypeCatalog(result);

            Contract.Assume(this.catalog.Catalogs != null);
            this.catalog.Catalogs.Add(cat);
        }

        /// <summary>
        /// Creates a new composition container from the current catalog state.
        /// </summary>
        /// <returns>Never returns null.</returns>
        public CompositionContainer CreateNew()
        {
            ThrowIfDisposed();

            var container = new CompositionContainer(this.catalog);

            // add itself
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            return container;
        }

        /// <summary>
        /// Called to dispose the instance.
        /// </summary>
        /// <param name="disposeManagedResources">True when also managed resources are disposed.</param>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                this.catalog.Dispose();
            }

            base.Dispose();
        }

        [ContractInvariantMethod]
        private void InvariantContract()
        {
            Contract.Invariant(this.catalog != null);
        }
    }
}