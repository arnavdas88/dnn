// -----------------------------------------------------------------------
// <copyright file="PropertyItem.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Encapsulates an image metadata property that is included in an image file. Not inheritable.
    /// </summary>
    /// <remarks>
    /// An <b>PropertyItem</b> object is intended to be used by classes that uses the <see cref="Image"/> object.
    /// An <b>PropertyItem</b> object is used to retrieve the metadata of existing image files, not to create or change the metadata.
    /// Therefore, the <b>PropertyItem</b> class does not have a defined <b>Public</b> constructor, and you cannot create an instance of an <b>PropertyItem</b> object.
    /// </remarks>
    public sealed class PropertyItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyItem"/> class.
        /// </summary>
        /// <param name="propertyId">The identifier of the property item.</param>
        /// <param name="value">The value of the property item.</param>
        internal PropertyItem(int propertyId, object value)
        {
            this.Id = propertyId;
            this.Value = value;

            // convert string values coming from exif specification to enums
            string s = value as string;
            if (!string.IsNullOrEmpty(s))
            {
                TIFFFieldAttribute tiffFieldAttribute = GetTIFFAttribute<TIFFFieldAttribute>();
                if (tiffFieldAttribute != null)
                {
                    if (int.TryParse(s, out int result))
                    {
                        this.Value = result;
                    }
                }
            }

            T GetTIFFAttribute<T>() where T : Attribute
            {
                if (!Enum.IsDefined(typeof(TIFFField), propertyId))
                {
                    return null;
                }

                FieldInfo fieldInfo = typeof(TIFFField).GetField(typeof(TIFFField).GetEnumName((TIFFField)propertyId));
                return Attribute.GetCustomAttribute(fieldInfo, typeof(T)) as T;
            }
        }

        /// <summary>
        /// Gets the identifier of the property item.
        /// </summary>
        /// <value>
        /// An <see cref="Int32"/> that contains the property item identifier.
        /// </value>
        public int Id { get; }

        /// <summary>
        /// Gets the value of the property item.
        /// </summary>
        /// <value>
        /// An <see cref="Object"/> that contains the property item value.
        /// </value>
        public object Value { get; internal set; }

        /// <summary>
        /// Converts this <see cref="PropertyItem"/> to a human-readable string.
        /// </summary>
        /// <returns>A string that represents this <see cref="Image"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} : {1}", this.Id, this.Value);
        }
    }
}
