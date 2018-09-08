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
        /// Computes the absolute value of elements of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = Math.Abs(y)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(int length, float[] y, int offy)
        {
            NativeMethods.abs_ip_f32(length, y, offy);
        }

        /// <summary>
        /// Computes the absolute value of elements of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The array that receives the data.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.abs_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the argument of the <see cref="Abs(int, float[], int, float[], int)"/> method.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The <see cref="Abs(int, float[], int, float[], int)"/> method argument <paramref name="x"/>.</param>
        /// <param name="dx">The destination array that receives calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/> and <paramref name="dx"/>.</param>
        /// <param name="cleardx">Specifies whether <paramref name="dx"/> should be cleared before computation starts.</param>
        /// <param name="y">The <see cref="Abs(int, float[], int, float[], int)"/> method argument <paramref name="y"/>.</param>
        /// <param name="dy">The array that contains gradient for <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/> and <paramref name="dy"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx[i] += x[i] == y[i] ? dy[i] : -dy[i]</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AbsGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] y, float[] dy, int offy)
        {
            NativeMethods.abs_gradient_f32(length, x, dx, offx, cleardx, y, dy, offy);
        }

#if false
        /// <summary>
        /// Adds a constant value to each element of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.addc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Adds a constant value to each element of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = x[i] + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.addc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds the elements of two arrays in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y += x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.add_ip_f32(length, x, offx, y, offy);
        }
#endif

        /// <summary>
        /// Adds the elements of two arrays with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] x, int offx, int incx, float[] y, int offy, int incy)
        {
            NativeMethods.add_inc_ip_f32(length, x, offx, incx, y, offy, incy);
        }

#if false
        /// <summary>
        /// Adds the elements of two arrays not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = a + b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.add_f32(length, a, offa, b, offb, y, offy);
        }
#endif

        /// <summary>
        /// Adds the elements of two arrays with increment not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
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
        public static void Add(int length, float[] a, int offa, int inca, float[] b, int offb, int incb, float[] y, int offy, int incy)
        {
            NativeMethods.add_inc_f32(length, a, offa, inca, b, offb, incb, y, offy, incy);
        }

#if false
        /// <summary>
        /// Subtracts a constant value from each element of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] -= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.subc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts a constant value from each element of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to subtract.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = x[i] - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.subc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y -= x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sub_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Subtracts the elements of two arrays not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to subtract.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = a - b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.sub_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] *= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.mulc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Multiplies each element of an array by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] := x[i] * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MulC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.mulc_f32(length, x, offx, alpha, y, offy);
        }
