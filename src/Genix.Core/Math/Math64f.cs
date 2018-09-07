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
        /// Computes the absolute value of elements of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = Math.Abs(y)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(int length, double[] y, int offy)
        {
            NativeMethods.abs_ip_f64(length, y, offy);
        }

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

        /// <summary>
        /// Multiplies each element of an array by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, double alpha, double[] y, int offy)
        {
            NativeMethods.mulc_ip_f64(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] := x[i] * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, double[] x, int offx, double alpha, double[] y, int offy)
        {
            NativeMethods.mulc_f64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Divides each element of an array by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="alpha">The scalar to divide.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] /= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DivC(int length, double alpha, double[] y, int offy)
        {
            NativeMethods.divc_ip_f64(length, alpha, y, offy);
        }

        /// <summary>
        /// Divides each element of an array by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to divide.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = x[i] / alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DivC(int length, double[] x, int offx, double alpha, double[] y, int offy)
        {
            NativeMethods.divc_f64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Computes the Manhattan distance between elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The first array <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The first array <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Manhattan distance between elements of two arrays.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as sum(abs(x[i] - y[i])).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ManhattanDistance(int length, double[] x, int offx, double[] y, int offy)
        {
            return NativeMethods.manhattan_distance_f64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the Euclidean distance between elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The first array <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The first array <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Euclidean distance between elements of two arrays.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as sqrt(sum((x[i] - y[i])^2)).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double EuclideanDistance(int length, double[] x, int offx, double[] y, int offy)
        {
            return NativeMethods.euclidean_distance_f64(length, x, offx, y, offy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void abs_ip_f64(int n, [Out] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_ip_f64(int n, double a, [In, Out] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_f64(int n, [In] double[] x, int offx, double a, [Out] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_ip_f64(int n, double a, [In, Out] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_f64(int n, [In] double[] x, int offx, double a, [Out] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern double manhattan_distance_f64(int n, [In] double[] x, int offx, [In] double[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern double euclidean_distance_f64(int n, [In] double[] x, int offx, [In] double[] y, int offy);
        }
    }
}
