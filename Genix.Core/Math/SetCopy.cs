// -----------------------------------------------------------------------
// <copyright file="SetCopy.cs" company="Noname, Inc.">
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
    /// Provides set and copy operations.
    /// </summary>
    public static class SetCopy
    {
        /// <summary>
        /// Copies a range of values from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, float[] x, int offx, float[] y, int offy)
        {
            Debug.Assert(x.Length > offx + length - 1, "The source array should be big enough.");
            Debug.Assert(y.Length > offy + length - 1, "The destination array should be big enough.");
            NativeMethods.copyf(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="incx">The increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "length-1", Justification = "Done in debug mode only.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            Debug.Assert(x.Length > offx + ((length - 1) * incx), "The source array should be big enough.");
            Debug.Assert(y.Length > offy + ((length - 1) * incy), "The destination array should be big enough.");
            NativeMethods.copyf_inc(length, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Copies a range of signed 32-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.copyi32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of unsigned 32-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Copy(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.copyu32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of signed 64-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(int length, long[] x, int offx, long[] y, int offy)
        {
            NativeMethods.copyi64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies a range of unsigned 64-bit values from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Copy(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.copyu64(length, x, offx, y, offy);
        }

        /*/// <summary>
         /// Copies a range of values from a array starting at the specified source index
         /// to another array starting at the specified destination index.
         /// </summary>
         /// <param name="length">The number of elements to copy.</param>
         /// <param name="x">The array that contains the data to copy.</param>
         /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
         /// <param name="y">The array that receives the data.</param>
         /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
         [MethodImpl(MethodImplOptions.AggressiveInlining)]
         public static void Copy(int length, char[] x, int offx, char[] y, int offy)
         {
             NativeMethods.mkl_copyi16(length, x, offx, y, offy);
         }*/

        /// <summary>
        /// Sets values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, float value, float[] y, int offy)
        {
            NativeMethods.setf(length, value, y, offy);
        }

        /// <summary>
        /// Sets values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, float value, float[] y, int offy, int incy)
        {
            NativeMethods.setf_inc(length, value, y, offy, incy);
        }

        /// <summary>
        /// Sets signed 32-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, int value, int[] y, int offy)
        {
            NativeMethods.seti32(length, value, y, offy);
        }

        /// <summary>
        /// Sets unsigned 32-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, uint value, uint[] y, int offy)
        {
            NativeMethods.setu32(length, value, y, offy);
        }

        /// <summary>
        /// Sets signed 64-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, long value, long[] y, int offy)
        {
            NativeMethods.seti64(length, value, y, offy);
        }

        /// <summary>
        /// Sets unsigned 64-bit values in the array starting at the specified source index to the specified value.
        /// </summary>
        /// <param name="length">The number of elements to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(int length, ulong value, ulong[] y, int offy)
        {
            NativeMethods.setu64(length, value, y, offy);
        }

        /// <summary>
        /// Copies elements of an array to another array with unit increment.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="a">The array that contains the data to copy.</param>
        /// <param name="offa">The index in the <c>a</c> at which copying begins.</param>
        /// <param name="inca">The increment for the elements of <c>a</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
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
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(int length, float[] a, int offa, float[] y, int offy, int incy)
        {
            NativeMethods.unpack(length, a, offa, y, offy, incy);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyf(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyf_inc(int n, [In] float[] x, int offx, int incx, [Out] float[] y, int offy, int incy);

            /*[DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyi16(int n, [In] char[] x, int offx, [Out] char[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyi32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "copyi32")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyu32(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyi64(int n, [In] long[] x, int offx, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "copyi64")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void copyu64(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void setf(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void setf_inc(int n, float a, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void seti32(int n, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "seti32")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void setu32(int n, uint a, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void seti64(int n, long a, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "seti64")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void setu64(int n, ulong a, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void pack(int n, [In] float[] a, int offa, int inca, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void unpack(int n, [In] float[] a, int offa, [Out] float[] y, int offy, int incy);
        }
    }
}
