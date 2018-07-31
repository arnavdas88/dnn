// -----------------------------------------------------------------------
// <copyright file="Mathematics.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides mathematical methods.
    /// </summary>
    public static class Mathematics
    {
        /// <summary>
        /// Computes absolute value of a range of values from one array starting at the specified source index
        /// and stores results in another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to compute.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sabs(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the argument of the <see cref="Abs"/> method.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The <see cref="Abs"/> method input array <paramref name="x"/>.</param>
        /// <param name="dx">The array that contains calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="y">The <see cref="Abs"/> method output array <paramref name="y"/>.</param>
        /// <param name="dy">The array that contains gradient for <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> and <c>dy</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offx + i) += x(offx + i) == y(offy + i) ? dy(offy + i) : -dy(offy + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AbsGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] y, float[] dy, int offy)
        {
            NativeMethods.sabs_gradient(length, x, dx, offx, cleardx, y, dy, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.addc_ip_s32(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(int length, uint alpha, uint[] y, int offy)
        {
            NativeMethods.addc_ip_u32(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, long alpha, long[] y, int offy)
        {
            NativeMethods.addc_ip_s64(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(int length, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.addc_ip_u64(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of floats in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.addc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.addc_s32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(int length, uint[] x, int offx, uint alpha, uint[] y, int offy)
        {
            NativeMethods.addc_u32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, long[] x, int offx, long alpha, long[] y, int offy)
        {
            NativeMethods.addc_s64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(int length, ulong[] x, int offx, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.addc_u64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of floats not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.addc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.add_ip_s32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.add_ip_u32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, long[] x, int offx, long[] y, int offy)
        {
            NativeMethods.add_ip_s64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.add_ip_u64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of floats in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.add_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of floats with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.sadd_inc(length, y, offy, incy, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, int[] a, int offa, int[] b, int offb, int[] y, int offy)
        {
            NativeMethods.add_s32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(int length, uint[] a, int offa, uint[] b, int offb, uint[] y, int offy)
        {
            NativeMethods.add_u32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, long[] a, int offa, long[] b, int offb, long[] y, int offy)
        {
            NativeMethods.add_s64(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(int length, ulong[] a, int offa, ulong[] b, int offb, ulong[] y, int offy)
        {
            NativeMethods.add_u64(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of floats not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.add_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays of floats with increment not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which adding begins.</param>
        /// <param name="inca">The increment for the elements of <paramref name="a"/>.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which adding begins.</param>
        /// <param name="incb">The increment for the elements of <paramref name="b"/>.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.sadd_inc(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.subc_ip_s32(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 32-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void SubC(int length, uint alpha, uint[] y, int offy)
        {
            NativeMethods.subc_ip_u32(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 64-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, long alpha, long[] y, int offy)
        {
            NativeMethods.subc_ip_s64(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 64-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void SubC(int length, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.subc_ip_u64(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of floats in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.subc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 32-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.subc_s32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 32-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void SubC(int length, uint[] x, int offx, uint alpha, uint[] y, int offy)
        {
            NativeMethods.subc_u32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 64-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, long[] x, int offx, long alpha, long[] y, int offy)
        {
            NativeMethods.subc_s64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of 64-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void SubC(int length, ulong[] x, int offx, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.subc_u64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array of floats not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.subc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.sub_ip_s32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 32-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Sub(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.sub_ip_u32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 64-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, long[] x, int offx, long[] y, int offy)
        {
            NativeMethods.sub_ip_s64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 64-bit unsigned integers in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Sub(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.sub_ip_u64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sub_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which subtraction begins.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.ssub_inc(length, y, offy, incy, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 32-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, int[] a, int offa, int[] b, int offb, int[] y, int offy)
        {
            NativeMethods.sub_s32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 32-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Sub(int length, uint[] a, int offa, uint[] b, int offb, uint[] y, int offy)
        {
            NativeMethods.sub_u32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 64-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, long[] a, int offa, long[] b, int offb, long[] y, int offy)
        {
            NativeMethods.sub_s64(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of 64-bit unsigned integers not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Sub(int length, ulong[] a, int offa, ulong[] b, int offb, ulong[] y, int offy)
        {
            NativeMethods.sub_u64(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) = a(offa + i) + b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.sub_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats with increment not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains source data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which subtraction begins.</param>
        /// <param name="inca">The increment for the elements of <paramref name="a"/>.</param>
        /// <param name="b">The second array that contains source data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which subtraction begins.</param>
        /// <param name="incb">The increment for the elements of <paramref name="b"/>.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which subtraction begins.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.ssub_inc(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified source index
        /// to another array starting at the specified destination index
        /// if values in mask arrays match.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="maskx">The first array that contains the data to compare.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="masky">The second array that contains the data to compare.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := y(i) + (xmask(offx + i) == ymask(offy + i) ? x(offx + i) : 0)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MatchAndAdd(int length, float[] x, float[] maskx, int offx, float[] y, float[] masky, int offy)
        {
            NativeMethods.smatchandadd(length, x, maskx, offx, y, masky, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 32-bit integers by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.mulc_ip_s32(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 32-bit unsigned integers by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void MulC(int length, uint alpha, uint[] y, int offy)
        {
            NativeMethods.mulc_ip_u32(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 64-bit integers by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, long alpha, long[] y, int offy)
        {
            NativeMethods.mulc_ip_s64(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 64-bit unsigned integers by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void MulC(int length, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.mulc_ip_u64(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of single-precision floating-point numbers by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains source and destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.mulc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 32-bit integers by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which multiplication begins.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.mulc_s32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 32-bit unsigned integers by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which multiplication begins.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void MulC(int length, uint[] x, int offx, uint alpha, uint[] y, int offy)
        {
            NativeMethods.mulc_u32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 64-bit integers by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which multiplication begins.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, long[] x, int offx, long alpha, long[] y, int offy)
        {
            NativeMethods.mulc_s64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of 64-bit unsigned integers by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which multiplication begins.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void MulC(int length, ulong[] x, int offx, ulong alpha, ulong[] y, int offy)
        {
            NativeMethods.mulc_u64(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array of single-precision floating-point numbers by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The array that contains source data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which multiplication begins.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The array that contains destination data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which multiplication begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.mulc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <paramref name="a"/>.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which computation begins.</param>
        /// <param name="b">The input array <paramref name="b"/>.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which computation begins.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := a(offa + i) * b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.smul(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Divides elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <paramref name="a"/>.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which computation begins.</param>
        /// <param name="b">The input array <paramref name="b"/>.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which computation begins.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := a(offa + i) / b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.sdiv(length, a, offa, 1, b, offb, 1, y, offy, 1);
        }

        /// <summary>
        /// Divides elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <paramref name="a"/>.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which computation begins.</param>
        /// <param name="inca">the increment for the elements of <paramref name="a"/>.</param>
        /// <param name="b">The input array <paramref name="b"/>.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which computation begins.</param>
        /// <param name="incb">the increment for the elements of <paramref name="b"/>.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i * incy) := a(offa + i * inca) / b(offb + i * incb)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.sdiv(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Divides all elements of one array by a scalar and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to divide.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := x / alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float alpha, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mulc_f32(length, x, offx, 1.0f / alpha, y, offy);
        }

        /// <summary>
        /// Multiplies elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and adds results to the destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <paramref name="a"/>.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which computation begins.</param>
        /// <param name="b">The input array <paramref name="b"/>.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which computation begins.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += a(offa + i) * b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.smuladd(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods._saxpy(length, alpha, x, offx, 1, y, offy, 1);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + beta * y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, float beta, float[] y, int offy)
        {
            NativeMethods._saxpby(length, alpha, x, offx, 1, beta, y, offy, 1);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <paramref name="x"/>.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + beta * y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, int incx, float beta, float[] y, int offy, int incy)
        {
            NativeMethods._saxpby(length, alpha, x, offx, incx, beta, y, offy, incy);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods._saxpy(length, alpha, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Squares elements from one array starting at the specified index
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Square(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.ssqr(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a square root of elements of one array starting at the specified index
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := sqrt(x(offx + i))</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sqrt(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.ssqrt(length, x, offx, y, offy);
        }

        /// <summary>
        /// Raises elements of one array starting at the specified index to the scalar power
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="power">The constant value for power.</param>
        /// <param name="y">The output array <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pow(int length, float[] x, int offx, float power, float[] y, int offy)
        {
            NativeMethods.spowx(length, x, offx, power, y, offy);
        }

        /// <summary>
        /// Computes the derivative of <see cref="Pow"/> method.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <paramref name="x"/>.</param>
        /// <param name="dx">The output array <c>dx</c>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="power">The constant value for power.</param>
        /// <param name="dy">The chain gradient array <c>dy</c>.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offdx + i) += p * x(offx + i) ^ (p-1) * dy(offdy + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PowGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float power, float[] dy, int offdy)
        {
            NativeMethods.powx_gradient(length, x, dx, offx, cleardx, power, dy, offdy);
        }

        /// <summary>
        /// Computes a sum of two logarithms using Log-Sum-Exp trick.
        /// </summary>
        /// <param name="a">The first value to add.</param>
        /// <param name="b">The second value to add.</param>
        /// <returns>The resulting value equal to log(exp(a) + exp(b)).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LogSumExp(float a, float b)
        {
            return NativeMethods.slogSumExp2(a, b);
        }

        /// <summary>
        /// Computes a natural logarithm element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.slog(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes an exponential element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exp(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sexp(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a sines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sin(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.ssin(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a gradient of a sines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains <see cref="Mathematics.Sin"/> method input.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] dy, int offdy)
        {
            NativeMethods.ssin_gradient(length, x, dx, offx, cleardx, dy, offdy);
        }

        /// <summary>
        /// Computes a cosines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cos(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.scos(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a gradient of a cosines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains <see cref="Mathematics.Cos"/> method input.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CosGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] dy, int offdy)
        {
            NativeMethods.scos_gradient(length, x, dx, offx, cleardx, dy, offdy);
        }

        /// <summary>
        /// Computes the L1-Norm (sum of magnitudes) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <paramref name="x"/>.</param>
        /// <returns>
        /// The L1-Norm of array elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L1Norm(int length, float[] x, int offx, int incx)
        {
            return NativeMethods._snrm1(length, x, offx, incx);
        }

        /// <summary>
        /// Computes the L2-Norm (Euclidian norm) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <paramref name="x"/>.</param>
        /// <returns>
        /// The L2-Norm of array elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L2Norm(int length, float[] x, int offx, int incx)
        {
            return NativeMethods._snrm2(length, x, offx, incx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 8-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(int length, byte[] x, int offx)
        {
            return NativeMethods.sum_u8(length, x, offx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(int length, int[] x, int offx)
        {
            return NativeMethods.sum_s32(length, x, offx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 32-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Sum(int length, uint[] x, int offx)
        {
            return NativeMethods.sum_u32(length, x, offx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 64-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(int length, long[] x, int offx)
        {
            return NativeMethods.sum_s64(length, x, offx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 64-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Sum(int length, ulong[] x, int offx)
        {
            return NativeMethods.sum_u64(length, x, offx);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of floats.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which adding begins.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(int length, float[] x, int offx)
        {
            return NativeMethods.sum_f32(length, x, offx);
        }

        /// <summary>
        /// Computes the variance of all elements in the array of floats.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which calculation begins.</param>
        /// <returns>
        /// The variance of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Variance(int length, float[] x, int offx)
        {
            return NativeMethods.svariance(length, x, offx);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sabs(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sabs_gradient(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                [In] float[] dy,
                int offy);

            /*[DllImport(NativeMethods.DllName)]
             [SuppressUnmanagedCodeSecurity]
             public static extern void sinv(int n, [In] float[] a, int offa, [Out] float[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_u32(int n, uint a, [In, Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_s64(int n, long a, [In, Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_u64(int n, ulong a, [In, Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_u32(int n, [In] uint[] x, int offx, uint a, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_s64(int n, [In] long[] x, int offx, long a, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_u64(int n, [In] ulong[] x, int offx, ulong a, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_s32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_u32(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_s64(int n, [In] long[] x, int offx, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_u64(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_s32(int n, [In] int[] a, int offa, [In] int[] b, int offb, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_u32(int n, [In] uint[] a, int offa, [In] uint[] b, int offb, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_s64(int n, [In] long[] a, int offa, [In] long[] b, int offb, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_u64(int n, [In] ulong[] a, int offa, [In] ulong[] b, int offb, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sadd_inc(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smatchandadd(int n, [In] float[] x, [In] float[] xmask, int offx, [Out] float[] y, [In] float[] ymask, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_u32(int n, uint a, [In, Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_s64(int n, long a, [In, Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_u64(int n, ulong a, [In, Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_u32(int n, [In] uint[] x, int offx, uint a, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_s64(int n, [In] long[] x, int offx, long a, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_u64(int n, [In] ulong[] x, int offx, ulong a, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_s32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_u32(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_s64(int n, [In] long[] x, int offx, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_u64(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_s32(int n, [In] int[] a, int offa, [In] int[] b, int offb, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_u32(int n, [In] uint[] a, int offa, [In] uint[] b, int offb, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_s64(int n, [In] long[] a, int offa, [In] long[] b, int offb, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_u64(int n, [In] ulong[] a, int offa, [In] ulong[] b, int offb, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void ssub_inc(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            /*[DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smulc_inc(int n, float a, [In, Out] float[] x, int offx, int incx);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_u32(int n, uint a, [In, Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_s64(int n, long a, [In, Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_u64(int n, ulong a, [In, Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_u32(int n, [In] uint[] x, int offx, uint a, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_s64(int n, [In] long[] x, int offx, long a, [Out] long[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_u64(int n, [In] ulong[] x, int offx, ulong a, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smul(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smuladd(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sdiv(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void _saxpy(int n, float a, [In] float[] x, int offx, int incx, [In, Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void _saxpby(int n, float a, [In] float[] x, int offx, int incx, float b, [In, Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void ssqr(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void ssqrt(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void spowx(int n, [In] float[] a, int offa, float b, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void powx_gradient(
                int n,
                [In] float[] x,
                [In, Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                float power,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float slogSumExp2(float a, float b);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void slog(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sexp(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void ssin(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void ssin_gradient(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void scos(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void scos_gradient(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float _snrm1(int n, [In] float[] x, int offx, int incx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float _snrm2(int n, [In] float[] x, int offx, int incx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int sum_u8(int n, [In] byte[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int sum_s32(int n, [In] int[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint sum_u32(int n, [In] uint[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long sum_s64(int n, [In] long[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern ulong sum_u64(int n, [In] ulong[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float sum_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float svariance(int n, [In] float[] x, int offx);
        }
    }
}
