// -----------------------------------------------------------------------
// <copyright file="CrossEntropyLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if false
namespace Accord.DNN.Learning
{
    using System;

    /// <summary>
    /// Cross entropy loss.
    /// </summary>
    public class CrossEntropyLoss : ILoss<Tensor, Tensor, float>
    {
        /// <summary>
        /// Computes the loss between the expected value (ground truth) and the given actual value that have been predicted.
        /// </summary>
        /// <param name="predicted">The value that have been predicted.</param>
        /// <param name="expected">The expected value that should have been predicted.</param>
        /// <returns>The loss value between the expected value and the actual predicted value.</returns>
        public float Loss(Tensor predicted, Tensor expected)
        {
            if (predicted == null)
            {
                throw new ArgumentNullException(nameof(predicted));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            float[] p = expected.Weights;
            float[] q = predicted.Weights;

            float loss = 0.0f;
            for (int i = 0, ii = p.Length; i < ii; i++)
            {
                loss += p[i] * (float)Math.Log(q[i]);
            }

            return loss;
        }
    }
}
#endif