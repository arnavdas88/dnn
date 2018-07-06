// -----------------------------------------------------------------------
// <copyright file="RandomGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents standard random numbers generator.
    /// </summary>
    public class RandomGenerator : RandomNumberGenerator
    {
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomGenerator"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RandomGenerator() : this(new Random(0))
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
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Generate()
        {
            return (float)this.random.NextDouble();
        }
    }
}
