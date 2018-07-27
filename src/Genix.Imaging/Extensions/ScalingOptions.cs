// -----------------------------------------------------------------------
// <copyright file="ScalingOptions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <summary>
    /// Defines the options for scaling methods.
    /// </summary>
    [Flags]
    public enum ScalingOptions
    {
        /// <summary>
        /// None of the options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Upscale 1bpp images before scaling and then binarize them after scaling.
        /// </summary>
        Upscale1Bpp = 1
    }
}
