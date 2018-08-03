// -----------------------------------------------------------------------
// <copyright file="RandomGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents standard random numbers generator.
    /// </summary>
    public class RandomGenerator : RandomNumberGenerator<float>
    {
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomGenerator"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RandomGenerator()
            : this(new Random(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomGenerator"/> class.
        /// </summary>
        /// <param name="random">The random number generator to use as a source of randomness.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RandomGenerator(Random random)
        {
            this.random = random;
        }

        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>
        /// A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Generate()
        {
            return (float)this.random.NextDouble();
        }

        /// <summary>
        /// Generates a random observation that is within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Generate(float minValue, float maxValue)
        {
            return (this.Generate() * (maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Generates a random vector of observations that are within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <param name="samples">The number of samples to generate.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(float minValue, float maxValue, int samples)
        {
            return this.Generate(minValue, maxValue, samples, null);
        }

        /// <summary>
        /// Generates a random vector of observations that are within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <param name="samples">The number of samples to generate.</param>
        /// <param name="result">The location where to store the samples.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(float minValue, float maxValue, int samples, float[] result)
        {
            if (result == null)
            {
                result = new float[samples];
            }

            for (int i = 0; i < samples; i++)
            {
                result[i] = this.Generate(minValue, maxValue);
            }

            return result;
        }
    }
}
