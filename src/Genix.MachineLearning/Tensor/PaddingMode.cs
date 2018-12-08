// -----------------------------------------------------------------------
// <copyright file="PaddingMode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    /// <summary>
    /// Defines the padding mode.
    /// </summary>
    public enum PaddingMode
    {
        /// <summary>
        /// The size of output is ceiling(inputSize / stride).
        /// </summary>
        Same = 0,

        /// <summary>
        /// No padding. The size of output is ceiling((inputSize - kernelSize + 1) / stride).
        /// </summary>
        Valid = 1,
    }
}
