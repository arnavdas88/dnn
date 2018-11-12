// -----------------------------------------------------------------------
// <copyright file="Orientation.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    /// <summary>
    /// Describes how a child element is oriented within a parent.
    /// </summary>
    public enum Orientation
    {
        /// <summary>
        /// None or unknown orientation.
        /// </summary>
        None = 0,

        /// <summary>
        /// The child element is horizontally oriented.
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// The child element is vertically oriented.
        /// </summary>
        Vertical = 2,
    }
}
