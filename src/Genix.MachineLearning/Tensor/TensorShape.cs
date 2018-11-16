// -----------------------------------------------------------------------
// <copyright file="TensorShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    /// <summary>
    /// Defines one of the predefined four-dimensional tensor shapes.
    /// </summary>
    public enum TensorShape
    {
        /// <summary>
        /// Batch, width, height, channels.
        /// </summary>
        BWHC = 0,

        /// <summary>
        /// Batch, height, width, channels.
        /// </summary>
        BHWC = 1,

        /// <summary>
        /// Batch, channels, height, width.
        /// </summary>
        BCHW = 2,

        /// <summary>
        /// Unknown shape.
        /// </summary>
        Unknown = -1,
    }
}
