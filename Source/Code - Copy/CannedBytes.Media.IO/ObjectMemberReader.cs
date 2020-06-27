namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// Provides access to an object's members for reading.
    /// </summary>
    public class ObjectMemberReader
    {
        /// <summary>The list with object members.</summary>
        private ObjectMemberList members;

        /// <summary>An enumerator for multi-call iteration.</summary>
        private IEnumerator<ObjectMemberData> enumerator;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public ObjectMemberReader(object instance)
        {
            Check.IfArgumentNull(instance, "instance");

            this.Instance = instance;
            this.ObjectType = instance.GetType();
            this.members = new ObjectMemberList(this.ObjectType);
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
                var result = from member in this.members
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
                return this.enumerator != null &&
                       this.enumerator.Current != null &&
                       this.enumerator.Current.IsCollection;
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
                return this.enumerator != null &&
                       this.enumerator.Current != null &&
                       this.enumerator.Current.IsCollection &&
                       !this.enumerator.Current.ChunkIdsAreChunkTypes;
            }
        }

        /// <summary>
        /// Iterates through the object's member values.
        /// </summary>
        /// <param name="chunkObject">The value of the 'current' member.</param>
        /// <returns>Returns false if there are no more members.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "We need both the object and the bool.")]
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Not possible to use generics.")]
        public bool GetNextChunkObject(out object chunkObject)
        {
            if (this.enumerator == null)
            {
                this.enumerator = this.members.GetEnumerator();
            }

            while (this.enumerator.MoveNext())
            {
                if (this.enumerator.Current.ChunkIds != null &&
                    this.enumerator.Current.ChunkIds.Count > 0)
                {
                    chunkObject = this.enumerator.Current.GetValue(this.Instance);
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
            foreach (var member in this.members)
            {
                if (member.ChunkIds != null && member.ChunkIds.Count > 0)
                {
                    throw new NotSupportedException("This method does not support writing chunks. No mixed (chunks and data) types allowed.");
                }

                this.WritePropertyValue(member, writer);
            }
        }

        /// <summary>
        /// Writes the value of the <paramref name="member"/> to the stream.
        /// </summary>
        /// <param name="member">Must not be null.</param>
        /// <param name="writer">Must not be null.</param>
        private void WritePropertyValue(ObjectMemberData member, FileChunkWriter writer)
        {
            var value = member.GetValue(this.Instance);
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