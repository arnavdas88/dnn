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
    /// Provides bit manipulation methods for 32-bit big-endian architecture.
    /// </summary>
    [CLSCompliant(false)]
    public static class BitUtils32
    {
        /// <summary>
        /// The first bit.
        /// </summary>
        public const uint LSB = 0x8000_0000;

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
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "32-count", Justification = "Do not validate for performance. Use assert instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBits(uint bits, int position, int count)
        {
            Debug.Assert(position + count < 32, "The position+count must be less than 32.");
            return (bits >> (32 - (position + count))) & (uint.MaxValue >> (32 - count));
        }

        /// <summary>
        /// Sets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to set.</param>
        /// <param name="position">The bit position to set.</param>
        /// <returns>The changed bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBit(uint bits, int position)
        {
            Debug.Assert(position < 32, "The bit position must be less than 32.");
            return bits | (LSB >> position);
        }

        /// <summary>
        /// Sets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The bit position to reset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(uint[] bits, int position)
        {
            bits[position >> 5] |= BitUtils32.LSB >> (position & 31);
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
        /// <returns>The changed bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ResetBit(uint bits, int position)
        {
            Debug.Assert(position < 32, "The bit position must be less than 32.");
            return bits & ~(LSB >> position);
        }

        /// <summary>
        /// Resets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The bit position to reset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetBit(uint[] bits, int position)
        {
            bits[position >> 5] &= ~(BitUtils32.LSB >> (position & 31));
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
        /// <param name="source">The bits to copy from.</param>
        /// <param name="sourcePosition">The starting bit position in <c>bitssrc</c>.</param>
        /// <param name="destination">The bits to copy to.</param>
        /// <param name="destinationPosition">The starting bit position in <c>bitsdst</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBits(int count, uint[] source, int sourcePosition, uint[] destination, int destinationPosition)
        {
            NativeMethods.bits_copy_be32(count, source, sourcePosition, destination, destinationPosition);
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
        /// Counts the number of one bits (population count) in a 32-bits integer.
        /// </summary>
        /// <param name="bits">The bits to count.</param>
        /// <returns>
        /// The number of one bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOneBits(uint bits)
        {
            return (int)NativeMethods.bits_popcount_32(bits);
        }

        /// <summary>
        /// Counts the number of zero bits (population count) in a 32-bits integer.
        /// </summary>
        /// <param name="bits">The bits to count.</param>
        /// <returns>
        /// The number of zero bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountZeroBits(uint bits)
        {
            return 32 - BitUtils32.CountOneBits(bits);
        }

        /// <summary>
        /// Counts the number of one bits (population count) in a range of 32-bits integers.
        /// </summary>
        /// <param name="count">The number of bits to count.</param>
        /// <param name="bits">The bits to count.</param>
        /// <param name="position">The starting bit position to count.</param>
        /// <returns>
        /// The number of one bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOneBits(int count, uint[] bits, int position)
        {
            return (int)NativeMethods.bits_count_32(count, bits, position);
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
        public static int CountZeroBits(int count, uint[] bits, int position)
        {
            return count - BitUtils32.CountOneBits(count, bits, position);
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

        /// <summary>
        /// Performs logical NOT operation on single 32-bits array element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="xy">The source array.</param>
        /// <param name="offxy">The starting element position in <c>xy</c>.</param>
        public static void WordsNOT(int length, uint[] xy, int offxy)
        {
            NativeMethods.bits_not1_32(length, xy, offxy);
        }

        /// <summary>
        /// Performs logical NOT operation on two 32-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsNOT(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bits_not2_32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical OR operation on two 32-bits arrays bit-wise.
        /// </summary>
        /// <param name="count">The number of bits to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="posx">The starting bit position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="posy">The starting bit position in <c>y</c>.</param>
        public static void BitsOR(int count, uint[] x, int posx, uint[] y, int posy)
        {
            NativeMethods.bits_or2_be32(count, x, posx, y, posy);
        }

        /// <summary>
        /// Performs logical OR operation on two 32-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordOR(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bits_or2_32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical OR operation on two 32-bits arrays element-wise and puts the results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting element position in <c>a</c>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting element position in <c>b</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordOR(int length, uint[] a, int offa, uint[] b, int offb, uint[] y, int offy)
        {
            NativeMethods.bits_or3_32(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation between 32-bits array and a scalar value.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="mask">The mask to apply.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
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
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        /// <param name="incy">The increment for the elements of <c>y</c></param>
        public static void WordsAND(int length, uint mask, uint[] y, int offy, int incy)
        {
            NativeMethods.bits_and_mask_inc_32(length, mask, y, offy, incy);
        }

        /// <summary>
        /// Performs logical AND operation on two 32-bits arrays bit-wise.
        /// </summary>
        /// <param name="count">The number of bits to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="posx">The starting bit position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="posy">The starting bit position in <c>y</c>.</param>
        public static void BitsAND(int count, uint[] x, int posx, uint[] y, int posy)
        {
            NativeMethods.bits_and2_be32(count, x, posx, y, posy);
        }

        /// <summary>
        /// Performs logical AND operation on two 32-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordAND(int length, uint[] x, int offx, uint[] y, int offy)
        {
            NativeMethods.bits_and2_32(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation on two 32-bits arrays element-wise and puts the results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting element position in <c>a</c>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting element position in <c>b</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordAND(int length, uint[] a, int offa, uint[] b, int offb, uint[] y, int offy)
        {
            NativeMethods.bits_and3_32(length, a, offa, b, offb, y, offy);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

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
            public static extern uint bits_popcount_32(uint bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint bits_count_32(int count, [In] uint[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_not1_32(int length, [In, Out] uint[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_not2_32(int length, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or2_32(int length, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or2_be32(int count, [In] uint[] x, int posx, [Out] uint[] y, int posy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or3_32(int length, [In] uint[] a, int offa, [In] uint[] b, int offb, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and_mask_32(int length, [In] uint mask, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and_mask_inc_32(int length, [In] uint mask, [Out] uint[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and2_32(int length, [In] uint[] x, int offx, [Out] uint[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and2_be32(int count, [In] uint[] x, int posx, [Out] uint[] y, int posy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and3_32(int length, [In] uint[] a, int offa, [In] uint[] b, int offb, [Out] uint[] y, int offy);
        }
    }
}
