// -----------------------------------------------------------------------
// <copyright file="Axis.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    /// <summary>
    /// Defines the axis along one of the tensor dimensions.
    /// </summary>
    public enum Axis
    {
        /// <summary>
        /// The mini-batch axis.
        /// </summary>
        B = 0,

        /// <summary>
        /// The horizontal axis.
        /// </summary>
        X = 1,

        /// <summary>
        /// The vertical axis.
        /// </summary>
        Y = 2,

        /// <summary>
        /// The channel axis.
        /// </summary>
        C = 3,
    }
}
