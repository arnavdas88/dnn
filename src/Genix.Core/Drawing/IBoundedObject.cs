// -----------------------------------------------------------------------
// <copyright file="IBoundedObject.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    /// <summary>
    /// Represents an object that has bounds.
    /// </summary>
    public interface IBoundedObject
    {
        /// <summary>
        /// Gets the object's bounding box.
        /// </summary>
        /// <value>
        /// A <see cref="Rectangle"/> structure that contains the object's bounding box.
        /// </value>
        Rectangle Bounds { get; }
    }
}
