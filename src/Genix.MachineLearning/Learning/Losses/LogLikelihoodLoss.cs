// -----------------------------------------------------------------------
// <copyright file="LogLikelihoodLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Negative log-likelihood loss.
    /// </summary>
    public class LogLikelihoodLoss : ILoss<int[]>
    {
        /// <summary>
        /// Gets or sets the label smoothing regularization rate.
        /// </summary>
        /// <value>
        /// Label smoothing regularization rate. Default value is 0.0f.
        /// </value>
        /// <remarks>
        /// See <a href="https://arxiv.org/abs/1512.00567">Rethinking the Inception Architecture for Computer Vision</a>.
        /// </remarks>
        public float LSR { get; set; } = 0.0f;

        /// <inheritdoc />
        public float Loss(Tensor y, int[] expected, bool calculateGradient)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            int mb;
            int mbsize;
            if (y.Rank == 1)
            {
                mb = 1;
                mbsize = y.Length;
            }
            else
            {
                mb = y.Axes[0];
                mbsize = y.Strides[0];
            }

            if (expected.Length != mb)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The number of expected labels: {0} does not match the number of batches: {1}.",
                    expected.Length,
                    mb));
            }

            // loss is the class negative log likelihood
            float[] yw = y.Weights;
            float loss = 0.0f;
            for (int i = 0, yi = 0; i < mb; i++, yi += mbsize)
            {
                loss += -(float)Math.Log(yw[yi + expected[i]]);
            }

            if (calculateGradient)
            {
                float[] dyw = y.Gradient;
                Arrays.Set(y.Length, this.LSR / mbsize, dyw, 0);

                for (int i = 0, yi = 0; i < mb; i++, yi += mbsize)
                {
                    dyw[yi + expected[i]] = 1.0f - this.LSR;
                }
            }

            return loss / mb;
        }
    }
}
