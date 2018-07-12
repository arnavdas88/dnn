// -----------------------------------------------------------------------
// <copyright file="EXIFFieldAttribute.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <summary>
    /// Represents an attribute that determines the type of enumeration that describes one of <see cref="EXIFField"/> fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class EXIFFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EXIFFieldAttribute"/> class.
        /// </summary>
        /// <param name="fieldType">The type of enumeration that describes the field.</param>
        public EXIFFieldAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets a type of enumeration that describes the field.
        /// </summary>
        public Type FieldType { get; private set; }
    }
}
