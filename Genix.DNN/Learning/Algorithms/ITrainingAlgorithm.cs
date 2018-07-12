// -----------------------------------------------------------------------
// <copyright file="ITrainingAlgorithm.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    /// <summary>
    /// Specifies an algorithm for training neural nets.
    /// </summary>
    public interface ITrainingAlgorithm
    {
        /// <summary>
        /// Computes gradient deltas for a mini batch.
        /// </summary>
        /// <param name="epoch">The zero-based index of learning epoch.</param>
        /// <param name="gradient">The gradient to compute.</param>
        /// <param name="totalSamples">The total number of sample processed to the moment.</param>
        void ComputeDeltas(int epoch, float[] gradient, int totalSamples);
    }
}
