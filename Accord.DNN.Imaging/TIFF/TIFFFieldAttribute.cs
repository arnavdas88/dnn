// -----------------------------------------------------------------------
// <copyright file="TIFFFieldAttribute.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;

    /// <summary>
    /// Represents an attribute that determines the type of enumeration that describes one of <see cref="TIFFField"/> fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class TIFFFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TIFFFieldAttribute"/> class.
        /// </summary>
        /// <param name="fieldType">The type of enumeration that describes the field.</param>
        public TIFFFieldAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets a type of enumeration that describes the field.
        /// </summary>
        public Type FieldType { get; }
    }
}
