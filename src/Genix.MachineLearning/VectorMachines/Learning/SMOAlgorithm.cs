// -----------------------------------------------------------------------
// <copyright file="SMOAlgorithm.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines.Learning
{
    /// <summary>
    /// Defines the optimization algorithms used by Sequential Minimum Optimization (SMO).
    /// </summary>
    public enum SMOAlgorithm
    {
        /// <summary>
        /// The selection algorithm implemented in LibSVM library.
        /// </summary>
        LibSVM = 0,
    }
}
