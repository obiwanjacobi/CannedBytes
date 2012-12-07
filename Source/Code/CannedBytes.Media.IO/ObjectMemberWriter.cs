using System;
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

        public FourCharacterCode CurrentChunkId
        {
            get
            {
                if (this.memberEnum != null &&
                    this.memberEnum.Current != null)
                {
                    return this.memberEnum.Current.ChunkId;
                }

                return null;
            }
        }

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
                if (member.ChunkId != null && !ignoreChunks)
                {
                    throw new NotSupportedException("This method does not support reading chunks.");
                }

                if (member.FieldInfo != null)
                {
                    WriteField(member.FieldInfo, reader);
                }

                if (member.PropertyInfo != null)
                {
                    WriteProperty(member.PropertyInfo, reader);
                }

                member.ValueAssigned = true;
            }
        }

        public bool WriteNextChunkObject(object rtObj)
        {
            Contract.Requires<ArgumentNullException>(rtObj != null);

            var type = rtObj.GetType();

            if (type.IsGenericType)
            {
                // use generic parameter as type
                type = (from typeArg in type.GetGenericArguments()
                        select typeArg).FirstOrDefault();
            }

            var chunkId = ChunkAttribute.GetChunkId(type);

            // start from the beginning.
            this.memberEnum.Reset();

            while (this.memberEnum.MoveNext())
            {
                if (this.memberEnum.Current.ChunkId != null &&
                    this.memberEnum.Current.ChunkId.ToString() == chunkId &&
                    this.memberEnum.Current.ValueAssigned == false)
                {
                    if (this.memberEnum.Current.FieldInfo != null)
                    {
                        this.memberEnum.Current.FieldInfo.SetValue(Instance, rtObj);
                    }

                    if (this.memberEnum.Current.PropertyInfo != null)
                    {
                        this.memberEnum.Current.PropertyInfo.SetValue(Instance, rtObj, null);
                    }

                    this.memberEnum.Current.ValueAssigned = true;
                    return true;
                }
            }

            return false;
        }

        protected virtual void WriteProperty(PropertyInfo propertyInfo, FileChunkReader reader)
        {
            try
            {
                object value = ReadValueForType(propertyInfo.PropertyType, reader);

                propertyInfo.SetValue(Instance, value, null);
            }
            catch (EndOfStreamException eos)
            {
                var msg = String.Format("The end of the chunk was encountered while reading data for the {1} named '{2}' on '{0}'." +
                    " Use the [Ignore] attribute to exclude members from serializing.",
                    propertyInfo.DeclaringType.FullName, propertyInfo.MemberType.ToString().ToLowerInvariant(), propertyInfo.Name);

                throw new ChunkFileException(msg, eos);
            }
        }

        protected virtual void WriteField(FieldInfo fieldInfo, FileChunkReader reader)
        {
            try
            {
                object value = ReadValueForType(fieldInfo.FieldType, reader);

                fieldInfo.SetValue(Instance, value);
            }
            catch (EndOfStreamException eos)
            {
                var msg = String.Format("The end of the chunk was encountered while reading data for the {1} named '{2}' on '{0}'." +
                    " Use the [Ignore] attribute to exclude members from serializing.",
                    fieldInfo.DeclaringType.FullName, fieldInfo.MemberType.ToString().ToLowerInvariant(), fieldInfo.Name);

                throw new ChunkFileException(msg, eos);
            }
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
                //return reader.ReadChunk();
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
                    }
                    else
                    {
                        var msg = String.Format(
                            "The generic type '{0}' is not supported. Use IEnumerable<T>.",
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
                    member.ChunkId = new FourCharacterCode(chunkId);
                }
            }

            return members;
        }

        private class MemberData
        {
            public FieldInfo FieldInfo;
            public PropertyInfo PropertyInfo;
            public Type DataType;
            public FourCharacterCode ChunkId;
            public bool ValueAssigned;
        }
    }
}