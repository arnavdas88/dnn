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
        /// Gets the object horizontal alignment.
        /// </summary>
        /// <value>
        /// A <see cref="Genix.Drawing.HorizontalAlignment"/> enumeration value.
        /// </value>
        HorizontalAlignment HorizontalAlignment { get; }

        /// <summary>
        /// Gets the object vertical alignment.
        /// </summary>
        /// <value>
        /// A <see cref="Genix.Drawing.VerticalAlignment"/> enumeration value.
        /// </value>
        VerticalAlignment VerticalAlignment { get; }
    }
}
