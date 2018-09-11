// -----------------------------------------------------------------------
// <copyright file="BitUtils64.cs" company="Noname, Inc.">
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
    /// Provides bit manipulation methods for 64-bit little-endian architecture.
    /// </summary>
    [CLSCompliant(false)]
    public static class BitUtils64
    {
        /// <summary>
        /// The first bit.
        /// </summary>
        public const ulong LSB = 1ul;

        /// <summary>
        /// Reverses the order of bytes in a 64-bit integer.
        /// </summary>
        /// <param name="bits">The integer to reverse byte order.</param>
        /// <returns>
        /// The integer with a reversed bytes.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BiteSwap(ulong bits)
        {
            return NativeMethods.byteswap_64(bits);
        }

        /// <summary>
        /// Reverses the order of bytes in an array of 64-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="xy">The integers to reverse byte order.</param>
        /// <param name="offxy">The index in the <paramref name="xy"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, ulong[] xy, int offxy)
        {
            NativeMethods.bytesswap_ip_64(length, xy, offxy);
        }

        /// <summary>
        /// Reverses the order of bytes in an array of 64-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="x">The integers to reverse byte order.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The integers that receive swapped bytes.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bytesswap_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Reverses the order of groups of bits in each byte an array of 64-bit integers in-place.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="xy">The integers to reverse bit order.</param>
        /// <param name="offxy">The index in the <paramref name="xy"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, ulong[] xy, int offxy)
        {
            NativeMethods.bits_reverse_ip_64(length, bitCount, xy, offxy);
        }

        /// <summary>
        /// Reverses the order of groups of bits in each byte an array of 64-bit integers not-in-place.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bits_reverse_64(length, bitCount, x, offx, y, offy);
        }

        /// <summary>
        /// Reverses the order of groups of bits in each byte an array of 64-bit integers and places result into array of bytes.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="bitCount">The number of bits in a group (1, 2, or 4).</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which swapping begins.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitSwap(int length, int bitCount, ulong[] x, int offx, byte[] y, int offy)
        {
            NativeMethods.bits_reverse_64_u64u8(length, bitCount, x, offx, y, offy * sizeof(ulong));
        }

        /// <summary>
        /// Performs logical NOT operation on single 64-bits array element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="xy">The source array.</param>
        /// <param name="offxy">The starting element position in <paramref name="xy"/>.</param>
        public static void WordsNOT(int length, ulong[] xy, int offxy)
        {
            NativeMethods.bits_not1_64(length, xy, offxy);
        }

        /// <summary>
        /// Performs logical NOT operation on two 64-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <paramref name="y"/>.</param>
        public static void WordsNOT(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bits_not2_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation between 64-bits array and a scalar value with increment in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="mask">The mask to apply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <paramref name="y"/>.</param>
        /// <param name="incy">The increment for the elements of <paramref name="y"/>.</param>
        public static void WordsAnd(int length, ulong mask, ulong[] y, int offy, int incy)
        {
            NativeMethods.bits_and_mask_inc_64(length, mask, y, offy, incy);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern int byteswap_64(ulong bits);

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_ip_64(int n, [In, Out] ulong[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bytesswap_64(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_copy_64(int count, [In] ulong[] bitssrc, int possrc, [Out] ulong[] bitsdst, int posdst);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_64(int length, int bitCount, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "bits_reverse_64")]
            public static extern void bits_reverse_64_u64u8(int length, int bitCount, [In] ulong[] x, int offx, [Out] byte[] y, int offy64);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_reverse_ip_64(int length, int bitCount, [In, Out] ulong[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not1_64(int length, [In, Out] ulong[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_not2_64(int length, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern void bits_and_mask_inc_64(int length, [In] ulong mask, [Out] ulong[] y, int offy, int incy);
        }
    }
}
