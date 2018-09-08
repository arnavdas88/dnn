﻿// -----------------------------------------------------------------------
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
        /// Rounds the value up to a next specified multiple.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="multiple">The multiple.</param>
        /// <returns>The rounded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundUp(int value, int multiple)
        {
            return (value + multiple - 1) / multiple * multiple;
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.ssub_inc(length, y, offy, incy, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays of floats with increment not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
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
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="masky">The second array that contains the data to compare.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := y(i) + (xmask(offx + i) == ymask(offy + i) ? x(offx + i) : 0)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MatchAndAdd(int length, float[] x, float[] maskx, int offx, float[] y, float[] masky, int offy)
        {
            NativeMethods.smatchandadd(length, x, maskx, offx, y, masky, offy);
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
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
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
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
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
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
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
        /// Computes a square root of sum of two squared elements of two array starting at the specified indexes
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting index in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting index in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting index in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Hypotenuse(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.hypot_f32(length, a, offa, b, offb, y, offy);
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
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pow(int length, float[] x, int offx, float power, float[] y, int offy)
        {
            NativeMethods.powx_f32(length, x, offx, power, y, offy);
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
            NativeMethods.powx_gradient_f32(length, x, dx, offx, cleardx, power, dy, offdy);
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
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.log_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes an exponential element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exp(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.exp_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the angle whose tangent is the quotient of two specified numbers element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The array that contains the y coordinate of points.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which computation begins.</param>
        /// <param name="b">The array that contains the x coordinate of points.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Atan2(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.atan2_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
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
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
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
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
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
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
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

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            /*[DllImport(NativeMethods.DllName)]
             public static extern void sinv(int n, [In] float[] a, int offa, [Out] float[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            public static extern void smatchandadd(int n, [In] float[] x, [In] float[] xmask, int offx, [Out] float[] y, [In] float[] ymask, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void ssub_inc(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            /*[DllImport(NativeMethods.DllName)]
            public static extern void smulc_inc(int n, float a, [In, Out] float[] x, int offx, int incx);*/

            [DllImport(NativeMethods.DllName)]
            public static extern void sdiv(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void _saxpby(int n, float a, [In] float[] x, int offx, int incx, float b, [In, Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void hypot_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void powx_f32(int n, [In] float[] a, int offa, float b, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void powx_gradient_f32(
                int n,
                [In] float[] x,
                [In, Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                float power,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            public static extern float slogSumExp2(float a, float b);

            [DllImport(NativeMethods.DllName)]
            public static extern void log_f32(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void exp_f32(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void atan2_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern int sum_s32(int n, [In] int[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern uint sum_u32(int n, [In] uint[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern long sum_s64(int n, [In] long[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern ulong sum_u64(int n, [In] ulong[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float svariance(int n, [In] float[] x, int offx);
        }
    }
}
