// -----------------------------------------------------------------------
// <copyright file="GaussianGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Gaussian random numbers generator.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The random number generator generates Gaussian random numbers with specified mean and standard deviation values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>Sample usage:</para>
    /// <code>
    /// // create instance of random generator
    /// IRandomNumberGenerator generator = new GaussianGenerator(5.0, 1.5);
    /// // generate random number
    /// float randomNumber = generator.Generate();
    /// </code>
    /// </example>
    public class GaussianGenerator : RandomNumberGenerator
    {
        private readonly double mean;
        private readonly double standardDeviation;
        private readonly Random random;

        private bool useSecond = false;
        private double secondValue = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianGenerator"/> class with given mean and standard deviation.
        /// </summary>
        /// <param name="mean">The distribution's mean value μ (mu).</param>
        /// <param name="standardDeviation">The distribution's standard deviation σ (sigma).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GaussianGenerator(double mean, double standardDeviation) : this(mean, standardDeviation, new Random(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianGenerator"/> class with given mean and standard deviation.
        /// </summary>
        /// <param name="mean">The distribution's mean value μ (mu).</param>
        /// <param name="standardDeviation">The distribution's standard deviation σ (sigma).</param>
        /// <param name="random">The random number generator to use as a source of randomness.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GaussianGenerator(double mean, double standardDeviation, Random random)
        {
            this.mean = mean;
            this.standardDeviation = standardDeviation;
            this.random = random;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="GaussianGenerator"/> class from being created.
        /// </summary>
        private GaussianGenerator()
        {
        }

        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Generate()
        {
            return (float)(this.mean + (this.GaussRandom() * this.standardDeviation));
        }

        private double GaussRandom()
        {
            if (this.useSecond)
            {
                this.useSecond = false;
                return this.secondValue;
            }

            double u, v, r;
            do
            {
                u = (2.0 * this.random.NextDouble()) - 1.0;
                v = (2.0 * this.random.NextDouble()) - 1.0;
                r = (u * u) + (v * v);
            }
            while (r == 0.0 || r >= 1.0);

            double c = Math.Sqrt(-2.0 * Math.Log(r) / r);

            this.secondValue = v * c; // cache this
            this.useSecond = true;
            return u * c;
        }
    }
}
