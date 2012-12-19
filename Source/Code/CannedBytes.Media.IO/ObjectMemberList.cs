namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// Manages a collection of <see cref="ObjectMemberData"/> instances.
    /// </summary>
    internal class ObjectMemberList : Collection<ObjectMemberData>
    {
        /// <summary>
        /// Constructs and populates the instance.
        /// </summary>
        /// <param name="type">The type of the object to get its members from.</param>
        public ObjectMemberList(Type type)
        {
            Check.IfArgumentNull(type, "type");

            var list = BuildMemberList(type);

            foreach (var member in list)
            {
                Items.Add(member);
            }
        }

        /// <summary>
        /// Retrieves a list of member info from the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IEnumerable", Justification = "We want to indicate the type.")]
        private static IEnumerable<ObjectMemberData> BuildMemberList(Type type)
        {
            Contract.Requires(type != null);
            Check.IfArgumentNull(type, "type");

            var members = (from member in type.GetMembers()
                           where member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field
                           where member.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0
                           select new ObjectMemberData { FieldInfo = member as FieldInfo, PropertyInfo = member as PropertyInfo }).ToList();

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

                    if (genType.FullName.StartsWith("System.Collections.Generic.", StringComparison.OrdinalIgnoreCase) &&
                        genType.FullName.EndsWith("`1", StringComparison.OrdinalIgnoreCase))
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
    }
}