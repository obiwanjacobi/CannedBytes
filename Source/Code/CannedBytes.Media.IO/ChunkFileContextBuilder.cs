using CannedBytes.Media.IO.SchemaAttributes;
using CannedBytes.Media.IO.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

namespace CannedBytes.Media.IO
{
    public sealed class ChunkFileContextBuilder
    {
        private readonly AddMode _addMode = AddMode.OverwriteExisting;
        private Endianness _endianness;
        private ChunkFileInfo _chunkFileInfo;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly List<Type> _chunkTypes = new List<Type>();
        private readonly List<Type> _handlerTypes = new List<Type>();

        public ChunkFileContextBuilder()
        {
            // add built-in types
            Discover(GetType().Assembly);
        }

        public ChunkFileContextBuilder LittleEndian()
        {
            return Endianness(IO.Endianness.LittleEndian);
        }

        public ChunkFileContextBuilder BigEndian()
        {
            return Endianness(IO.Endianness.BigEndian);
        }

        public ChunkFileContextBuilder Endianness(Endianness endianness)
        {
            _endianness = endianness;
            return this;
        }

        public ChunkFileContextBuilder ForReading(string filePath)
        {
            if (_chunkFileInfo != null)
            {
                throw new InvalidOperationException();
            }

            _chunkFileInfo = ChunkFileInfo.OpenRead(filePath);
            return this;
        }

        public ChunkFileContextBuilder ForWriting(string filePath)
        {
            if (_chunkFileInfo != null)
            {
                throw new InvalidOperationException();
            }

            _chunkFileInfo = ChunkFileInfo.OpenWrite(filePath);
            return this;
        }

        public ChunkFileContextBuilder WithService<ClassT>()
            where ClassT : class, new()
        {
            _services.Add(typeof(ClassT), null);
            return this;
        }

        public ChunkFileContextBuilder WithService<InterfaceT>(InterfaceT instance)
            where InterfaceT : class
        {
            Check.IfArgumentNull(instance, nameof(instance));
            _services.Add(typeof(InterfaceT), instance);
            return this;
        }

        public ChunkFileContextBuilder DiscoverChunks(Assembly assembly)
        {
            var types = DiscoverTypes(assembly, typeof(ChunkAttribute));
            _chunkTypes.AddRange(types);
            return this;
        }

        public ChunkFileContextBuilder DiscoverChunkHandlers(Assembly assembly)
        {
            var types = DiscoverTypes(assembly, typeof(FileChunkHandlerAttribute));
            _handlerTypes.AddRange(types);
            return this;
        }

        public ChunkFileContextBuilder Discover(Assembly assembly)
        {
            return DiscoverChunks(assembly)
                .DiscoverChunkHandlers(assembly);
        }

        private static IEnumerable<Type> DiscoverTypes(Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass &&
                       t.GetCustomAttributes(attributeType, inherit: true).Any());
        }

        public ChunkFileContextBuilder AddChunk<ChunkT>()
        {
            return AddChunk(typeof(ChunkT));
        }

        public ChunkFileContextBuilder AddChunk(Type chunkType)
        {
            var chunkId = ChunkAttribute.GetChunkId(chunkType);
            if (String.IsNullOrEmpty(chunkId))
            {
                throw new ArgumentException($"The specified type {chunkType.Name} has no ChunkAttribute.", nameof(chunkType));
            }
            _chunkTypes.Add(chunkType);
            return this;
        }

        public ChunkFileContextBuilder AddChunkHandler<HandlerT>()
        {
            return AddChunkHandler(typeof(HandlerT));
        }

        public ChunkFileContextBuilder AddChunkHandler(Type handlerType)
        {
            var chunkId = FileChunkHandlerAttribute.GetChunkId(handlerType);
            if (String.IsNullOrEmpty(chunkId))
            {
                throw new ArgumentException($"The specified type {handlerType.Name} has no FileChunkHandlerAttribute.", nameof(handlerType));
            }
            _handlerTypes.Add(handlerType);
            return this;
        }

        public ChunkFileContext Build()
        {
            var services = CreateServices();
            CreateChunkTypeFactory(services);
            CreateChunkHandlerManager(services);

            return new ChunkFileContext
            {
                ChunkFile = _chunkFileInfo,
                Services = services,
            };
        }

        private void CreateChunkTypeFactory(ServiceContainer services)
        {
            var chunkFactory = new ChunkTypeFactory();

            foreach (var chunkType in _chunkTypes)
            {
                chunkFactory.AddChunkType(chunkType, _addMode);
            }

            services.AddService(typeof(IChunkTypeFactory), chunkFactory);
        }

        private void CreateChunkHandlerManager(ServiceContainer services)
        {
            var handlers = _handlerTypes
                .Select(t => CreateInstance(t, null))
                .Cast<IFileChunkHandler>();

            var handlerMgr = new FileChunkHandlerManager(handlers);
            services.AddService(typeof(FileChunkHandlerManager), handlerMgr);
        }

        private ServiceContainer CreateServices()
        {
            var services = new ServiceContainer();
            AddDefaultServices(services);

            foreach (var svcType in _services)
            {
                services.AddService(svcType.Key, CreateInstance(svcType.Key, svcType.Value));
            }

            return services;
        }

        private void AddDefaultServices(ServiceContainer services)
        {
            if (_endianness == IO.Endianness.LittleEndian)
            {
                services.AddService(typeof(INumberReader), new LittleEndianNumberReader());
                services.AddService(typeof(INumberWriter), new LittleEndianNumberWriter());
            }
            else
            {
                services.AddService(typeof(INumberReader), new BigEndianNumberReader());
                services.AddService(typeof(INumberWriter), new BigEndianNumberWriter());
            }

            services.AddService(typeof(IStreamNavigator), new StreamNavigator());
            services.AddService(typeof(IStringReader), new SizePrefixedStringReader());
            services.AddService(typeof(IStringWriter), new SizePrefixedStringWriter());
        }

        private static object CreateInstance(Type type, object instance)
        {
            if (instance == null)
            {
                instance = Activator.CreateInstance(type);
            }
            return instance;
        }
    }
}
