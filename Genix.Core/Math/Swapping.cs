// -----------------------------------------------------------------------
// <copyright file="Swapping.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides data swapping methods.
    /// </summary>
    public static class Swapping
    {
        /// <summary>
        /// Swaps two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to swap.</param>
        /// <param name="b">The second of two 32-bit signed integers to swap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Need both parameters as references for swapping.")]
        public static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}
