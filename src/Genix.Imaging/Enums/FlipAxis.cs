// -----------------------------------------------------------------------
// <copyright file="FlipAxis.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the axis used to flip the image.
    /// </summary>
    public enum FlipAxis
    {
        /// <summary>
        /// No flipping.
        /// </summary>
        None = 0,

        /// <summary>
        /// Flip about x-axis (vertical flip).
        /// </summary>
        X,

        /// <summary>
        /// Flip about y-axis (horizontal flip).
        /// </summary>
        Y,

        /// <summary>
        /// Flip about both axes (180 degrees rotation).
        /// </summary>
        Both,
    }
}
