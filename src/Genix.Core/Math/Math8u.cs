// -----------------------------------------------------------------------
// <copyright file="Math8u.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides mathematical methods for unsigned 8-bit integers.
    /// </summary>
    public static class Math8u
    {
        /// <summary>
        /// Computes the sum of all elements in the array of 8-bit unsigned integers.
        /// </summary>
        /// <param name="length">The number of elements to add.</param>
        /// <param name="x">The array that contains data.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The sum of elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(int length, byte[] x, int offx)
        {
            return NativeMethods.sum_u8(length, x, offx);
        }

        /// <summary>
        /// Computes a smaller of each pair of elements of the two array arguments not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The pointer to a first source array.</param>
        /// <param name="b">The pointer to a second source array.</param>
        /// <param name="y">The pointer to a destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(int length, IntPtr a, IntPtr b, IntPtr y)
        {
            NativeMethods.min_u8(length, a, 0, b, 0, y, 0);
        }

        /// <summary>
        /// Computes a larger of each pair of elements of the two array arguments not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The pointer to a first source array.</param>
        /// <param name="b">The pointer to a second source array.</param>
        /// <param name="y">The pointer to a destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(int length, IntPtr a, IntPtr b, IntPtr y)
        {
            NativeMethods.max_u8(length, a, 0, b, 0, y, 0);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern int sum_u8(int n, [In] byte[] x, int offx);

            [DllImport(NativeMethods.DllName)]
            public static extern void min_u8(int n, [In] IntPtr a, int offa, [In] IntPtr b, int offb, [Out] IntPtr y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void max_u8(int n, [In] IntPtr a, int offa, [In] IntPtr b, int offb, [Out] IntPtr y, int offy);
        }
    }
}
