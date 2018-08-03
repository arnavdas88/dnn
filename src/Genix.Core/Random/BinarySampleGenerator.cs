// -----------------------------------------------------------------------
// <copyright file="BinarySampleGenerator.cs" company="Noname, Inc.">
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
    public class BinarySampleGenerator : RandomNumberGenerator<bool>
    {
        private readonly double positiveRatio;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySampleGenerator"/> class.
        /// </summary>
        /// <param name="positiveRatio">The percentage of positive samples in generated distribution, a value from 0.0 to 1.0.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinarySampleGenerator(double positiveRatio)
            : this(positiveRatio, new Random(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySampleGenerator"/> class.
        /// </summary>
        /// <param name="positiveRatio">The percentage of positive samples in generated distribution, a value from 0.0 to 1.0.</param>
        /// <param name="random">The random number generator to use as a source of randomness.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinarySampleGenerator(double positiveRatio, Random random)
        {
            this.positiveRatio = positiveRatio;
            this.random = random;
        }

        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Generate()
        {
            return this.random.NextDouble() < this.positiveRatio;
        }
    }
}