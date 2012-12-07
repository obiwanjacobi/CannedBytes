using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace CannedBytes.ComponentModel.Composition
{
    public class CompositionContainerFactory : IDisposable
    {
        AggregateCatalog catalog = new AggregateCatalog();

        public void Clear()
        {
            this.catalog.Catalogs.Clear();
        }

        public void AddDefaultTypes()
        {
            AddAllMarkedTypesInAssembly(typeof(CompositionContainerFactory).Assembly);
        }

        public void AddTypes(params Type[] types)
        {
            var cat = new TypeCatalog(types);
            this.catalog.Catalogs.Add(cat);
        }

        public void AddAllMarkedTypesInAssembly(Assembly assembly)
        {
            var cat = new AssemblyCatalog(assembly);

            this.catalog.Catalogs.Add(cat);
        }

        public void AddMarkedTypesInAssembly(Assembly assembly, Type contract)
        {
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

            this.catalog.Catalogs.Add(cat);
        }

        public void AddMarkedTypesInAssembly(Assembly assembly, string contract)
        {
            var result = from type in assembly.GetTypes()
                         from attr in type.GetCustomAttributes(typeof(ExportAttribute), true)
                         where ((ExportAttribute)attr).ContractName == contract
                         select type;

            var cat = new TypeCatalog(result);

            this.catalog.Catalogs.Add(cat);
        }

        public CompositionContainer CreateNew()
        {
            var container = new CompositionContainer(this.catalog);

            // add itself
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            return container;
        }

        public static CompositionContainer CreateDefault()
        {
            using (var builder = new CompositionContainerFactory())
            {
                builder.AddDefaultTypes();

                var container = builder.CreateNew();
                return container;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                this.catalog.Dispose();
            }
        }
    }
}