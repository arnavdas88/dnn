// -----------------------------------------------------------------------
// <copyright file="MKL.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <summary>
    /// Provides math methods.
    /// </summary>
    public static class MKL
    {
        /// <summary>
        /// Swaps two 32-bit signed integers.
        /// </summary>
        /// <param name="a">The first of two 32-bit signed integers to swap.</param>
        /// <param name="b">The second of two 32-bit signed integers to swap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Need both parameters as references for swapping.")]
        public static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// Computes a hyperbolic tangent of the specified value.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The hyperbolic tangent of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tanh(float value)
        {
            return (float)Math.Tanh(value);
        }

        /// <summary>
        /// Computes a derivative of a hyperbolic tangent of the specified value.
        /// The method takes the result of <see cref="MKL.Tanh(Single)"/> method as an argument.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The derivative of a hyperbolic tangent of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float TanhDerivative2(float value)
        {
            return 1.0f - (value * value);
        }

        /// <summary>
        /// Computes a sigmoid nonlinearity of the specified angle. S(x) = 1 / (1 + e^-x).
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The sigmoid of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sigmoid(float value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }

        /// <summary>
        /// Computes a derivative of a sigmoid nonlinearity of the specified angle.
        /// The method takes the result of <see cref="MKL.Sigmoid(Single)"/> method as an argument.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The derivative of a sigmoid of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SigmoidDerivative2(float value)
        {
            return value * (1.0f - value);
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
            return NativeMethods.logSumExp2(a, b);
        }

        /// <summary>
        /// Determines whether the two array contain same data.
        /// </summary>
        /// <param name="length">The number of elements to check.</param>
        /// <param name="x">The first array to compare.</param>
        /// <param name="offx">The index in the <c>x</c> at which comparing begins.</param>
        /// <param name="y">The second array to compare.</param>
        /// <param name="offy">The index in the <c>y</c> at which comparing begins.</param>
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
        /// <param name="offx">The index in the <c>x</c> at which comparing begins.</param>
        /// <param name="y">The second array to compare.</param>
        /// <param name="offy">The index in the <c>y</c> at which comparing begins.</param>
        /// <returns>
        /// <b>true</b> if two arrays contain same data; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(int length, int[] x, int offx, int[] y, int offy)
        {
            return NativeMethods.icompare(length, x, offx, y, offy) == 0;
        }

        /// <summary>
        /// Determines whether the two arrays contain same data.
        /// </summary>
        /// <param name="length">The number of elements to check.</param>
        /// <param name="x">The first array to compare.</param>
        /// <param name="offx">The index in the <c>x</c> at which comparing begins.</param>
        /// <param name="y">The second array to compare.</param>
        /// <param name="offy">The index in the <c>y</c> at which comparing begins.</param>
        /// <returns>
        /// <b>true</b> if two arrays contain same data; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(int length, char[] x, int offx, char[] y, int offy)
        {
            return NativeMethods.ccompare(length, x, offx, y, offy) == 0;
        }

        /// <summary>
        /// Clips array values to a specified minimum and maximum values.
        /// </summary>
        /// <param name="length">The number of elements to clip.</param>
        /// <param name="minValue">The minimum value to clip by.</param>
        /// <param name="maxValue">The maximum value to clip by.</param>
        /// <param name="x">The array that contains the data to clip.</param>
        /// <param name="offx">The index in the <c>x</c> at which clipping begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>x(offx + i) := min(max(x(offx + i), minValue), maxValue)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clip(int length, float minValue, float maxValue, float[] x, int offx)
        {
            if (!float.IsNaN(minValue))
            {
                for (int i = offx, ii = offx + length; i < ii; i++)
                {
                    x[i] = x[i] > minValue ? x[i] : minValue;
                }
            }

            if (!float.IsNaN(maxValue))
            {
                for (int i = offx, ii = offx + length; i < ii; i++)
                {
                    x[i] = x[i] < maxValue ? x[i] : maxValue;
                }
            }
        }

        /// <summary>
        /// Copies a range of values from a array starting at the specified source index
        /// to another array starting at the specified destination index
        /// specified number of times.
        /// </summary>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="count">The number of times to copy <c>x</c>.</param>
        /// <param name="x">The array that contains the data to copy.</param>
        /// <param name="offx">The index in the <c>x</c> at which copying begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which copying begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tile(int length, int count, float[] x, int offx, float[] y, int offy)
        {
            for (int i = 0; i < count; i++, offy += length)
            {
                SetCopy.Copy(length, x, offx, y, offy);
            }
        }

        /// <summary>
        /// Replaces all occurrences of the specified value in the array with another specified value.
        /// </summary>
        /// <param name="length">The number of elements to replace.</param>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The value to replace all occurrences of <c>oldValue</c>.</param>
        /// <param name="y">The array that contains the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which replacement begins.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace(int length, float oldValue, float newValue, float[] y, int offy)
        {
            if (float.IsNaN(oldValue))
            {
                for (int i = offy, ii = offy + length; i < ii; i++)
                {
                    if (float.IsNaN(y[i]))
                    {
                        y[i] = newValue;
                    }
                }
            }
            else if (float.IsNegativeInfinity(oldValue))
            {
                for (int i = offy, ii = offy + length; i < ii; i++)
                {
                    if (float.IsNegativeInfinity(y[i]))
                    {
                        y[i] = newValue;
                    }
                }
            }
            else if (float.IsPositiveInfinity(oldValue))
            {
                for (int i = offy, ii = offy + length; i < ii; i++)
                {
                    if (float.IsPositiveInfinity(y[i]))
                    {
                        y[i] = newValue;
                    }
                }
            }
            else
            {
                for (int i = offy, ii = offy + length; i < ii; i++)
                {
                    if (y[i] == oldValue)
                    {
                        y[i] = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// Computes absolute value of a range of values from one array starting at the specified source index
        /// and stores results in another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to compute.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_sabs(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the argument of the <see cref="Abs"/> method.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The <see cref="Abs"/> method input array <c>x</c>.</param>
        /// <param name="dx">The array that contains calculated gradient for <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> and <c>dx</c> at which computation begins.</param>
        /// <param name="y">The <see cref="Abs"/> method output array <c>y</c>.</param>
        /// <param name="dy">The array that contains gradient for <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> and <c>dy</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offx + i) += x(offx + i) == y(offy + i) ? dy(offy + i) : -dy(offy + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AbsDerivative(int length, float[] x, float[] dx, int offx, float[] y, float[] dy, int offy)
        {
            NativeMethods.abs_derivative(length, x, dx, offx, y, dy, offy);
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which adding begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which adding begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_sadd(length, y, offy, x, offx, y, offy);
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which adding begins.</param>
        /// <param name="incx">The increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which adding begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_sadd_inc(length, y, offy, incy, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified index
        /// to another array starting at the specified index
        /// and stores results in third array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains the data to add.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data to add.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which adding begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mkl_sadd(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified index
        /// to another array starting at the specified index
        /// and stores results in third array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first array that contains the data to add.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="inca">The increment for the elements of <c>a</c>.</param>
        /// <param name="b">The second array that contains the data to add.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="incb">The increment for the elements of <c>b</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which adding begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_sadd_inc(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Adds a scalar value to all values of one array starting at the specified source index
        /// and stores results in another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The first array that contains the data to subtract from.</param>
        /// <param name="offx">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.addxc(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds a scalar value to all values of array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.addc(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a range of values from one array starting at the specified source index
        /// from another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains the data to subtract.</param>
        /// <param name="offx">The index in the <c>x</c> at which subtraction begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_ssub(length, y, offy, 1, x, offx, 1, y, offy, 1);
        }

        /// <summary>
        /// Subtracts a range of values from one array starting at the specified source index 
        /// from another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The array that contains the data to subtract.</param>
        /// <param name="offx">The index in the <c>x</c> at which subtraction begins.</param>
        /// <param name="incx">The increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_ssub(length, y, offy, incy, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Subtracts a range of values from one array starting at the specified index
        /// from another array starting at the specified index
        /// and stores results in third array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains the data to subtract.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data to subtract.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mkl_ssub(length, a, offa, 1, b, offb, 1, y, offy, 1);
        }

        /// <summary>
        /// Subtracts a range of values from one array starting at the specified index
        /// from another array starting at the specified index
        /// and stores results in third array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first array that contains the data to subtract.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="inca">The increment for the elements of <c>a</c>.</param>
        /// <param name="b">The second array that contains the data to subtract.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="incb">The increment for the elements of <c>b</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_ssub(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Subtracts a scalar value to all values of one array starting at the specified source index
        /// and stores results in another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The first array that contains the data to subtract from.</param>
        /// <param name="offx">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which subtraction begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.addxc(length, x, offx, -alpha, y, offy);
        }

        /// <summary>
        /// Multiplies elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <c>a</c>.</param>
        /// <param name="offa">The index in the <c>a</c> at which computation begins.</param>
        /// <param name="b">The input array <c>b</c>.</param>
        /// <param name="offb">The index in the <c>b</c> at which computation begins.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := a(offa + i) * b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mkl_smul(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Multiplies all elements of one array by a scalar and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to multiply.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(int length, float alpha, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mulc(length, x, offx, 1, alpha, y, offy, 1);
        }

        /// <summary>
        /// Multiplies all elements of one array by a scalar and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to multiply.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">The increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(int length, float alpha, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.mulc(length, x, offx, incx, alpha, y, offy, incy);
        }

        /// <summary>
        /// Divides elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <c>a</c>.</param>
        /// <param name="offa">The index in the <c>a</c> at which computation begins.</param>
        /// <param name="b">The input array <c>b</c>.</param>
        /// <param name="offb">The index in the <c>b</c> at which computation begins.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := a(offa + i) / b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mkl_sdiv(length, a, offa, 1, b, offb, 1, y, offy, 1);
        }

        /// <summary>
        /// Divides elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <c>a</c>.</param>
        /// <param name="offa">The index in the <c>a</c> at which computation begins.</param>
        /// <param name="inca">the increment for the elements of <c>a</c>.</param>
        /// <param name="b">The input array <c>b</c>.</param>
        /// <param name="offb">The index in the <c>b</c> at which computation begins.</param>
        /// <param name="incb">the increment for the elements of <c>b</c>.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <c>y</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i * incy) := a(offa + i * inca) / b(offb + i * incb)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_sdiv(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

        /// <summary>
        /// Divides all elements of one array by a scalar and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to divide.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := x / alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float alpha, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mulc(length, x, offx, 1, 1.0f / alpha, y, offy, 1);
        }

        /// <summary>
        /// Divides all elements of one array by a scalar and puts results into destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to divide.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">The increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">The increment for the elements of <c>y</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := x / alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(int length, float alpha, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.mulc(length, x, offx, incx, 1.0f / alpha, y, offy, incy);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_saxpy(length, alpha, x, offx, 1, y, offy, 1);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + beta * y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, float beta, float[] y, int offy)
        {
            NativeMethods.mkl_saxpby(length, alpha, x, offx, 1, beta, y, offy, 1);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <c>x</c>.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <c>y</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + beta * y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, int incx, float beta, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_saxpby(length, alpha, x, offx, incx, beta, y, offy, incy);
        }

        /// <summary>
        /// Adds a range of values multiplied by a specified factor from a array starting at the specified source index
        /// to another array starting at the specified destination index.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="alpha">The scalar <c>alpha</c>.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <c>x</c>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="incy">the increment for the elements of <c>y</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y := alpha * x + y</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float alpha, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.mkl_saxpy(length, alpha, x, offx, incx, y, offy, incy);
        }

        /// <summary>
        /// Multiplies elements of one array starting at the specified index
        /// to elements of another array starting at the specified index
        /// and adds results to the destination array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The input array <c>a</c>.</param>
        /// <param name="offa">The index in the <c>a</c> at which computation begins.</param>
        /// <param name="b">The input array <c>b</c>.</param>
        /// <param name="offb">The index in the <c>b</c> at which computation begins.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) += a(offa + i) * b(offb + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyAndAdd(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mkl_smuladd(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Calculates a dot product between values from one array starting at the specified index
        /// and values from another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <returns>
        /// The calculated dot product value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(int length, float[] a, int offa, float[] b, int offb)
        {
            return NativeMethods.mkl_sdot(length, a, offa, 1, b, offb, 1);
        }

        /// <summary>
        /// Calculates a dot product between values from one array starting at the specified index
        /// and values from another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <c>a</c> at which calculation begins.</param>
        /// <param name="inca">the increment for the elements of <c>a</c>.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <c>b</c> at which calculation begins.</param>
        /// <param name="incb">the increment for the elements of <c>b</c>.</param>
        /// <returns>
        /// The calculated dot product value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(int length, float[] a, int offa, int inca, float[] b, int offb, int incb)
        {
            return NativeMethods.mkl_sdot(length, a, offa, inca, b, offb, incb);
        }

        /// <summary>
        /// Computes a sines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sin(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_ssin(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a gradient of a sines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="x">The array that contains <see cref="MKL.Sin"/> method input.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinGradient(int length, float[] dx, int offdx, bool cleardx, float[] x, int offx, float[] dy, int offdy)
        {
            NativeMethods.mkl_ssin_grad(length, dx, offdx, cleardx, x, offx, dy, offdy);
        }

        /// <summary>
        /// Computes a cosines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cos(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_scos(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a gradient of a cosines element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="x">The array that contains <see cref="MKL.Cos"/> method input.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CosGradient(int length, float[] dx, int offdx, bool cleardx, float[] x, int offx, float[] dy, int offdy)
        {
            NativeMethods.mkl_scos_grad(length, dx, offdx, cleardx, x, offx, dy, offdy);
        }

        /// <summary>
        /// Squares elements from one array starting at the specified index
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Square(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_ssqr(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a square root of elements of one array starting at the specified index
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := sqrt(x(offx + i))</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sqrt(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_ssqrt(length, x, offx, y, offy);
        }

        /// <summary>
        /// Raises elements of one array starting at the specified index to the scalar power
        /// and puts results into another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="power">The constant value for power.</param>
        /// <param name="y">The output array <c>y</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := x(offx + i) * x(offx + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pow(int length, float[] x, int offx, float power, float[] y, int offy)
        {
            NativeMethods.mkl_spowx(length, x, offx, power, y, offy);
        }

        /// <summary>
        /// Computes the derivative of <see cref="Pow"/> method.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The input array <c>x</c>.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="power">The constant value for power.</param>
        /// <param name="dy">The chain gradient array <c>dy</c>.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        /// <param name="dx">The output array <c>dx</c>.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offdx + i) += p * x(offx + i) ^ (p-1) * dy(offdy + i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PowDerivative(int length, float[] x, int offx, float power, float[] dy, int offdy, float[] dx, int offdx)
        {
            NativeMethods.pow_derivative(length, x, offx, power, dy, offdy, dx, offdx);
        }

        /// <summary>
        /// Computes a natural logarithm element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_slog(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes an exponential element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exp(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mkl_sexp(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the L1-Norm (sum of magnitudes) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <c>x</c>.</param>
        /// <returns>
        /// The L1-Norm of array elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L1Norm(int length, float[] x, int offx, int incx)
        {
            return NativeMethods.mkl_snrm1(length, x, offx, incx);
        }

        /// <summary>
        /// Computes the L2-Norm (Euclidian norm) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="incx">the increment for the elements of <c>x</c>.</param>
        /// <returns>
        /// The L2-Norm of array elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L2Norm(int length, float[] x, int offx, int incx)
        {
            return NativeMethods.mkl_snrm2(length, x, offx, incx);
        }

        /// <summary>
        /// Calculates softmax probabilities for values in one array and stores calculated values in another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives calculated probabilities. Can be <b>null</b>.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
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
                MKL.Divide(length, esum, y, offy, y, offy);
            }
        }

        /// <summary>
        /// Adds a range of values from one array starting at the specified source index
        /// to another array starting at the specified destination index
        /// if values in mask arrays match.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains the data to add.</param>
        /// <param name="maskx">The first array that contains the data to compare.</param>
        /// <param name="offx">The index in the <c>x</c> at which adding begins.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="masky">The second array that contains the data to compare.</param>
        /// <param name="offy">The index in the <c>y</c> at which adding begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y(offy + i) := y(i) + (xmask(offx + i) == ymask(offy + i) ? x(offx + i) : 0)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MatchAndAdd(int length, float[] x, float[] maskx, int offx, float[] y, float[] masky, int offy)
        {
            NativeMethods.matchandadd(length, x, maskx, offx, y, masky, offy);
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an array.
        /// </summary>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="x">The array to sort.</param>
        /// <param name="offx">The index in the <c>x</c> at which sorting begins.</param>
        /// <param name="ascending"><b>true</b> to use ascending sorting order; <b>false</b> to use descending sorting order.</param>
        public static void Sort(int length, float[] x, int offx, bool ascending)
        {
            NativeMethods.qsortf(length, x, offx, ascending);
        }

        /// <summary>
        /// Sorts the elements in a range of elements in a pair of arrays
        /// (one contains the keys and the other contains the corresponding items)
        /// based on the keys in the first array.
        /// </summary>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="x">The array that contains the keys to sort.</param>
        /// <param name="offx">The index in the <c>x</c> at which sorting begins.</param>
        /// <param name="y">The array that contains the items that correspond to each of the keys in the <c>x</c>.</param>
        /// <param name="offy">The index in the <c>y</c> at which sorting begins.</param>
        /// <param name="ascending"><b>true</b> to use ascending sorting order; <b>false</b> to use descending sorting order.</param>
        public static void Sort(int length, float[] x, int offx, int[] y, int offy, bool ascending)
        {
            NativeMethods.qsortfv(length, x, offx, y, offy, ascending);
        }

        private static class NativeMethods
        {
            private const string DllName = "Accord.DNN.CPP.dll";

            /*[DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sinv(int n, [In] float[] a, int offa, [Out] float[] y, int offy);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sabs(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void abs_derivative(int n, [In] float[] x, [Out] float[] dx, int offx, [In] float[] y, [In] float[] dy, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sadd(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sadd_inc(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_ssub(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            /*[DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sscal(int n, float a, [In, Out] float[] x, int offx, int incx);*/

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_smul(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_smuladd(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sdiv(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_ssqr(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_ssqrt(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_spowx(int n, [In] float[] a, int offa, float b, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void pow_derivative(int n, [In] float[] x, int offx, float power, [In] float[] dy, int offdy, [In, Out] float[] dx, int offdx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float mkl_sdot(int n, [In] float[] x, int offx, int incx, [In] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_saxpy(int n, float a, [In] float[] x, int offx, int incx, [In, Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_saxpby(int n, float a, [In] float[] x, int offx, int incx, float b, [In, Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_ssin(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_ssin_grad(
                int n,
                [Out] float[] dx,
                int offdx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] x,
                int offx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_scos(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_scos_grad(
                int n,
                [Out] float[] dx,
                int offdx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] x,
                int offx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_slog(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_sexp(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float mkl_snrm1(int n, [In] float[] x, int offx, int incx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float mkl_snrm2(int n, [In] float[] x, int offx, int incx);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addxc(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc(int n, [In] float[] x, int offx, int incx, float a, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void matchandadd(int n, [In] float[] x, [In] float[] xmask, int offx, [Out] float[] y, [In] float[] ymask, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int fcompare(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int icompare(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int ccompare(int n, [In] char[] x, int offx, [Out] char[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void qsortf(
                int n,
                [In, Out] float[] x,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool ascending);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void qsortfv(
                int n,
                [In, Out] float[] x,
                int offx,
                [In, Out] int[] y,
                int offy,
                [MarshalAs(UnmanagedType.Bool)] bool ascending);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float logSumExp2(float a, float b);
        }
    }
}
