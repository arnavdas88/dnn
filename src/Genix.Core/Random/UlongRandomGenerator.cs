// -----------------------------------------------------------------------
// <copyright file="UlongRandomGenerator.cs" company="Noname, Inc.">
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
    [CLSCompliant(false)]
    public class UlongRandomGenerator : RandomNumberGenerator<ulong>
    {
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="UlongRandomGenerator"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UlongRandomGenerator()
            : this(new Random(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UlongRandomGenerator"/> class.
        /// </summary>
        /// <param name="random">The random number generator to use as a source of randomness.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is <b>null</b>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UlongRandomGenerator(Random random)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A 64-bit integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Generate()
        {
            return (ulong)(uint)this.random.Next() |
                (this.random.Next(0, 2) == 0 ? 0x8000_0000ul : 0ul) |
                (ulong)(uint)this.random.Next() << 32 |
                (this.random.Next(0, 2) == 0 ? 0x8000_0000_0000_0000ul : 0ul);
        }
    }
}
