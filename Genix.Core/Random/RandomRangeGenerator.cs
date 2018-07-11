// -----------------------------------------------------------------------
// <copyright file="RandomRangeGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents random numbers generator that generates random numbers within a specified range. 
    /// </summary>
    public class RandomRangeGenerator : RandomNumberGenerator
    {
        private readonly float minValue;
        private readonly float maxValue;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomRangeGenerator"/> class.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <c>maxValue</c> must be greater than or equal to <c>minValue</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RandomRangeGenerator(float minValue, float maxValue) : this(minValue, maxValue, new Random(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomRangeGenerator"/> class.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <c>maxValue</c> must be greater than or equal to <c>minValue</c>.</param>
        /// <param name="random">The random number generator to use as a source of randomness.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RandomRangeGenerator(float minValue, float maxValue, Random random)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.random = random;
        }

        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Generate()
        {
            return ((float)this.random.NextDouble() * (this.maxValue - this.minValue)) + this.minValue;
        }
    }
}
