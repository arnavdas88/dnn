// -----------------------------------------------------------------------
// <copyright file="Flip.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the axis used to flip the image.
    /// </summary>
    public enum Flip
    {
        /// <summary>
        /// No flipping.
        /// </summary>
        None = 0,

        /// <summary>
        /// Horizontal flip.
        /// </summary>
        X,

        /// <summary>
        /// Vertical flip.
        /// </summary>
        Y,
    }
}
