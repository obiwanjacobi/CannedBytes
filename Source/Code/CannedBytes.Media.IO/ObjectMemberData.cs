namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Maintains information about a member of the runtime object.
    /// </summary>
    internal class ObjectMemberData
    {
        /// <summary>
        /// Is set when the member is a public field.
        /// </summary>
        public FieldInfo FieldInfo { get; set; }

        /// <summary>
        /// Is set when the member is public property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// The data type of the member.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Any chunk id's that apply to the member.
        /// </summary>
        public List<string> ChunkIds { get; set; }

        /// <summary>
        /// If true the contents of <see cref="ChunkIds"/> came from <see cref="T:ChunkTypeAttribute"/>s.
        /// </summary>
        public bool ChunkIdsAreChunkTypes { get; set; }

        /// <summary>
        /// Indicates if the member is a collection (or list).
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Is set when the writer has set a value for the member.
        /// </summary>
        public bool ValueAssigned { get; set; }

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