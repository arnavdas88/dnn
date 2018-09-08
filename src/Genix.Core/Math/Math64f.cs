// -----------------------------------------------------------------------
// <copyright file="Math64f.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides mathematical methods for double-precision floating point numbers.
    /// </summary>
    public static class Math64f
    {
        /// <summary>
        /// Returns the larger of two double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(double a, double b) => a >= b ? a : b;

        /// <summary>
        /// Returns the larger of three double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of three double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of three double-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of three double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(double a, double b, double c)
        {
            double ab = Math64f.Max(a, b);
            return Math64f.Max(ab, c);
        }

        /// <summary>
        /// Returns the larger of four double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of four double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of four double-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of four double-precision floating-point numbers to compare.</param>
        /// <param name="d">The forth of four double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(double a, double b, double c, double d)
        {
            double ab = Math64f.Max(a, b);
            double cd = Math64f.Max(c, d);
            return Math64f.Max(ab, cd);
        }

        /// <summary>
        /// Returns the smaller of two double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double a, double b) => a <= b ? a : b;

        /// <summary>
        /// Returns the smaller of three double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of three double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of three double-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of three double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double a, double b, double c)
        {
            double ab = Math64f.Min(a, b);
            return Math64f.Min(ab, c);
        }

        /// <summary>
        /// Returns the smaller of four double-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of four double-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of four double-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of four double-precision floating-point numbers to compare.</param>
        /// <param name="d">The forth of four double-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double a, double b, double c, double d)
        {
            double ab = Math64f.Min(a, b);
            double cd = Math64f.Min(c, d);
            return Math64f.Min(ab, cd);
        }
    }
}
