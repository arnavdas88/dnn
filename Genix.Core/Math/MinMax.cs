// -----------------------------------------------------------------------
// <copyright file="MinMax.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides methods to calculate minimums and maximums.
    /// </summary>
    public static class MinMax
    {
        /// <summary>
        /// Returns the larger of two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b)
        {
            return a >= b ? a : b;
        }

        /// <summary>
        /// Returns the larger of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b, int c)
        {
            int ab = MinMax.Max(a, b);
            return MinMax.Max(ab, c);
        }

        /// <summary>
        /// Returns the larger of four 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c> or <c>d</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b, int c, int d)
        {
            int ab = MinMax.Max(a, b);
            int cd = MinMax.Max(c, d);
            return MinMax.Max(ab, cd);
        }

        /// <summary>
        /// Returns the larger of two single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b)
        {
            return a >= b ? a : b;
        }

        /// <summary>
        /// Returns the smaller of two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b)
        {
            return a <= b ? a : b;
        }

        /// <summary>
        /// Returns the smaller of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b, int c)
        {
            int ab = MinMax.Min(a, b);
            return MinMax.Min(ab, c);
        }

        /// <summary>
        /// Returns the smaller of four 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c> or <c>d</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b, int c, int d)
        {
            int ab = MinMax.Min(a, b);
            int cd = MinMax.Min(c, d);
            return MinMax.Min(ab, cd);
        }

        /// <summary>
        /// Returns the smaller of two single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b)
        {
            return a <= b ? a : b;
        }
    }
}
