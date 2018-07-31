// -----------------------------------------------------------------------
// <copyright file="ChiSquare.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    /// <summary>
    /// Represents the Chi-Square kernel comes from the Chi-Square distribution.
    /// </summary>
    public class ChiSquare
        : IKernel<float[]>
    {
        /// <inheritdoc />
        public float Execute(float[] x, float[] y)
        {
            float sum = 0.0f;

            for (int i = 0, ii = x.Length; i < ii; i++)
            {
                float den = 0.5f * (x[i] + y[i]);
                if (den != 0.0f)
                {
                    float num = x[i] - y[i];
                    sum += (num * num) / den;
                }
            }

            return 1.0f - sum;
        }
    }
}
