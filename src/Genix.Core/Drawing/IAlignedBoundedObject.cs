// -----------------------------------------------------------------------
// <copyright file="IAlignedBoundedObject.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    /// <summary>
    /// Represents a <see cref="IBoundedObject"/> that is aligned within its parent.
    /// </summary>
    public interface IAlignedBoundedObject : IBoundedObject
    {
        /// <summary>
        /// Gets or sets the object horizontal alignment.
        /// </summary>
        /// <value>
        /// A <see cref="Drawing.HorizontalAlignment"/> enumeration value.
        /// </value>
        HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the object vertical alignment.
        /// </summary>
        /// <value>
        /// A <see cref="Drawing.VerticalAlignment"/> enumeration value.
        /// </value>
        VerticalAlignment VerticalAlignment { get; set; }
    }
}
