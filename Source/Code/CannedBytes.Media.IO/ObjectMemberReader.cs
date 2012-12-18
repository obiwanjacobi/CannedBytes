namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CannedBytes.Media.IO.SchemaAttributes;

    public class ObjectMemberReader
    {
        private IEnumerable<MemberData> members;
        private IEnumerator<MemberData> enumerator;

        public ObjectMemberReader(object instance)
        {
            Check.IfArgumentNull(instance, "instance");

            this.Instance = instance;
            this.ObjectType = instance.GetType();
            this.members = BuildMemberList(ObjectType);
        }

        public object Instance { get; private set; }

        public Type ObjectType { get; private set; }

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

        public bool CurrentMemberIsCollection
        {
            get
            {
                return this.enumerator != null &&
                       this.enumerator.Current != null &&
                       this.enumerator.Current.IsCollection;
            }
        }

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
                    chunkObject = this.enumerator.Current.GetValue(Instance);
                    return true;
                }
            }

            chunkObject = null;
            return false;
        }

        public void WriteFields(FileChunkWriter writer)
        {
            foreach (var member in this.members)
            {
                if (member.ChunkIds != null && member.ChunkIds.Count > 0)
                {
                    throw new NotSupportedException("This method does not support writing chunks. No mixed (chunks and data) types allowed.");
                }

                WritePropertyValue(member, writer);
            }
        }

        private void WritePropertyValue(MemberData member, FileChunkWriter writer)
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
                    //case TypeCode.Decimal:
                    //case TypeCode.Double:
                    case TypeCode.Int16:
                        writer.WriteInt16((short)value);
                        break;
                    case TypeCode.Int32:
                        writer.WriteInt32((int)value);
                        break;
                    case TypeCode.Int64:
                        writer.WriteInt64((long)value);
                        break;
                    //case TypeCode.Single:
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

        private void WriteObjectValue(FileChunkWriter writer, object value, Type type)
        {
            if (type.FullName == typeof(FourCharacterCode).FullName)
            {
                writer.WriteFourCharacterCode((FourCharacterCode)value);
            }

            if (type.FullName == typeof(Stream).FullName)
            {
                writer.WriteStream((Stream)value);
            }

            if (type.FullName == typeof(byte[]).FullName)
            {
                writer.WriteBuffer((byte[])value);
            }

            if (type.IsChunk())
            {
                // should be handled outside the member writer
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Retrieves a list of member info from the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        private static IEnumerable<MemberData> BuildMemberList(Type type)
        {
            Contract.Requires(type != null);
            Check.IfArgumentNull(type, "type");

            var members = (from member in type.GetMembers()
                           where member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field
                           where member.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0
                           select new MemberData { FieldInfo = member as FieldInfo, PropertyInfo = member as PropertyInfo }).ToList();

            foreach (var member in members)
            {
                Contract.Assume(member != null);

                string chunkId = null;
                Type dataType = null;

                if (member.FieldInfo != null)
                {
                    dataType = member.FieldInfo.FieldType;
                }

                if (member.PropertyInfo != null)
                {
                    dataType = member.PropertyInfo.PropertyType;
                }

                Contract.Assume(dataType != null);

                if (dataType.IsGenericType)
                {
                    var genType = dataType.GetGenericTypeDefinition();

                    if (genType.FullName.StartsWith("System.Collections.Generic.") &&
                        genType.FullName.EndsWith("`1"))
                    {
                        member.DataType = (from typeArg in dataType.GetGenericArguments()
                                           select typeArg).FirstOrDefault();
                        member.IsCollection = true;
                    }
                    else
                    {
                        var msg = String.Format(
                                  CultureInfo.InvariantCulture,
                                  "The generic type '{0}' is not supported. Use IEnumerable<T> for collections.",
                                  genType.FullName);

                        throw new NotSupportedException(msg);
                    }
                }
                else
                {
                    member.DataType = dataType;
                }

                if (member.DataType != null)
                {
                    chunkId = ChunkAttribute.GetChunkId(member.DataType);
                }

                if (!String.IsNullOrEmpty(chunkId))
                {
                    member.ChunkIds = new List<string>();
                    member.ChunkIds.Add(chunkId);
                }
                else
                {
                    var memberInfo = member.GetMemberInfo();

                    if (memberInfo != null)
                    {
                        var chunkTypes = ChunkTypeAttribute.GetChunkTypes(memberInfo);

                        if (chunkTypes != null && chunkTypes.Length > 0)
                        {
                            member.ChunkIds = new List<string>(chunkTypes);
                        }

                        member.ChunkIdsAreChunkTypes = true;
                    }
                }
            }

            return members;
        }

        /// <summary>
        /// Maintains information about a member of the runtime object.
        /// </summary>
        private class MemberData
        {
            //// Ignored. Its a private class.
            //// SA1401: Fields must be declared with private access. Use properties to expose fields.

            /// <summary>
            /// Is set when the member is a public field.
            /// </summary>
            public FieldInfo FieldInfo;

            /// <summary>
            /// Is set when the member is public property.
            /// </summary>
            public PropertyInfo PropertyInfo;

            /// <summary>
            /// The data type of the member.
            /// </summary>
            public Type DataType;

            /// <summary>
            /// Any chunk id's that apply to the member.
            /// </summary>
            public List<string> ChunkIds;

            /// <summary>
            /// If true the contents of <see cref="ChunkIds"/> came from <see cref="ChunkTypeAttribute"/>s.
            /// </summary>
            public bool ChunkIdsAreChunkTypes;

            /// <summary>
            /// Indicates if the member is a collection (or list).
            /// </summary>
            public bool IsCollection;

            /// <summary>
            /// Is set when the writer has set a value for the member.
            /// </summary>
            public bool ValueAssigned;

            /// <summary>
            /// Can always write to a collection, but not overwrite a single value.
            /// </summary>
            public bool CanSetValue
            {
                get { return this.IsCollection || (!this.ValueAssigned && !this.IsCollection); }
            }

            /// <summary>
            /// Indicates if there is a match for this member with the <paramref name="chunkId"/>.
            /// </summary>
            /// <param name="chunkId">Can be null or empty.</param>
            /// <returns>Returns true if there is a match.</returns>
            public bool ChunkMatches(string chunkId)
            {
                if (this.ChunkIds != null)
                {
                    foreach (var chunkType in this.ChunkIds)
                    {
                        if (chunkType.MatchesWith(chunkId))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Sets a value for the member. Handler adding to a collection.
            /// </summary>
            /// <param name="instance">The runtime object. Must not be null.</param>
            /// <param name="value">The value to set.</param>
            /// <param name="isCollection">An indication if the <paramref name="value"/> is a collection.</param>
            public void SetValue(object instance, object value, bool isCollection)
            {
                Contract.Requires(instance != null);
                Check.IfArgumentNull(instance, "instance");

                if (this.IsCollection && !isCollection)
                {
                    IList collection = this.GetValue(instance) as IList;

                    if (collection == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(new[] { this.DataType });
                        collection = (IList)Activator.CreateInstance(listType);

                        this.SetMemberValue(instance, collection);
                    }

                    collection.Add(value);
                }
                else
                {
                    this.SetMemberValue(instance, value);
                }
            }

            /// <summary>
            /// Sets the <paramref name="value"/> to either the field or the property.
            /// </summary>
            /// <param name="instance">Must not be null.</param>
            /// <param name="value">The value to set.</param>
            /// <remarks>Set <see cref="F:ValueAssigned"/> to true.</remarks>
            private void SetMemberValue(object instance, object value)
            {
                Contract.Requires(instance != null);
                Check.IfArgumentNull(instance, "instance");

                if (this.FieldInfo != null)
                {
                    this.FieldInfo.SetValue(instance, value);
                    this.ValueAssigned = true;
                }

                if (this.PropertyInfo != null)
                {
                    this.PropertyInfo.SetValue(instance, value, null);
                    this.ValueAssigned = true;
                }
            }

            /// <summary>
            /// Gets the value for the property or field.
            /// </summary>
            /// <param name="instance">Must not be null.</param>
            /// <returns>Can return null.</returns>
            public object GetValue(object instance)
            {
                Contract.Requires(instance != null);
                Check.IfArgumentNull(instance, "instance");

                if (this.FieldInfo != null)
                {
                    return this.FieldInfo.GetValue(instance);
                }

                if (this.PropertyInfo != null)
                {
                    return this.PropertyInfo.GetValue(instance, null);
                }

                return null;
            }

            /// <summary>
            /// Retrieves the name of the member.
            /// </summary>
            /// <returns>Never returns null.</returns>
            public string GetMemberName()
            {
                Contract.Ensures(Contract.Result<string>() != null);

                if (this.FieldInfo != null)
                {
                    return this.FieldInfo.Name;
                }

                if (this.PropertyInfo != null)
                {
                    return this.PropertyInfo.Name;
                }

                return String.Empty;
            }

            /// <summary>
            /// Gets the <see cref="MemberInfo"/> of this member.
            /// </summary>
            /// <returns>Can return null.</returns>
            public MemberInfo GetMemberInfo()
            {
                if (this.FieldInfo != null)
                {
                    return this.FieldInfo;
                }

                if (this.PropertyInfo != null)
                {
                    return this.PropertyInfo;
                }

                return null;
            }
        }
    }
}