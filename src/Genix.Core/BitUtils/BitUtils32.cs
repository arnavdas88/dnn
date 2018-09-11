// -----------------------------------------------------------------------
// <copyright file="BitUtils32.cs" company="Noname, Inc.">
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
    /// Provides bit manipulation methods for 32-bit little-endian architecture.
    /// </summary>
    [CLSCompliant(false)]
    public static class BitUtils32
    {
        /// <summary>
        /// Reverses the order of bytes in an array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="xy">The pointer to source and destination arrays.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, IntPtr xy)
        {
            NativeMethods.bytesswap_ip_32(length, xy, 0);
        }

        /// <summary>
        /// Reverses the order of groups of bits in each byte an array of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="xy">The pointer to source and destination arrays.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, IntPtr xy)
        {
            NativeMethods.bits_reverse_ip_32(length, bitCount, xy, 0);
        }

        /// <summary>
        /// Performs logical NOT operation on single 32-bits array element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="xy">The source array.</param>
        /// <param name="offxy">The starting element position in <paramref name="xy"/>.</param>
        public static void WordsNOT(int length, uint[] xy, int offxy)
        {
            NativeMethods.bits_not1_32(length, xy, offxy);
        }

        /// <summary>
        /// Performs logical NOT operation on two 32-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <paramref name="y"/>.</param>
        public static void WordsNOT(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bits_not2_32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation between 32-bits array and a scalar value.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="mask">The mask to apply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        public static void WordsAND(int length, uint mask, uint[] y, int offy, int incy)
        {
            NativeMethods.bits_and_mask_inc_32(length, mask, y, offy, incy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_ip_32(int n, IntPtr xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_ip_32(int length, int bitCount, IntPtr xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not1_32(int length, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not2_32(int length, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_and_mask_inc_32(int length, [In] uint mask, [Out] uint[] y, int offy, int incy);
        }
    }
}
