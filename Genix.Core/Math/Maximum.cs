// -----------------------------------------------------------------------
// <copyright file="Maximum.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides methods to calculate minimums and maximums.
    /// </summary>
    public static class Maximum
    {
        /// <summary>
        /// Returns the larger of two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b)
        {
            return a >= b ? a : b;
        }

        /// <summary>
        /// Returns the larger of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b, int c)
        {
            int ab = Maximum.Max(a, b);
            return Maximum.Max(ab, c);
        }

        /// <summary>
        /// Returns the larger of four 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c> or <c>d</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b, int c, int d)
        {
            int ab = Maximum.Max(a, b);
            int cd = Maximum.Max(c, d);
            return Maximum.Max(ab, cd);
        }

        /// <summary>
        /// Returns the larger of two single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b)
        {
            return a >= b ? a : b;
        }

        /// <summary>
        /// Returns the smaller of two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b)
        {
            return a <= b ? a : b;
        }

        /// <summary>
        /// Returns the smaller of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b, int c)
        {
            int ab = Maximum.Min(a, b);
            return Maximum.Min(ab, c);
        }

        /// <summary>
        /// Returns the smaller of four 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <param name="c">The third of two 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c> or <c>c</c> or <c>d</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b, int c, int d)
        {
            int ab = Maximum.Min(a, b);
            int cd = Maximum.Min(c, d);
            return Maximum.Min(ab, cd);
        }

        /// <summary>
        /// Returns the smaller of two single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of two single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of two single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <c>a</c> or <c>b</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b)
        {
            return a <= b ? a : b;
        }

        /// <summary>
        /// Calculates a smaller of each pair of elements of the two array arguments.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which calculation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.min(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Calculates a larger of each pair of elements of the two array arguments.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which calculation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.max(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the arguments of the <see cref="MKL.Min"/> and <see cref="MKL.Max"/> methods.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">One of the <see cref="MKL.Min"/> of <see cref="MKL.Max"/> methods input arrays <c>a</c> or <c>b</c>.</param>
        /// <param name="dx">The array that contains calculated gradient for <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> and <c>dx</c> at which computation begins.</param>
        /// <param name="y">The <see cref="MKL.Min"/> of <see cref="MKL.Max"/> methods output array <c>y</c>.</param>
        /// <param name="dy">The array that contains gradient for <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> and <c>dy</c> at which calculation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offx + i) += x(offx + i) == y(offy + i) ? dy(offy + i) : 0</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMaxGradient(int length, float[] x, float[] dx, int offx, float[] y, float[] dy, int offy)
        {
            NativeMethods.minmax_gradient(length, x, dx, offx, y, dy, offy);
        }

        /// <summary>
        /// Returns the minimum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <returns>
        /// The minimum value in the array.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(int length, float[] x, int offx)
        {
            return x[Maximum.ArgMin(length, x, offx)];
        }

        /// <summary>
        /// Returns the maximum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <returns>
        /// The maximum value in the array.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(int length, float[] x, int offx)
        {
            return x[Maximum.ArgMax(length, x, offx)];
        }

        /// <summary>
        /// Returns the minimum and maximum values in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <param name="min">The minimum value in the array.</param>
        /// <param name="max">The maximum value in the array.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two parameters.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMax(int length, float[] x, int offx, out float min, out float max)
        {
            Maximum.ArgMinMax(length, x, offx, out int argmin, out int argmax);
            min = x[argmin];
            max = x[argmax];
        }

        /// <summary>
        /// Returns the position of minimum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <returns>
        /// The position of minimum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMin(int length, float[] x, int offx)
        {
            return NativeMethods.argmin(length, x, offx);
        }

        /// <summary>
        /// Returns the position of maximum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <returns>
        /// The position of maximum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMax(int length, float[] x, int offx)
        {
            return NativeMethods.argmax(length, x, offx);
        }

        /// <summary>
        /// Returns the position of minimum and maximum values in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <c>x</c> at which evaluation begins.</param>
        /// <param name="min">The position of minimum value in the array.</param>
        /// <param name="max">The position of maximum value in the array.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two parameters.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgMinMax(int length, float[] x, int offx, out int min, out int max)
        {
            NativeMethods.argminmax(length, x, offx, out min, out max);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void min(int n, [In] float[] a, int offa, [In, Out] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void max(int n, [In] float[] a, int offa, [In, Out] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void minmax_gradient(int n, [In] float[] x, [Out] float[] dx, int offx, [In] float[] y, [In] float[] dy, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int argmin(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int argmax(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void argminmax(int n, [In] float[] x, int offx, out int winmin, out int winmax);
        }
    }
}
