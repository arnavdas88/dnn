// -----------------------------------------------------------------------
// <copyright file="Math32f.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides mathematical methods for single-precision floating point numbers.
    /// </summary>
    public static class Math32f
    {
        /// <summary>
        /// Adds the elements of two arrays with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.add_inc_ip_f32(length, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Adds the elements of two arrays with increment not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="inca">The increment for the elements of <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="incb">The increment for the elements of <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.add_inc_f32(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Divides the elements of two arrays in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y /= x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Div(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.div_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Divides the elements of two arrays not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = a / b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Div(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.div_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Computes the sum of elements of an array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The sum of elements in <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(int length, float[] x, int offx)
        {
            return NativeMethods.sum_f32(length, x, offx);
        }

        /// <summary>
        /// Computes the cumulative sum of elements of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source and destination array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The sum of elements in <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CumulativeSum(int length, float[] x, int offx)
        {
            return NativeMethods.cumulative_sum_ip_f32(length, x, offx);
        }

        /// <summary>
        /// Computes the cumulative sum of elements of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The sum of elements in <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CumulativeSum(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.cumulative_sum_f32(length, x, offx, y, offy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void add_inc_ip_f32(int n, [In] float[] x, int offx, int incx, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_inc_f32(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void div_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void div_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float sum_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_ip_f32(int n, [In, Out] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);
        }
    }
}
