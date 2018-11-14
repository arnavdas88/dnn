// -----------------------------------------------------------------------
// <copyright file="NormalizationType.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Defines the normalization type.
    /// </summary>
    public enum NormalizationType : int
    {
        /// <summary>
        /// Infinity normalization.
        /// </summary>
        Infinity = 0,

        /// <summary>
        /// L1 normalization.
        /// </summary>
        L1,

        /// <summary>
        /// L2 normalization.
        /// </summary>
        L2,
    }
}
