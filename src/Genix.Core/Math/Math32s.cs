// -----------------------------------------------------------------------
// <copyright file="Math32s.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides mathematical methods for 32-bit signed integers.
    /// </summary>
    public static class Math32s
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
        public static void Abs(int length, int[] y, int offy)
        {
            NativeMethods.abs_ip_s32(length, y, offy);
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
        public static void AddC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.addc_ip_s32(length, alpha, y, offy);
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
        public static void AddC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.addc_s32(length, x, offx, alpha, y, offy);
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
        public static void Add(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.add_ip_s32(length, x, offx, y, offy);
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
        public static void Add(int length, int[] a, int offa, int[] b, int offb, int[] y, int offy)
        {
            NativeMethods.add_s32(length, a, offa, b, offb, y, offy);
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
        public static void SubC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.subc_ip_s32(length, alpha, y, offy);
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
        public static void SubC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.subc_s32(length, x, offx, alpha, y, offy);
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
        public static void Sub(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.sub_ip_s32(length, x, offx, y, offy);
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
        public static void Sub(int length, int[] a, int offa, int[] b, int offb, int[] y, int offy)
        {
            NativeMethods.sub_s32(length, a, offa, b, offb, y, offy);
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
        public static void MulC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.mulc_ip_s32(length, alpha, y, offy);
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
        public static void MulC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.mulc_s32(length, x, offx, alpha, y, offy);
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
        public static void Mul(int length, int[] x, int offx, int[] y, int offy)
        {
            NativeMethods.mul_ip_s32(length, x, offx, y, offy);
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
        public static void Mul(int length, int[] a, int offa, int[] b, int offb, int[] y, int offy)
        {
            NativeMethods.mul_s32(length, a, offa, b, offb, y, offy);
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
        public static void DivC(int length, int alpha, int[] y, int offy)
        {
            NativeMethods.divc_ip_s32(length, alpha, y, offy);
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
        public static void DivC(int length, int[] x, int offx, int alpha, int[] y, int offy)
        {
            NativeMethods.divc_s32(length, x, offx, alpha, y, offy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void abs_ip_s32(int n, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void addc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_ip_s32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void add_s32(int n, [In] int[] a, int offa, [In] int[] b, int offb, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void subc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void subc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sub_ip_s32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void sub_s32(int n, [In] int[] a, int offa, [In] int[] b, int offb, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mulc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mul_ip_s32(int n, [In] int[] x, int offx, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void mul_s32(int n, [In] int[] a, int offa, [In] int[] b, int offb, [Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_ip_s32(int n, int a, [In, Out] int[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void divc_s32(int n, [In] int[] x, int offx, int a, [Out] int[] y, int offy);
        }
    }
}
