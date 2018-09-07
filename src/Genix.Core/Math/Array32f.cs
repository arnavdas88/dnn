// -----------------------------------------------------------------------
// <copyright file="Array32f.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides methods for single-precision floating point arrays.
    /// </summary>
    public static class Array32f
    {
        /// <summary>
        /// Determines whether the two array contain same data.
        /// </summary>
        /// <param name="length">The number of elements to compare.</param>
        /// <param name="x">The first array to compare.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The second array to compare.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// <b>true</b> if two arrays contain same data; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.compare_f32(length, x, offx, y, offy) == 0;
        }

        /// <summary>
        /// Sets all elements in the array to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, float value, float[] y, int offy)
        {
            NativeMethods.set_f32(length, value, y, offy);
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, float[] x, int offx, float[] y, int offy)
        {
            Debug.Assert(x.Length > offx + length - 1, "The source array should be big enough.");
            Debug.Assert(y.Length > offy + length - 1, "The destination array should be big enough.");
            NativeMethods.copy_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// <paramref name="x"/> and <paramref name="y"/> can overlap.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(int length, float[] x, int offx, float[] y, int offy)
        {
            Debug.Assert(x.Length > offx + length - 1, "The source array should be big enough.");
            Debug.Assert(y.Length > offy + length - 1, "The destination array should be big enough.");
            NativeMethods.move_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(int length, float[] x, int offx, float[] y, int offy) => NativeMethods.swap_f32(length, x, offx, y, offy);

        /// <summary>
        /// Performs thresholding of elements of an array.
        /// Elements that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="threshold">The threshold value to use for each element.</param>
        /// <param name="value">The value to set for each element that is smaller than the <paramref name="threshold"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThresholdLT(int length, float threshold, float value, float[] y, int offy)
        {
            NativeMethods.threshold_lt_ip_f32(length, threshold, value, y, offy);
        }

        /// <summary>
        /// Performs thresholding of elements of an array.
        /// Elements that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="threshold">The threshold value to use for each element.</param>
        /// <param name="value">The value to set for each element that is greater than the <paramref name="threshold"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThresholdGT(int length, float threshold, float value, float[] y, int offy)
        {
            NativeMethods.threshold_gt_ip_f32(length, threshold, value, y, offy);
        }

        /// <summary>
        /// Performs thresholding of elements of an array.
        /// Elements that are smaller or greater than the thresholds, are set to a specified values.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="thresholdLT">The lower threshold value to use for each element.</param>
        /// <param name="valueLT">The value to set for each element that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each element.</param>
        /// <param name="valueGT">The value to set for each element that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThresholdLTGT(int length, float thresholdLT, float valueLT, float thresholdGT, float valueGT, float[] y, int offy)
        {
            NativeMethods.threshold_ltgt_ip_f32(length, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern int compare_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void set_f32(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void copy_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void move_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void swap_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void threshold_lt_ip_f32(int n, float threshold, float value, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void threshold_gt_ip_f32(int n, float threshold, float value, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void threshold_ltgt_ip_f32(int n, float thresholdLT, float valueLT, float thresholdGT, float valueGT, [In, Out] float[] y, int offy);
        }
    }
}
