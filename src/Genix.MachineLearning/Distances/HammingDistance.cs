// -----------------------------------------------------------------------
// <copyright file="HammingDistance.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Distances
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Measures the Hamming distance between elements of two vectors.
    /// </summary>
    public struct HammingDistance
        : IDistance<uint[], uint[], uint>,
          IDistance<ulong[], ulong[], ulong>
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint Distance(uint[] x, uint[] y)
        {
            return Vectors.HammingDistance(x.Length, x, 0, y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public ulong Distance(ulong[] x, ulong[] y)
        {
            return Vectors.HammingDistance(x.Length, x, 0, y, 0);
        }
    }
}
