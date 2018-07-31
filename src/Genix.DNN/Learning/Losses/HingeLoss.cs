// -----------------------------------------------------------------------
// <copyright file="HingeLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    using System;
    using System.Globalization;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Hinge loss.
    /// </summary>
    public class HingeLoss : ILoss<bool[]>
    {
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="y"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="expected"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The length of <paramref name="y"/> is not the same as the length of <paramref name="expected"/>.
        /// </exception>
        /// <remarks>
        /// The loss function is: l(y) = max(0, 1 - t*y), where t is expected output +/-1, and y is a classifier score.
        /// </remarks>
        public float Loss(Tensor y, bool[] expected, bool calculateGradient)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (expected.Length != y.Length)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The number of expected labels: {0} does not match the tensor length: {1}.",
                    expected.Length,
                    y.Length));
            }

            // loss is: l(y) = max(0, 1 - t * y)
            float[] yw = y.Weights;
            float loss = 0.0f;
            for (int i = 0, ii = y.Length; i < ii; i++)
            {
                loss += Maximum.Max(0, 1.0f - Score(i));
            }

            if (calculateGradient)
            {
                float[] dyw = y.Gradient;
                for (int i = 0, ii = y.Length; i < ii; i++)
                {
                    float score = Score(i);
                    dyw[i] = Score(i) > 1.0f ? 0.0f : yw[i];
                }
            }

            return loss / y.Length;

            float Score(int i)
            {
                return (expected[i] ? 1.0f : -1.0f) * yw[i];
            }
        }
    }
}
