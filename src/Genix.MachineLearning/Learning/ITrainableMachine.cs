// -----------------------------------------------------------------------
// <copyright file="ITrainableMachine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the contract for a machine that can be trained.
    /// </summary>
    public interface ITrainableMachine
    {
        /// <summary>
        /// Returns a collection of trainable weights.
        /// </summary>
        /// <returns>The sequence of tuples that contains tensors and regularization rate multipliers.</returns>
        IEnumerable<(Tensor w, float rateL1Multiplier, float rateL2Multiplier)> EnumWeights();

        /// <summary>
        /// Performs one iteration of training algorithm on multiple samples.
        /// </summary>
        /// <typeparam name="TExpected">The type for the expected value.</typeparam>
        /// <param name="samples">The input samples.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <returns>
        /// The calculated loss.
        /// </returns>
        float Learn<TExpected>(IList<(Tensor x, TExpected expected)> samples, ILoss<TExpected> lossFunction);
    }
}
