// -----------------------------------------------------------------------
// <copyright file="Arrays.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides array manipulation methods.
    /// </summary>
    public static partial class Arrays
    {
        /// <summary>
        /// Creates an array of zero-based indexes of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements in the array.</param>
        /// <returns>
        /// The allocated array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] Indexes(int length)
        {
            int[] a = new int[length];
            for (int i = 0; i < length; i++)
            {
                a[i] = i;
            }

            return a;
        }

        /// <summary>
        /// Determines whether the two arrays contain same data.
        /// </summary>
        /// <param name="length">The number of elements to check.</param>
        /// <param name="x">The first array to compare.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which comparing begins.</param>
        /// <param name="y">The second array to compare.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which comparing begins.</param>
        /// <returns>
        /// <b>true</b> if two arrays contain same data; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(int length, char[] x, int offx, char[] y, int offy) => NativeMethods.compare_s16(length, x, offx, y, offy) == 0;

        /// <summary>
        /// Copies values from source pointer to destination pointer in strides.
        /// </summary>
        /// <param name="count">The number of strides to copy.</param>
        /// <param name="x">The pointer to source data.</param>
        /// <param name="stridex">The number of bytes in <paramref name="x"/> stride.</param>
        /// <param name="y">The pointer to destination data.</param>
        /// <param name="stridey">The number of bytes in <paramref name="y"/> stride.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyStrides(int count, IntPtr x, int stridex, IntPtr y, int stridey) => NativeMethods.copy_strides_s8(count, x, stridex, y, stridey);

        /*/// <summary>
         /// Copies a range of values from a array starting at the specified source index
         /// to another array starting at the specified destination index.
         /// </summary>
         /// <param name="length">The number of elements to copy.</param>
         /// <param name="x">The array that contains the data to copy.</param>
         /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
         /// <param name="y">The array that receives the data.</param>
         /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
         [MethodImpl(MethodImplOptions.AggressiveInlining)]
         public static void Copy(int length, char[] x, int offx, char[] y, int offy)
         {
             NativeMethods.mkl_copy_s16(length, x, offx, y, offy);
         }*/

        /// <summary>
        /// Copies elements of an array to another array with unit increment.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="a">The array that contains the data to copy.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which copying begins.</param>
        /// <param name="inca">The increment for the elements of <paramref name="a"/>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(int length, float[] a, int offa, int inca, float[] y, int offy) => NativeMethods.pack(length, a, offa, inca, y, offy);

        /// <summary>
        /// Copies elements of an array with unit increment to another array.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="a">The array that contains the data to copy.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(int length, float[] a, int offa, float[] y, int offy, int incy) => NativeMethods.unpack(length, a, offa, y, offy, incy);

        /// <summary>
        /// Replaces all occurrences of the specified value in the array with another specified value.
        /// </summary>
        /// <param name="length">The number of elements to replace.</param>
        /// <param name="x">The array that contains the data to replace.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which replacing begins.</param>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The value to replace all occurrences of <c>oldValue</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which replacing begins.</param>
        /// <remarks>
        /// <para>
        /// The method replaces all occurrences of <c>oldValue</c> in <paramref name="x"/> with <c>newValue</c> and puts the results in <paramref name="y"/>.
        /// </para>
        /// <para>
        /// To remove <see cref="float.NaN"/> from the array call this method with <c>oldValue</c> of <see cref="float.NaN"/>.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace(int length, float[] x, int offx, float oldValue, float newValue, float[] y, int offy) => NativeMethods.sreplace(length, x, offx, oldValue, newValue, y, offy);

        /// <summary>
        /// Sorts the elements in a range of elements in an array.
        /// </summary>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="x">The array to sort.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which sorting begins.</param>
        /// <param name="ascending"><b>true</b> to use ascending sorting order; <b>false</b> to use descending sorting order.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(int length, float[] x, int offx, bool ascending) => NativeMethods.fqsort(length, x, offx, ascending);

        /// <summary>
        /// Sorts the elements in a range of elements in a pair of arrays
        /// (one contains the keys and the other contains the corresponding items)
        /// based on the keys in the first array.
        /// </summary>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="x">The array that contains the keys to sort.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which sorting begins.</param>
        /// <param name="y">The array that contains the items that correspond to each of the keys in the <paramref name="x"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which sorting begins.</param>
        /// <param name="ascending"><b>true</b> to use ascending sorting order; <b>false</b> to use descending sorting order.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(int length, float[] x, int offx, int[] y, int offy, bool ascending) => NativeMethods.fqsortv(length, x, offx, y, offy, ascending);

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            public static extern int compare_s16(int n, [In] char[] x, int offx, [Out] char[] y, int offy);

            /*[DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            public static extern void copy_s16(int n, [In] char[] x, int offx, [Out] char[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            public static extern void copy_strides_s8(int nstrides, IntPtr x, int stridex, IntPtr y, int stridey);

            [DllImport(NativeMethods.DllName)]
            public static extern void sreplace(int n, [In] float[] x, int offx, float oldValue, float newValue, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void pack(int n, [In] float[] a, int offa, int inca, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void unpack(int n, [In] float[] a, int offa, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void fqsort(
                int n,
                [In, Out] float[] x,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool ascending);

            [DllImport(NativeMethods.DllName)]
            public static extern void fqsortv(
                int n,
                [In, Out] float[] x,
                int offx,
                [In, Out] int[] y,
                int offy,
                [MarshalAs(UnmanagedType.Bool)] bool ascending);
        }
    }
}
