// -----------------------------------------------------------------------
// <copyright file="ProbabilityDistribution.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class ProbabilityDistribution
    {
        /// <summary>
        /// The random number generator.
        /// </summary>
        private readonly RandomNumberGenerator<float> random;

        /// <summary>
        /// The probability density function.
        /// </summary>
        private readonly float[] pdf;

        public ProbabilityDistribution(IList<float> weights, Random random)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            this.random = new RandomGenerator(random);

            this.pdf = weights.ToArray();
            float sum = Math32f.CumulativeSum(this.pdf.Length, this.pdf, 0);
            if (sum != 0.0f)
            {
                Math32f.DivC(this.pdf.Length, sum, this.pdf, 0);
            }
        }

        public int Next()
        {
            return BinarySearch(this.random.Generate());

            int BinarySearch(float value)
            {
                float[] pdf = this.pdf;

                int lo = 0;
                int hi = pdf.Length - 1;
                while (lo < hi)
                {
                    int i = lo + ((hi - lo) >> 1);
                    if (pdf[i] < value)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }

                Debug.Assert(lo >= 0 && lo < pdf.Length, "Must be in range.");
                return lo;
            }
        }
    }
}
