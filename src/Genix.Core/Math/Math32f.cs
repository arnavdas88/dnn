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
        /// <param name="dx">The array that contains calculated gradient for <paramref name="x"/>.</param>
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

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void abs_ip_f32(int n, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void abs_f32(int n, [In] float[] a, int offa, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void addc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void add_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void subc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sub_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mulc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mul_ip_f32(int n, [In] float[] x, int offx, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mul_f32(int n, [In] float[] a, int offa, [In] float[] b, int offb, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void divc_ip_f32(int n, float a, [In, Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void divc_f32(int n, [In] float[] x, int offx, float a, [Out] float[] y, int offy);
        }
    }
}
