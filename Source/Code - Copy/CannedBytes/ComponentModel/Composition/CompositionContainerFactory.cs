namespace CannedBytes.ComponentModel.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.CodeAnalysis;
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
            Check.IfArgumentNull(types, "types");
            ThrowIfDisposed();

            var cat = new TypeCatalog(types);

            try
            {
                Contract.Assume(this.catalog.Catalogs != null);
                this.catalog.Catalogs.Add(cat);
            }
            catch
            {
                cat.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds all exported types in the <paramref name="assembly"/>
        /// to the catalog.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        public void AddAllMarkedTypesInAssembly(Assembly assembly)
        {
            Contract.Requires(assembly != null);
            Check.IfArgumentNull(assembly, "assembly");
            ThrowIfDisposed();

            var cat = new AssemblyCatalog(assembly);

            try
            {
                Contract.Assume(this.catalog.Catalogs != null);
                this.catalog.Catalogs.Add(cat);
            }
            catch
            {
                cat.Dispose();
                throw;
            }
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
            Check.IfArgumentNull(contract, "contract");
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

            try
            {
                Contract.Assume(this.catalog.Catalogs != null);
                this.catalog.Catalogs.Add(cat);
            }
            catch
            {
                cat.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds all exported type in the <paramref name="assembly"/>
        /// of a specific <paramref name="contract"/> to the catalog.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        /// <param name="contract">The export contract. Must not be null or empty.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public void AddMarkedTypesInAssembly(Assembly assembly, string contract)
        {
            Contract.Requires(assembly != null);
            Contract.Requires(!String.IsNullOrEmpty(contract));
            Check.IfArgumentNull(assembly, "assembly");
            Check.IfArgumentNullOrEmpty(contract, "contract");
            ThrowIfDisposed();

            var result = from type in assembly.GetTypes()
                         from attr in type.GetCustomAttributes(typeof(ExportAttribute), true)
                         where ((ExportAttribute)attr).ContractName == contract
                         select type;

            var cat = new TypeCatalog(result);

            try
            {
                Contract.Assume(this.catalog.Catalogs != null);
                this.catalog.Catalogs.Add(cat);
            }
            catch
            {
                cat.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new composition container from the current catalog state.
        /// </summary>
        /// <returns>Never returns null.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Implemented the suggested pattern.")]
        public CompositionContainer CreateNew()
        {
            ThrowIfDisposed();

            CompositionContainer compositionContainer = null;
            CompositionContainer container = null;

            try
            {
                container = new CompositionContainer(this.catalog);
                // add itself
                var batch = new CompositionBatch();
                batch.AddExportedValue(container);
                container.Compose(batch);

                compositionContainer = container;
                container = null;
            }
            finally
            {
                if (container != null)
                {
                    container.Dispose();
                }
            }

            return compositionContainer;
        }

        /// <summary>
        /// Called to dispose the instance.
        /// </summary>
        /// <param name="disposeKind">The type of resources to dispose of.</param>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources)
            {
                this.catalog.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// The object's invariant contract.
        /// </summary>
        [ContractInvariantMethod]
        private void InvariantContract()
        {
            Contract.Invariant(this.catalog != null);
        }
    }
}