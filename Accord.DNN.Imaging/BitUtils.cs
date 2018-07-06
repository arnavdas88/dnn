// -----------------------------------------------------------------------
// <copyright file="BitUtils.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides bit manipulation methods for 32-bit big-endian architecture.
    /// </summary>
    internal static class BitUtils
    {
        /// <summary>
        /// The first bit.
        /// </summary>
        private const uint LSB = 0x8000_0000;

        /// <summary>
        /// Examines the bit at the specified position, and returns the value of that bit.
        /// </summary>
        /// <param name="bits">The bits to examine.</param>
        /// <param name="position">The bit position to test.</param>
        /// <returns>
        /// The bit at the position specified.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TestBit(uint bits, int position)
        {
            Debug.Assert(position < 32, "The bit position must be less than 32.");
            return (bits & (LSB >> position)) != 0;
        }

        /// <summary>
        /// Gets the specified number of bits at the specified position.
        /// </summary>
        /// <param name="bits">The bits to examine.</param>
        /// <param name="position">The bits index; in the range of 0 - 15.</param>
        /// <param name="count">The number of bits to get.</param>
        /// <returns>
        /// The value at the index specified.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBits(uint bits, int position, int count)
        {
            Debug.Assert(position + count < 32, "The position+count must be less than 32.");
            return (bits >> (32 - (position + count))) & (0xffff_ffff >> (32 - count));
        }

        /// <summary>
        /// Sets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to set.</param>
        /// <param name="position">The bit position to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref uint bits, int position)
        {
            Debug.Assert(position < 32, "The bit position must be less than 32.");
            bits |= LSB >> position;
        }

        /// <summary>
        /// Sets the range bits at the specified position.
        /// </summary>
        /// <param name="count">The number of bits to set.</param>
        /// <param name="bits">The bits to set.</param>
        /// <param name="position">The starting bit position to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBits(int count, uint[] bits, int position)
        {
            NativeMethods.bits_set_be32(count, bits, position);
        }

        /// <summary>
        /// Resets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The bit position to reset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetBit(ref uint bits, int position)
        {
            Debug.Assert(position < 32, "The bit position must be less than 32.");
            bits &= ~(LSB >> position);
        }

        /// <summary>
        /// Resets the range bits at the specified position.
        /// </summary>
        /// <param name="count">The number of bits to reset.</param>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The starting bit position to reset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetBits(int count, uint[] bits, int position)
        {
            NativeMethods.bits_reset_be32(count, bits, position);
        }

        /// <summary>
        /// Copies the range bits from a source array at the specified position
        /// to the destination array at the specified position.
        /// </summary>
        /// <param name="count">The number of bits to copy.</param>
        /// <param name="bitsSrc">The bits to copy from.</param>
        /// <param name="positionSrc">The starting bit position in <c>bitssrc</c>.</param>
        /// <param name="bitsDst">The bits to copy to.</param>
        /// <param name="positionDst">The starting bit position in <c>bitsdst</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBits(int count, uint[] bitsSrc, int positionSrc, uint[] bitsDst, int positionDst)
        {
            NativeMethods.bits_copy_be32(count, bitsSrc, positionSrc, bitsDst, positionDst);
        }

        /// <summary>
        /// Searches the range of values for a first set bit (1).
        /// </summary>
        /// <param name="bits">The bits to search.</param>
        /// <returns>
        /// The bit position of the first set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneForward(uint bits)
        {
            return NativeMethods.bit_scan_forward_be32(bits);
        }

        /// <summary>
        /// Searches the range of values for a last set bit (1).
        /// </summary>
        /// <param name="bits">The bits to search.</param>
        /// <returns>
        /// The bit position of the last set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneReverse(uint bits)
        {
            return NativeMethods.bit_scan_reverse_be32(bits);
        }

        /// <summary>
        /// Searches the range of values for a first set bit (1).
        /// </summary>
        /// <param name="count">The number of bits to search.</param>
        /// <param name="bits">The array that contains the data to search.</param>
        /// <param name="position">The starting bit position in <c>bits</c>.</param>
        /// <returns>
        /// The bit position of the first set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneForward(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_scan_one_forward_be32(count, bits, position);
        }

        /// <summary>
        /// Searches the range of values for a last set bit (1).
        /// </summary>
        /// <param name="count">The number of bits to search.</param>
        /// <param name="bits">The array that contains the data to search.</param>
        /// <param name="position">The starting bit position in <c>bits</c>.</param>
        /// <returns>
        /// The bit position of the last set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneReverse(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_scan_one_reverse_be32(count, bits, position);
        }

        /// <summary>
        /// Searches the range of values for a first reset bit (0).
        /// </summary>
        /// <param name="count">The number of bits to search.</param>
        /// <param name="bits">The array that contains the data to search.</param>
        /// <param name="position">The starting bit position in <c>bits</c>.</param>
        /// <returns>
        /// The bit position of the first reset bit (0) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanZeroForward(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_scan_zero_forward_be32(count, bits, position);
        }

        /// <summary>
        /// Searches the range of values for a last reset bit (0).
        /// </summary>
        /// <param name="count">The number of values to search.</param>
        /// <param name="bits">The array that bits the data to search.</param>
        /// <param name="position">The starting bit position in <c>bits</c>.</param>
        /// <returns>
        /// The bit position of the last reset bit (0) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanZeroReverse(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_scan_zero_reverse_be32(count, bits, position);
        }

        /// <summary>
        /// Counts the number of one bits (population count) in a range of 64-bits integers.
        /// </summary>
        /// <param name="count">The number of bits to count.</param>
        /// <param name="bits">The bits to count.</param>
        /// <param name="position">The starting bit position to count.</param>
        /// <returns>
        /// The number of one bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOneBits(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_onecount_32(count, bits, position);
        }

        /// <summary>
        /// Counts the number of zero bits (population count) in a range of 32-bits integers.
        /// </summary>
        /// <param name="count">The number of bits to count.</param>
        /// <param name="bits">The bits to count.</param>
        /// <param name="position">The starting bit position to count.</param>
        /// <returns>
        /// The number of zero bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountZeroBits(int count, uint[] bits, int position)
        {
            return NativeMethods.bits_zerocount_32(count, bits, position);
        }

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
        /// <param name="offxy">The index in the <c>xy</c> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, uint[] xy, int offxy)
        {
            NativeMethods.bytesswap_ip_32(length, xy, offxy);
        }

        /// <summary>
        /// Reverses the order of bytes in an array of 32-bit integers.
        /// </summary>
        /// <param name="length">The number of integers to swap.</param>
        /// <param name="x">The integers to reverse byte order.</param>
        /// <param name="offx">The index in the <c>x</c> at which swapping begins.</param>
        /// <param name="y">The integers that receive swapped bytes.</param>
        /// <param name="offy">The index in the <c>y</c> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bytesswap_32(length, x, offx, y, offy);
        }

        private static class NativeMethods
        {
            private const string DllName = "Accord.DNN.CPP.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int byteswap_32(uint bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bytesswap_ip_32(int n, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bytesswap_32(int n, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bit_scan_forward_be32(uint bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bit_scan_reverse_be32(uint bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_one_forward_be32(int count, uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_one_reverse_be32(int count, uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_zero_forward_be32(int count, uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_zero_reverse_be32(int count, uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_reset_be32(int count, [In, Out] uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_set_be32(int count, [In, Out] uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_copy_be32(int count, [In] uint[] bitssrc, int possrc, [Out] uint[] bitsdst, int posdst);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint bits_onecount_32(int count, [In] uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint bits_zerocount_32(int count, [In] uint[] bits, int pos);
        }
    }
} 
