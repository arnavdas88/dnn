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
            public static extern float sum_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_ip_f32(int n, [In, Out] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);
        }
    }
}
