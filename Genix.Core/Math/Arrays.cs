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
    public static class Arrays
    {
        /// <summary>
        /// Determines whether the two array contain same data.
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
        public static bool Equals(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.fcompare(length, x, offx, y, offy) == 0;
        }

        /// <summary>
        /// Determines whether the two array contain same data.
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
        public static bool Equals(int length, int[] x, int offx, int[] y, int offy)
        {
            return NativeMethods.i32compare(length, x, offx, y, offy) == 0;
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
        public static bool Equals(int length, char[] x, int offx, char[] y, int offy)
        {
            return NativeMethods.ccompare(length, x, offx, y, offy) == 0;
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, float[] x, int offx, float[] y, int offy)
        {
            Debug.Assert(x.Length > offx + length - 1, "The source array should be big enough.");
            Debug.Assert(y.Length > offy + length - 1, "The destination array should be big enough.");
            NativeMethods._scopy(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "length-1", Justification = "Done in debug mode only.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            Debug.Assert(x.Length > offx + ((length - 1) * incx), "The source array should be big enough.");
            Debug.Assert(y.Length > offy + ((length - 1) * incy), "The destination array should be big enough.");
            NativeMethods.scopy_inc(length, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Copies a range of signed 32-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.i32copy(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of unsigned 32-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Copy(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.u32copy(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of signed 64-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, long[] x, int offx, long[] y, int offy)
        {
            NativeMethods.i64copy(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of unsigned 64-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Copy(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.u64copy(length, x, offx, y, offy);
        }

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
             NativeMethods.mkl_i16copy(length, x, offx, y, offy);
         }*/

        /// <summary>
        /// Sets values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, float value, float[] y, int offy)
        {
            NativeMethods.sset(length, value, y, offy);
        }

        /// <summary>
        /// Sets values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, float value, float[] y, int offy, int incy)
        {
            NativeMethods.sset_inc(length, value, y, offy, incy);
        }

        /// <summary>
        /// Sets signed 32-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, int value, int[] y, int offy)
        {
            NativeMethods.i32set(length, value, y, offy);
        }

        /// <summary>
        /// Sets unsigned 32-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, uint value, uint[] y, int offy)
        {
            NativeMethods.u32set(length, value, y, offy);
        }

        /// <summary>
        /// Sets signed 64-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, long value, long[] y, int offy)
        {
            NativeMethods.i64set(length, value, y, offy);
        }

        /// <summary>
        /// Sets unsigned 64-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, ulong value, ulong[] y, int offy)
        {
            NativeMethods.u64set(length, value, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.i32swap(length, x, offx, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays of 32-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Swap(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.u32swap(length, x, offx, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays of 64-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(int length, long[] x, int offx, long[] y, int offy)
        {
            NativeMethods.i64swap(length, x, offx, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays of 64-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Swap(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.u64swap(length, x, offx, y, offy);
        }

        /// <summary>
        /// Swaps elements of two arrays of floats.
        /// </summary>
        /// <param name="length">The number of elements to swap.</param>
        /// <param name="x">The first array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The second array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods._sswap(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies elements of an array to another array with unit increment.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="a">The array that contains the data to copy.</param>
        /// <param name="offa">The index in the <c>a</c> at which copying begins.</param>
        /// <param name="inca">The increment for the elements of <c>a</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(int length, float[] a, int offa, int inca, float[] y, int offy)
        {
            NativeMethods.pack(length, a, offa, inca, y, offy);
        }

        /// <summary>
        /// Copies elements of an array with unit increment to another array.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="a">The array that contains the data to copy.</param>
        /// <param name="offa">The index in the <c>a</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(int length, float[] a, int offa, float[] y, int offy, int incy)
        {
            NativeMethods.unpack(length, a, offa, y, offy, incy);
        }

        /// <summary>
        /// Clips array values to a specified minimum and maximum values.
        /// </summary>
        /// <param name="length">The number of elements to clip.</param>
        /// <param name="minValue">The minimum value to clip by.</param>
        /// <param name="maxValue">The maximum value to clip by.</param>
        /// <param name="x">The array that contains the data to clip.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which clipping begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>x(offx + i) := min(max(x(offx + i), minValue), maxValue)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clip(int length, float minValue, float maxValue, float[] x, int offx)
        {
            if (!float.IsNaN(minValue))
            {
                Maximum.Min(length, x, offx, minValue, x, offx);
            }

            if (!float.IsNaN(maxValue))
            {
                Maximum.Max(length, x, offx, maxValue, x, offx);
            }
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index
        /// to another array starting at the specified destination index
        /// specified number of times.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="count">The number of times to copy <paramref name="x"/>.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tile(int length, int count, float[] x, int offx, float[] y, int offy)
        {
            for (int i = 0; i < count; i++, offy += length)
            {
                Arrays.Copy(length, x, offx, y, offy);
            }
        }

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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace(int length, float[] x, int offx, float oldValue, float newValue, float[] y, int offy)
        {
            NativeMethods.sreplace(length, x, offx, oldValue, newValue, y, offy);
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an array.
        /// </summary>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="x">The array to sort.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which sorting begins.</param>
        /// <param name="ascending"><b>true</b> to use ascending sorting order; <b>false</b> to use descending sorting order.</param>
        public static void Sort(int length, float[] x, int offx, bool ascending)
        {
            NativeMethods.fqsort(length, x, offx, ascending);
        }

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
        public static void Sort(int length, float[] x, int offx, int[] y, int offy, bool ascending)
        {
            NativeMethods.fqsortv(length, x, offx, y, offy, ascending);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int fcompare(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int i32compare(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int ccompare(int n, [In] char[] x, int offx, [Out] char[] y, int offy);

            /*[DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i16copy(int n, [In] char[] x, int offx, [Out] char[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i32copy(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i32copy")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u32copy(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i64copy(int n, [In] long[] x, int offx, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i64copy")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u64copy(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void _scopy(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void scopy_inc(int n, [In] float[] x, int offx, int incx, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i32set(int n, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i32set")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u32set(int n, uint a, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i64set(int n, long a, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i64set")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u64set(int n, ulong a, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sset(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sset_inc(int n, float a, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sreplace(int n, [In] float[] x, int offx, float oldValue, float newValue, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void pack(int n, [In] float[] a, int offa, int inca, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void unpack(int n, [In] float[] a, int offa, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i32swap(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i32swap")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u32swap(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void i64swap(int n, [In] long[] x, int offx, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "i64swap")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void u64swap(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void _sswap(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void fqsort(
                int n,
                [In, Out] float[] x,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool ascending);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
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
