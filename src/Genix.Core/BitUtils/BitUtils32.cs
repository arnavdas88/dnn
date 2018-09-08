// -----------------------------------------------------------------------
// <copyright file="BitUtils32.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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
        /// The first bit.
        /// </summary>
        public const uint LSB = 1;

        /// <summary>
        /// Reverses the order of bytes in a 32-bit integer.
        /// </summary>
        /// <param name="bits">The integer to reverse byte order.</param>
        /// <returns>
        /// The integer with a reversed bytes.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BiteSwap(uint bits)
        {
            return NativeMethods.byteswap_32(bits);
        }

        /// <summary>
        /// Reverses the order of bytes in an array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="xy">The integers to reverse byte order.</param>
        /// <param name="offxy">The index in the <paramref name="xy"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, uint[] xy, int offxy)
        {
            NativeMethods.bytesswap_ip_32(length, xy, offxy);
        }

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
        /// Reverses the order of bytes in an array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="x">The integers to reverse byte order.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The integers that receive swapped bytes.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bytesswap_32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Reverses the order of groups of bits in each byte an array of 32-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="xy">The integers to reverse bit order.</param>
        /// <param name="offxy">The index in the <paramref name="xy"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, uint[] xy, int offxy)
        {
            NativeMethods.bits_reverse_ip_32(length, bitCount, xy, offxy);
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
        /// Reverses the order of groups of bits in each byte an array of 32-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bits_reverse_32(length, bitCount, x, offx, y, offy);
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
        public static void WordsAND(int length, uint mask, uint[] y, int offy)
        {
            NativeMethods.bits_and_mask_32(length, mask, y, offy);
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
            public static extern int byteswap_32(uint bits);

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_ip_32(int n, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_ip_32(int n, IntPtr xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_32(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_copy_32(int count, [In] uint[] bitssrc, int possrc, [Out] uint[] bitsdst, int posdst);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_32(int length, int bitCount, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_ip_32(int length, int bitCount, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_ip_32(int length, int bitCount, IntPtr xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not1_32(int length, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not2_32(int length, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_and_mask_32(int length, [In] uint mask, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_and_mask_inc_32(int length, [In] uint mask, [Out] uint[] y, int offy, int incy);
        }
    }
}
