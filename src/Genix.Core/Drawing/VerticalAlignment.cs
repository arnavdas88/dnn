// -----------------------------------------------------------------------
// <copyright file="VerticalAlignment.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    /// <summary>
    /// Describes how a child element is vertically positioned within a parent.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// None or unknown alignment.
        /// </summary>
        None = 0,

        /// <summary>
        /// The child element is aligned to the top of the parent.
        /// </summary>
        Top = 1,

        /// <summary>
        /// The child element is aligned to the bottom of the parent.
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// The child element is aligned to the center of the parent.
        /// </summary>
        Center = 3,
    }
}
