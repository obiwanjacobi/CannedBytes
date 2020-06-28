namespace CannedBytes.Media.IO
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides access to an object's members for reading.
    /// </summary>
    public class ObjectMemberReader
    {
        /// <summary>The list with object members.</summary>
        private readonly ObjectMemberList _members;

        /// <summary>An enumerator for multi-call iteration.</summary>
        private IEnumerator<ObjectMemberData> enumerator;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        public ObjectMemberReader(object instance)
        {
            Check.IfArgumentNull(instance, nameof(instance));

            Instance = instance;
            ObjectType = instance.GetType();
            _members = new ObjectMemberList(ObjectType);
        }

        /// <summary>
        /// Gets the instance that was passed in the constructor.
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Gets a value indicating if the object contains sub-chunks.
        /// </summary>
        public bool IsChunkContainer
        {
            get
            {
                var result = from member in _members
                             where member.ChunkIds != null
                             where member.ChunkIds.Count > 0
                             select member;

                return result.Any();
            }
        }

        /// <summary>
        /// Gets a value indicating if the current member is a collection.
        /// </summary>
        /// <remarks>Only valid after a call to <see cref="GetNextChunkObject"/>.</remarks>
        public bool CurrentMemberIsCollection
        {
            get
            {
                return enumerator != null &&
                       enumerator.Current != null &&
                       enumerator.Current.IsCollection;
            }
        }

        /// <summary>
        /// Gets a value indicating if the current member should be represented by a LIST chunk.
        /// </summary>
        /// <remarks>Only valid after a call to <see cref="GetNextChunkObject"/>.</remarks>
        public bool CurrentMemberIsListChunk
        {
            get
            {
                return enumerator != null &&
                       enumerator.Current != null &&
                       enumerator.Current.IsCollection &&
                       !enumerator.Current.ChunkIdsAreChunkTypes;
            }
        }

        /// <summary>
        /// Iterates through the object's member values.
        /// </summary>
        /// <param name="chunkObject">The value of the 'current' member.</param>
        /// <returns>Returns false if there are no more members.</returns>
        public bool GetNextChunkObject(out object chunkObject)
        {
            if (enumerator == null)
            {
                enumerator = _members.GetEnumerator();
            }

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.ChunkIds != null &&
                    enumerator.Current.ChunkIds.Count > 0)
                {
                    chunkObject = enumerator.Current.GetValue(Instance);
                    return true;
                }
            }

            chunkObject = null;
            return false;
        }

        /// <summary>
        /// Writes all the object's members as data fields to the stream.
        /// </summary>
        /// <param name="writer">Must not be null.</param>
        public void WriteFields(FileChunkWriter writer)
        {
            foreach (var member in _members)
            {
                if (member.ChunkIds != null && member.ChunkIds.Count > 0)
                {
                    throw new NotSupportedException("This method does not support writing chunks. No mixed (chunks and data) types allowed.");
                }

                WritePropertyValue(member, writer);
            }
        }

        /// <summary>
        /// Writes the value of the <paramref name="member"/> to the stream.
        /// </summary>
        /// <param name="member">Must not be null.</param>
        /// <param name="writer">Must not be null.</param>
        private void WritePropertyValue(ObjectMemberData member, FileChunkWriter writer)
        {
            var value = member.GetValue(Instance);
            var type = member.DataType;

            if (value != null)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                        writer.WriteByte((byte)value);
                        break;
                    case TypeCode.Char:
                        writer.WriteChar((char)value);
                        break;
                    case TypeCode.Int16:
                        writer.WriteInt16((short)value);
                        break;
                    case TypeCode.Int32:
                        writer.WriteInt32((int)value);
                        break;
                    case TypeCode.Int64:
                        writer.WriteInt64((long)value);
                        break;
                    case TypeCode.String:
                        writer.WriteString((string)value);
                        break;
                    case TypeCode.UInt16:
                        writer.WriteUInt16((ushort)value);
                        break;
                    case TypeCode.UInt32:
                        writer.WriteUInt32((uint)value);
                        break;
                    case TypeCode.UInt64:
                        writer.WriteUInt64((ulong)value);
                        break;
                    case TypeCode.Object:
                        WriteObjectValue(writer, value, type);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Writes an object value to the stream.
        /// </summary>
        /// <param name="writer">Must not be null.</param>
        /// <param name="value">Must not be null.</param>
        /// <param name="valueType">Must not be null.</param>
        private static void WriteObjectValue(FileChunkWriter writer, object value, Type valueType)
        {
            if (valueType.FullName == typeof(FourCharacterCode).FullName)
            {
                writer.WriteFourCharacterCode((FourCharacterCode)value);
            }

            if (valueType.FullName == typeof(Stream).FullName)
            {
                writer.WriteStream((Stream)value);
            }

            if (valueType.FullName == typeof(byte[]).FullName)
            {
                writer.WriteBuffer((byte[])value);
            }

            if (valueType.IsChunk())
            {
                // should be handled outside the member writer
                throw new InvalidOperationException();
            }
        }
    }
}