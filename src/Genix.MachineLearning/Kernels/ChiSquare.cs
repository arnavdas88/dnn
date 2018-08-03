// -----------------------------------------------------------------------
// <copyright file="ChiSquare.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    using System.Diagnostics;

    /// <summary>
    /// Represents the Chi-Square kernel comes from the Chi-Square distribution.
    /// </summary>
    public class ChiSquare
        : IKernel
    {
        /// <inheritdoc />
        public float Execute(int length, float[] x, int offx, float[] y, int offy)
        {
            float sum = 0.0f;

            for (int i = 0; i < length; i++)
            {
                float den = x[offx + i] + y[offy + i];
                if (den != 0.0f)
                {
                    float num = x[offx + i] - y[offy + i];
                    sum += (num * num) / den;
                }
            }

            return 1.0f - (2.0f * sum);
        }
    }
}