#endif

        /// <summary>
        /// Multiplies the elements of two arrays in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y *= x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Mul(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.mul_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Multiplies the elements of two arrays not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = a * b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Mul(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.mul_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Divides each element of an array by a constant value in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="alpha">The scalar to divide.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] /= alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DivC(int length, float alpha, float[] y, int offy)
        {
            NativeMethods.divc_ip_f32(length, alpha, y, offy);
        }

        /// <summary>
        /// Divides each element of an array by a constant value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to divide.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = x[i] / alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DivC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.divc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Divides the elements of two arrays in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y /= x</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Div(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.div_ip_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Divides the elements of two arrays not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to divide.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y = a / b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Div(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.div_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Adds product of element of an array and a constant to the elements of destination array.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] += x[i] * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddProductC(int length, float[] x, int offx, float alpha, float[] y, int offy)
        {
            NativeMethods.addproductc_f32(length, x, offx, alpha, y, offy);
        }

        /// <summary>
        /// Adds product of elements of two arrays to the elements of destination array.
        /// </summary>
        /// <param name="length">The number of elements to multiply.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y += a * b</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddProduct(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.addproduct_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Squares elements of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = y[i] * y[i]</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Square(int length, float[] y, int offy)
        {
            NativeMethods.sqr_ip_f32(length, y, offy);
        }

        /// <summary>
        /// Squares elements of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = x[i] * x[i]</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Square(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sqr_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a square root of elements of an array in-place.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = sqrt(y[i])</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sqrt(int length, float[] y, int offy)
        {
            NativeMethods.sqrt_ip_f32(length, y, offy);
        }

        /// <summary>
        /// Computes a square root of elements of an array not in-place.
        /// </summary>
        /// <param name="length">The number of elements to square.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] = sqrt(x[i])</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sqrt(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sqrt_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a sines of elements of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sin(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sin_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the argument of the <see cref="Sin"/> method.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The <see cref="Sin"/> method argument <paramref name="x"/>.</param>
        /// <param name="dx">The destination array that receives calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/> and <paramref name="dx"/>.</param>
        /// <param name="cleardx">Specifies whether <paramref name="dx"/> should be cleared before computation starts.</param>
        /// <param name="dy">The array that contains gradient <see cref="Sin"/> method argument <c>y</c>.</param>
        /// <param name="offdy">The starting position in <paramref name="dy"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] dy, int offdy)
        {
            NativeMethods.sin_gradient_f32(length, x, dx, offx, cleardx, dy, offdy);
        }

        /// <summary>
        /// Computes a cosines of elements of an array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cos(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.cos_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the derivative of the argument of the <see cref="Cos"/> method.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The <see cref="Cos"/> method argument <paramref name="x"/>.</param>
        /// <param name="dx">The destination array that receives calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/> and <paramref name="dx"/>.</param>
        /// <param name="cleardx">Specifies whether <paramref name="dx"/> should be cleared before computation starts.</param>
        /// <param name="dy">The array that contains gradient <see cref="Cos"/> method argument <c>y</c>.</param>
        /// <param name="offdy">The starting position in <paramref name="dy"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CosGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] dy, int offdy)
        {
            NativeMethods.cos_gradient_f32(length, x, dx, offx, cleardx, dy, offdy);
        }

        /// <summary>
                 /// Computes a smaller of each element of an array and a scalar value in-place.
                 /// </summary>
                 /// <param name="length">The number of elements to calculate.</param>
                 /// <param name="a">The scalar value.</param>
                 /// <param name="y">The source and destination array.</param>
                 /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinC(int length, float a, float[] y, int offy)
        {
            NativeMethods.minc_ip_f32(length, a, y, offy);
        }

        /// <summary>
        /// Computes a smaller of each element of an array and a scalar value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="a">The scalar value.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinC(int length, float[] x, int offx, float a, float[] y, int offy)
        {
            NativeMethods.minc_f32(length, x, offx, a, y, offy);
        }

        /// <summary>
        /// Computes a smaller of each pair of elements of the two array arguments not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.min_f32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Computes a larger of each element of an array and a scalar value in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The scalar value.</param>
        /// <param name="y">The source and destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MaxC(int length, float a, float[] y, int offy)
        {
            NativeMethods.maxc_ip_f32(length, a, y, offy);
        }

        /// <summary>
        /// Computes a larger of each element of an array and a scalar value not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="a">The scalar value.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MaxC(int length, float[] x, int offx, float a, float[] y, int offy)
        {
            NativeMethods.maxc_f32(length, x, offx, a, y, offy);
        }

        /// <summary>
        /// Computes a larger of each pair of elements of the two array arguments not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting position in <paramref name="a"/>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting position in <paramref name="b"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(int length, float[] a, int offa, float[] b, int offb, float[] y, int offy)
        {
            NativeMethods.max_f32(length, a, offa, b, offb, y, offy);
        }

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

        /// <summary>
        /// Computes the L1-Norm (sum of magnitudes) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The L1-Norm of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L1Norm(int length, float[] x, int offx)
        {
            return NativeMethods.nrm1_f32(length, x, offx);
        }

        /// <summary>
        /// Computes the L2-Norm (Euclidian norm) of the array elements.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The L2-Norm of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float L2Norm(int length, float[] x, int offx)
        {
            return NativeMethods.nrm2_f32(length, x, offx);
        }

        /// <summary>
        /// Computes the Manhattan distance between elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The first array <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The first array <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Manhattan distance between elements of two arrays.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as sum(abs(x[i] - y[i])).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ManhattanDistance(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.manhattan_distance_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the squared Euclidean distance between elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The first array <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The first array <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Euclidean distance between elements of two arrays.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as sum((x[i] - y[i])^2).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EuclideanDistanceSquared(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.euclidean_distance_squared_f32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes the Euclidean distance between elements of two arrays.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">The first array <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The first array <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Euclidean distance between elements of two arrays.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as sqrt(sum((x[i] - y[i])^2)).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EuclideanDistance(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.euclidean_distance_f32(length, x, offx, y, offy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void abs_ip_f32(int n, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void abs_f32(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void abs_gradient_f32(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                [In] float[] dy,
                int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_inc_ip_f32(int n, [In] float[] x, int offx, int incx, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_inc_f32(int n, [In] float[] a, int offa, int inca, [In] float[] b, int offb, int incb, [Out] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void subc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void subc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sub_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sub_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mul_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mul_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void div_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void div_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addproductc_f32(int n, [In] float[] x, int offx, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addproduct_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sqr_ip_f32(int n, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sqr_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sqrt_ip_f32(int n, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sqrt_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sin_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sin_gradient_f32(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            public static extern void cos_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void cos_gradient_f32(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            public static extern void minc_ip_f32(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void minc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void min_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void maxc_ip_f32(int n, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void maxc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void max_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float sum_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_ip_f32(int n, [In, Out] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float cumulative_sum_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float nrm1_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float nrm2_f32(int n, [In] float[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern float manhattan_distance_f32(int n, [In] float[] x, int offx, [In] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float euclidean_distance_squared_f32(int n, [In] float[] x, int offx, [In] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float euclidean_distance_f32(int n, [In] float[] x, int offx, [In] float[] y, int offy);
        }
    }
}
