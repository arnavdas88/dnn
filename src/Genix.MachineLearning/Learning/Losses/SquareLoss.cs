// -----------------------------------------------------------------------
// <copyright file="SquareLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Globalization;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Square loss, also known as L2-loss or Euclidean loss.
    /// </summary>
    public class SquareLoss : ILoss<Tensor>
    {
        /// <inheritdoc />
        public float Loss(Tensor y, Tensor expected, bool calculateGradient)
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

            float[] yw = y.Weights;
            float[] ew = expected.Weights;

            if (calculateGradient)
            {
                Arrays.Copy(expected.Length, ew, 0, y.Gradient, 0);
            }

            if (y.Rank == 1)
            {
                return Calculate(expected.Length, 0, 0);
            }
            else
            {
                int mb = y.Axes[0];             // number of items in a mini-batch
                int mbsize = y.Strides[0];      // item size

                float loss = 0.0f;
                for (int i = 0, yi = 0, ei = 0; i < mb; i++, yi += mbsize, ei += mbsize)
                {
                    loss += Calculate(mbsize, yi, ei);
                }

                return loss / mb;
            }

            float Calculate(int length, int offy, int offe)
            {
                float sum = 0.0f;
                for (int i = 0; i < length; i++)
                {
                    float u = yw[offy + i] - ew[offe + i];
                    sum += u * u;
                }

                return (float)Math.Sqrt((float)sum) / length;
            }
        }
    }
}
