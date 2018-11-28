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

    /// <summary>
    /// Represents a discrete probability distribution.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbabilityDistribution"/> class.
        /// </summary>
        /// <param name="weights">The probabilities of possible outcomes.</param>
        /// <param name="random">The random number generator.</param>
        public ProbabilityDistribution(IList<float> weights, Random random)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            this.random = new RandomGeneratorF(random);

            this.pdf = weights.ToArray();
            float sum = Vectors.CumulativeSum(this.pdf.Length, this.pdf, 0);
            if (sum != 0.0f)
            {
                Vectors.DivC(this.pdf.Length, sum, this.pdf, 0);
            }
        }

        /// <summary>
        /// Returns next random outcome.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is greater than or equal to 0, and less than the number of possible outcomes.
        /// </returns>
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
