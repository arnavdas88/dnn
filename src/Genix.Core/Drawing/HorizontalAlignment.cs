// -----------------------------------------------------------------------
// <copyright file="HorizontalAlignment.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    /// <summary>
    /// Describes how a child element is horizontally positioned within a parent.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// None or unknown alignment.
        /// </summary>
        None = 0,

        /// <summary>
        /// The child element is aligned to the left of the parent.
        /// </summary>
        Left = 1,

        /// <summary>
        /// The child element is aligned to the right of the parent.
        /// </summary>
        Right = 2,

        /// <summary>
        /// The child element is aligned to the center of the parent.
        /// </summary>
        Center = 3,
    }
}
