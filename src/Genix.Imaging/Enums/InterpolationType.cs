// -----------------------------------------------------------------------
// <copyright file="InterpolationType.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <summary>
    /// Defines the interpolation type.
    /// </summary>
    [Flags]
    public enum InterpolationType
    {
        /// <summary>
        /// The nearest neighbor interpolation method.
        /// </summary>
        NearestNeighbor = 0,

        /// <summary>
        /// The linear interpolation method.
        /// </summary>
        Linear = 1,

        /// <summary>
        /// The interpolation with the two-parameter cubic filter.
        /// </summary>
        Cubic = 2,

        /// <summary>
        /// The interpolation with the Lanczos filter.
        /// </summary>
        Lanczos = 3,

        /// <summary>
        /// The super sampling interpolation method.
        /// </summary>
        Super = 4,
    }
}
