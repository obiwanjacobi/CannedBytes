using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    public class ObjectMemberWriter
    {
        IEnumerable<MemberData> members;
        IEnumerator<MemberData> memberEnum;

        public ObjectMemberWriter(object instance)
        {
            Contract.Requires(instance != null);

            Instance = instance;
            ObjectType = instance.GetType();
            this.members = BuildMemberList(ObjectType);
            this.memberEnum = members.GetEnumerator();
        }

        public object Instance { get; private set; }

        public Type ObjectType { get; private set; }

        public bool CanReadNext
        {
            get { return (this.members.Count() > 0); }
        }

        public void ReadFields(FileChunkReader reader, bool ignoreChunks)
        {
            // keep processing native data type members
            while (reader.CurrentStreamCanRead &&
                this.memberEnum.MoveNext())
            {
                var member = memberEnum.Current;

                // this member represent a chunk
                if (member.ChunkIds != null)
                {
                    if (!ignoreChunks)
                    {
                        throw new NotSupportedException("This method does not support reading chunks. No mixed (chunks and data) types allowed.");
                    }

                    continue;
                }

                try
                {
                    object value = ReadValueForType(member.DataType, reader);

                    member.SetValue(Instance, value, false);
                }
                catch (EndOfStreamException eos)
                {
                    var msg = String.Format("The end of the chunk was encountered while reading data for the '{1}' member on '{0}'." +
                        " Use the [Ignore] attribute to exclude members from serializing.",
                        Instance.GetType().FullName, member.GetMemberName());

                    throw new ChunkFileException(msg, eos);
                }
            }
        }

        public bool WriteNextChunkObject(object rtObj)
        {
            Contract.Requires<ArgumentNullException>(rtObj != null);

            bool isCollection = false;
            var type = rtObj.GetType();

            if (type.IsGenericType)
            {
                // use generic parameter as type
                type = (from typeArg in type.GetGenericArguments()
                        select typeArg).FirstOrDefault();
                isCollection = true;
            }

            var chunkId = ChunkAttribute.GetChunkId(type);

            // start from the beginning.
            this.memberEnum.Reset();

            while (this.memberEnum.MoveNext())
            {
                if (this.memberEnum.Current.ChunkMatches(chunkId) &&
                    this.memberEnum.Current.CanSetValue)
                {
                    this.memberEnum.Current.SetValue(Instance, rtObj, isCollection);

                    return true;
                }
            }

            return false;
        }

        protected virtual object ReadValueForType(Type type, FileChunkReader reader)
        {
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

            var value = ReadValueForCustomType(type, reader);

            return value;
        }

        protected virtual object ReadValueForCustomType(Type type, FileChunkReader reader)
        {
            Contract.Requires<ArgumentNullException>(type != null);

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

        private static IEnumerable<MemberData> BuildMemberList(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            var members = (from member in type.GetMembers()
                           where member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field
                           where member.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0
                           select new MemberData { FieldInfo = member as FieldInfo, PropertyInfo = member as PropertyInfo }).ToList();

            foreach (var member in members)
            {
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
                            "The generic type '{0}' is not supported. Use IEnumerable<T> for collections.",
                            genType.FullName);

                        throw new NotSupportedException(msg);
                    }
                }
                else
                {
                    member.DataType = dataType;
                }

                chunkId = ChunkAttribute.GetChunkId(member.DataType);

                if (!String.IsNullOrEmpty(chunkId))
                {
                    member.ChunkIds = new List<string>();
                    member.ChunkIds.Add(chunkId);
                }
                else
                {
                    var chunkTypes = ChunkTypeAttribute.GetChunkTypes(member.GetMemberInfo());

                    if (chunkTypes != null && chunkTypes.Length > 0)
                    {
                        member.ChunkIds = new List<string>(chunkTypes);
                    }
                }
            }

            return members;
        }

        private class MemberData
        {
            public FieldInfo FieldInfo;
            public PropertyInfo PropertyInfo;
            public Type DataType;
            public List<string> ChunkIds;
            public bool IsCollection;
            public bool ValueAssigned;

            /// <summary>
            /// Can always write to a collection, but not overwrite a single value.
            /// </summary>
            public bool CanSetValue
            {
                get { return (IsCollection || (!ValueAssigned && !IsCollection)); }
            }

            public bool ChunkMatches(string chunkId)
            {
                if (ChunkIds != null)
                {
                    foreach (var chunkType in ChunkIds)
                    {
                        if (chunkType.MatchesWith(chunkId))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public void SetValue(object instance, object value, bool isCollection)
            {
                if (IsCollection && !isCollection)
                {
                    IList collection = GetValue(instance) as IList;

                    if (collection == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(new[] { DataType });
                        collection = (IList)Activator.CreateInstance(listType);

                        SetMemberValue(instance, collection);
                    }

                    collection.Add(value);
                }
                else
                {
                    SetMemberValue(instance, value);
                }
            }

            private void SetMemberValue(object instance, object value)
            {
                if (FieldInfo != null)
                {
                    FieldInfo.SetValue(instance, value);
                    ValueAssigned = true;
                }

                if (PropertyInfo != null)
                {
                    PropertyInfo.SetValue(instance, value, null);
                    ValueAssigned = true;
                }
            }

            public object GetValue(object instance)
            {
                if (FieldInfo != null)
                {
                    return FieldInfo.GetValue(instance);
                }

                if (PropertyInfo != null)
                {
                    return PropertyInfo.GetValue(instance, null);
                }

                return null;
            }

            public string GetMemberName()
            {
                if (FieldInfo != null)
                {
                    return FieldInfo.Name;
                }

                if (PropertyInfo != null)
                {
                    return PropertyInfo.Name;
                }

                return String.Empty;
            }

            public MemberInfo GetMemberInfo()
            {
                if (FieldInfo != null)
                {
                    return FieldInfo;
                }

                if (PropertyInfo != null)
                {
                    return PropertyInfo;
                }

                return null;
            }
        }
    }
}