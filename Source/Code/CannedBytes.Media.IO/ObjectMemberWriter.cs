namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// Manages writing to a runtime chunk object.
    /// </summary>
    public class ObjectMemberWriter
    {
        /// <summary>
        /// Backing field for a list of writable public fields or properties.
        /// </summary>
        private IEnumerable<MemberData> members;

        /// <summary>
        /// Constructs a new instance on the specified runtime object.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public ObjectMemberWriter(object instance)
        {
            Contract.Requires(instance != null);
            Check.IfArgumentNull(instance, "instance");

            this.Instance = instance;
            this.ObjectType = instance.GetType();
            this.members = BuildMemberList(this.ObjectType);
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public void ReadFields(FileChunkReader reader)
        {
            Contract.Requires(reader != null);
            Check.IfArgumentNull(reader, "reader");

            // keep processing native data type members
            foreach (var member in this.members)
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
                    object value = this.ReadValueForType(member.DataType, reader);

                    member.SetValue(this.Instance, value, false);
                }
                catch (EndOfStreamException eos)
                {
                    var fmt = "The end of the chunk was encountered while reading data for the '{1}' member on '{0}'." +
                              " Use the [Ignore] attribute to exclude members from serializing.";

                    var msg = String.Format(
                              CultureInfo.InvariantCulture,
                              fmt,
                              this.Instance.GetType().FullName,
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Check is not recognized")]
        public bool WriteChunkObject(object value)
        {
            Contract.Requires(value != null);
            Check.IfArgumentNull(value, "runtimeObject");

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

            foreach (var member in this.members)
            {
                if (member == null)
                {
                    throw new ChunkFileException("Internal error: null references in member list.");
                }

                if (member.ChunkMatches(chunkId) &&
                    member.CanSetValue)
                {
                    member.SetValue(this.Instance, value, isCollection);

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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Check is not recognized")]
        protected virtual object ReadValueForType(Type type, FileChunkReader reader)
        {
            Contract.Requires(type != null);
            Contract.Requires(reader != null);
            Check.IfArgumentNull(type, "type");
            Check.IfArgumentNull(reader, "reader");

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    return reader.ReadByte();
                case TypeCode.Char:
                    return reader.ReadChar();
                case TypeCode.Decimal:
                    return reader.ReadDecimal();
                case TypeCode.Double:
                    return reader.ReadDouble();
                case TypeCode.Int16:
                    return reader.ReadInt16();
                case TypeCode.Int32:
                    return reader.ReadInt32();
                case TypeCode.Int64:
                    return reader.ReadInt64();
                case TypeCode.Single:
                    return reader.ReadSingle();
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

            var value = this.ReadValueForCustomType(type, reader);

            return value;
        }

        /// <summary>
        /// Uses the <paramref name="reader"/> to read a custom <paramref name="type"/> (class).
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <param name="reader">Must not be null.</param>
        /// <returns>Returns null if the custom type is not supported.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Check is not recognized")]
        protected virtual object ReadValueForCustomType(Type type, FileChunkReader reader)
        {
            Contract.Requires(type != null);
            Contract.Requires(reader != null);
            Check.IfArgumentNull(type, "type");
            Check.IfArgumentNull(reader, "reader");

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

        /// <summary>
        /// Retrieves a list of member info from the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IEnumerable", Justification = "We want to indicate the type.")]
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

                    if (genType.FullName == "System.Collections.Generic.IEnumerable`1")
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
            /// Indicates if there is a macth for this member with the <paramref name="chunkId"/>.
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