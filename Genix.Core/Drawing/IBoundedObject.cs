﻿// -----------------------------------------------------------------------
// <copyright file="IBoundedObject.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Drawing;

    /// <summary>
    /// Represents an object that has bounds.
    /// </summary>
    public interface IBoundedObject
    {
        /// <summary>
        /// Gets the bounds of this object.
        /// </summary>
        /// <value>
        /// A <see cref="System.Drawing.Rectangle"/> structure that contains the bounds of this object.
        /// </value>
        Rectangle Bounds { get; }
    }
}
