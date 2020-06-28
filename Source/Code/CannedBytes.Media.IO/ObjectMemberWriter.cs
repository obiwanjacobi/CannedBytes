namespace CannedBytes.Media.IO
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Manages writing to a runtime chunk object.
    /// </summary>
    public class ObjectMemberWriter
    {
        /// <summary>
        /// Backing field for a list of writable public fields or properties.
        /// </summary>
        private readonly ObjectMemberList _members;

        /// <summary>
        /// Constructs a new instance on the specified runtime object.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        public ObjectMemberWriter(object instance)
        {
            Check.IfArgumentNull(instance, nameof(instance));

            Instance = instance;
            ObjectType = instance.GetType();
            _members = new ObjectMemberList(ObjectType);
        }

        /// <summary>
        /// Gets the current runtime object the writer is acting on.
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Gets the type of the runtime object.
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Uses the <paramref name="reader"/> to populate the fields and properties of the runtime object.
        /// </summary>
        /// <param name="reader">Must not be null.</param>
        public void ReadFields(FileChunkReader reader)
        {
            Check.IfArgumentNull(reader, nameof(reader));

            // keep processing native data type members
            foreach (var member in _members)
            {
                if (!reader.CurrentStreamCanRead)
                {
                    break;
                }

                // this member represent a chunk
                if (member.ChunkIds != null)
                {
                    throw new NotSupportedException("This method does not support reading chunks. No mixed (chunks and data) types allowed.");
                }

                try
                {
                    object value = ReadValueForType(member.DataType, reader);

                    member.SetValue(Instance, value, false);
                }
                catch (EndOfStreamException eos)
                {
                    var fmt = "The end of the chunk was encountered while reading data for the '{1}' member on '{0}'." +
                              " Use the [Ignore] attribute to exclude members from serializing.";

                    var msg = String.Format(
                              CultureInfo.InvariantCulture,
                              fmt,
                              Instance.GetType().FullName,
                              member.GetMemberName());

                    throw new ChunkFileException(msg, eos);
                }
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to one of the fields or properties of the runtime object.
        /// </summary>
        /// <param name="value">Must not be null.</param>
        /// <returns>Returns true when the value was written.</returns>
        /// <remarks>Once a property is set it will not be overwritten by subsequent calls to this method.</remarks>
        public bool WriteChunkObject(object value)
        {
            Check.IfArgumentNull(value, nameof(value));

            bool isCollection = false;
            var type = value.GetType();

            if (type.IsGenericType)
            {
                // use generic parameter as type
                type = (from typeArg in type.GetGenericArguments()
                        select typeArg).FirstOrDefault();

                if (type == null)
                {
                    throw new ChunkFileException("Internal error. No type argument from generic type.");
                }

                isCollection = true;
            }

            var chunkId = ChunkAttribute.GetChunkId(type);

            foreach (var member in _members)
            {
                if (member.ChunkMatches(chunkId) &&
                    member.CanSetValue)
                {
                    member.SetValue(Instance, value, isCollection);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Uses the <paramref name="reader"/> to read data for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <param name="reader">Must not be null.</param>
        /// <returns>Returns the value read or null if type is unsupported.</returns>
        protected virtual object ReadValueForType(Type type, FileChunkReader reader)
        {
            Check.IfArgumentNull(type, nameof(type));
            Check.IfArgumentNull(reader, nameof(reader));

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    return reader.ReadByte();
                case TypeCode.Char:
                    return reader.ReadChar();
                case TypeCode.Int16:
                    return reader.ReadInt16();
                case TypeCode.Int32:
                    return reader.ReadInt32();
                case TypeCode.Int64:
                    return reader.ReadInt64();
                case TypeCode.String:
                    return reader.ReadString();
                case TypeCode.UInt16:
                    return reader.ReadUInt16();
                case TypeCode.UInt32:
                    return reader.ReadUInt32();
                case TypeCode.UInt64:
                    return reader.ReadUInt64();
                case TypeCode.Object:
                    // handled as custom object
                    break;
                default:
                    throw new NotSupportedException();
            }

            var value = ReadValueForCustomType(type, reader);

            return value;
        }

        /// <summary>
        /// Uses the <paramref name="reader"/> to read a custom <paramref name="type"/> (class).
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <param name="reader">Must not be null.</param>
        /// <returns>Returns null if the custom type is not supported.</returns>
        protected virtual object ReadValueForCustomType(Type type, FileChunkReader reader)
        {
            Check.IfArgumentNull(type, nameof(type));
            Check.IfArgumentNull(reader, nameof(reader));

            if (type.FullName == typeof(FourCharacterCode).FullName)
            {
                return reader.ReadFourCharacterCode();
            }

            if (type.FullName == typeof(Stream).FullName)
            {
                return reader.GetRemainingCurrentChunkSubStream();
            }

            if (type.FullName == typeof(byte[]).FullName)
            {
                return reader.GetRemainingCurrentChunkBuffer();
            }

            if (type.IsChunk())
            {
                // should be handled outside the member writer
                throw new InvalidOperationException();
            }

            return null;
        }
    }
}