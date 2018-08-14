// -----------------------------------------------------------------------
// <copyright file="Maximum.cs" company="Noname, Inc.">
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
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b) => a >= b ? a : b;

        /// <summary>
        /// Returns the larger of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of three 32-bit signed integers to compare.</param>
        /// <param name="b">The second of three 32-bit signed integers to compare.</param>
        /// <param name="c">The third of three 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is larger.
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
        /// <param name="a">The first of four 32-bit signed integers to compare.</param>
        /// <param name="b">The second of four 32-bit signed integers to compare.</param>
        /// <param name="c">The third of four 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of four 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is larger.
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
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b) => a >= b ? a : b;

        /// <summary>
        /// Returns the larger of three single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of three single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of three single-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of three single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b, float c)
        {
            float ab = Maximum.Max(a, b);
            return Maximum.Max(ab, c);
        }

        /// <summary>
        /// Returns the larger of four single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of four single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of four single-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of four single-precision floating-point numbers to compare.</param>
        /// <param name="d">The forth of four single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is larger.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b, float c, float d)
        {
            float ab = Maximum.Max(a, b);
            float cd = Maximum.Max(c, d);
            return Maximum.Max(ab, cd);
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
            double ab = Maximum.Max(a, b);
            return Maximum.Max(ab, c);
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
            double ab = Maximum.Max(a, b);
            double cd = Maximum.Max(c, d);
            return Maximum.Max(ab, cd);
        }

        /// <summary>
        /// Returns the smaller of two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to compare.</param>
        /// <param name="b">The second of two 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b) => a <= b ? a : b;

        /// <summary>
        /// Returns the smaller of three 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of three 32-bit signed integers to compare.</param>
        /// <param name="b">The second of three 32-bit signed integers to compare.</param>
        /// <param name="c">The third of three 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is smaller.
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
        /// <param name="a">The first of four 32-bit signed integers to compare.</param>
        /// <param name="b">The second of four 32-bit signed integers to compare.</param>
        /// <param name="c">The third of four 32-bit signed integers to compare.</param>
        /// <param name="d">The forth of four 32-bit signed integers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is smaller.
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
        /// Parameter <paramref name="a"/> or <paramref name="b"/>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b) => a <= b ? a : b;

        /// <summary>
        /// Returns the smaller of three single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of three single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of three single-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of three single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b, float c)
        {
            float ab = Maximum.Min(a, b);
            return Maximum.Min(ab, c);
        }

        /// <summary>
        /// Returns the smaller of four single-precision floating-point numbers.
        /// </summary>
        /// <param name="a">The first of four single-precision floating-point numbers to compare.</param>
        /// <param name="b">The second of four single-precision floating-point numbers to compare.</param>
        /// <param name="c">The third of four single-precision floating-point numbers to compare.</param>
        /// <param name="d">The forth of four single-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter <paramref name="a"/> or <paramref name="b"/> or <c>c</c> or <c>d</c>, whichever is smaller.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b, float c, float d)
        {
            float ab = Maximum.Min(a, b);
            float cd = Maximum.Min(c, d);
            return Maximum.Min(ab, cd);
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
            double ab = Maximum.Min(a, b);
            return Maximum.Min(ab, c);
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
            double ab = Maximum.Min(a, b);
            double cd = Maximum.Min(c, d);
            return Maximum.Min(ab, cd);
        }

        /// <summary>
        /// Calculates a smaller of each pair of elements of the two array arguments.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which calculation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which calculation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.smin(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Calculates a larger of each pair of elements of the two array arguments.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which calculation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which calculation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.smax(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the arguments of the <see cref="Maximum.Min(int, float[], int, float[], int, float[], int)"/> and <see cref="Maximum.Max(int, float[], int, float[], int, float[], int)"/> methods.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">One of the <see cref="Maximum.Min(int, float[], int, float[], int, float[], int)"/> or
        /// <see cref="Maximum.Max(int, float[], int, float[], int, float[], int)"/> methods input arrays <c>a</c> or <c>b</c>.</param>
        /// <param name="dx">The array that contains calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <paramref name="dx"/> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <paramref name="dx"/> should be cleared before operation.</param>
        /// <param name="y">The <see cref="Maximum.Min(int, float[], int, float[], int, float[], int)"/> or
        /// <see cref="Maximum.Max(int, float[], int, float[], int, float[], int)"/> methods output array <paramref name="y"/>.</param>
        /// <param name="dy">The array that contains gradient for <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> and <paramref name="dy"/> at which calculation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offx + i) += x(offx + i) == y(offy + i) ? dy(offy + i) : 0</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMaxGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] y, float[] dy, int offy)
        {
            NativeMethods.sminmax_gradient(length, x, dx, offx, cleardx, y, dy, offy);
        }

        /// <summary>
        /// Returns the minimum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The minimum value in the array.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(int length, float[] x, int offx) => x[Maximum.ArgMin(length, x, offx)];

        /// <summary>
        /// Returns the maximum value in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The maximum value in the array.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(int length, float[] x, int offx) => x[Maximum.ArgMax(length, x, offx)];

        /// <summary>
        /// Returns the minimum and maximum values in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
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
        /// Returns the position of minimum value in the array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The position of minimum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMin(int length, int[] x, int offx) => NativeMethods.i32argmin(length, x, offx);

        /// <summary>
        /// Returns the position of maximum value in the array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The position of maximum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMax(int length, int[] x, int offx) => NativeMethods.i32argmax(length, x, offx);

        /// <summary>
        /// Returns the position of minimum value in the array of floats.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The position of minimum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMin(int length, float[] x, int offx) => NativeMethods.sargmin(length, x, offx);

        /// <summary>
        /// Returns the position of maximum value in the array of floats.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <returns>
        /// The position of maximum value in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArgMax(int length, float[] x, int offx) => NativeMethods.sargmax(length, x, offx);

        /// <summary>
        /// Returns the position of minimum and maximum values in the array.
        /// </summary>
        /// <param name="length">The number of elements to evaluate.</param>
        /// <param name="x">The array that contains data used for evaluation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which evaluation begins.</param>
        /// <param name="min">The position of minimum value in the array.</param>
        /// <param name="max">The position of maximum value in the array.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two parameters.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgMinMax(int length, float[] x, int offx, out int min, out int max)
        {
            NativeMethods.sargminmax(length, x, offx, out min, out max);
        }

        /// <summary>
        /// Calculates softmax probabilities for values in one array and stores calculated values in another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which computation begins.</param>
        /// <param name="y">The array that receives calculated probabilities. Can be <b>null</b>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which computation begins.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SoftMax(int length, float[] x, int offx, float[] y, int offy)
        {
            // compute max activation
            float amax = Maximum.Max(length, x, offx);

            // compute exponentials (carefully to not blow up)
            float esum = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float e = (float)Math.Exp(x[offx + i] - amax);
                esum += e;
                y[offy + i] = e;
            }

            // normalize and output to sum to one
            if (esum != 0.0f)
            {
                Math32f.DivC(length, esum, y, offy);
            }
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smin(int n, [In] float[] a, int offa, [In, Out] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void smax(int n, [In] float[] a, int offa, [In, Out] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sminmax_gradient(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                [In] float[] dy,
                int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int i32argmin(int n, [In] int[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int i32argmax(int n, [In] int[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int sargmin(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int sargmax(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sargminmax(int n, [In] float[] x, int offx, out int winmin, out int winmax);
        }
    }
}
