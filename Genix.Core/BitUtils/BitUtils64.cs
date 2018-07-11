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
    /// Provides bit manipulation methods for 64-bit big-endian architecture.
    /// </summary>
    [CLSCompliant(false)]
    public static class BitUtils64
    {
        /// <summary>
        /// The first bit.
        /// </summary>
        public const ulong LSB = 0x8000_0000_0000_0000ul;

        /// <summary>
        /// Examines the bit at the specified position, and returns the value of that bit.
        /// </summary>
        /// <param name="bits">The bits to examine.</param>
        /// <param name="position">The bit position to test.</param>
        /// <returns>
        /// The bit at the position specified.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TestBit(ulong bits, int position)
        {
            Debug.Assert(position < 64, "The bit position must be less than 64.");
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
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "64-count", Justification = "Do not validate for performance. Use assert instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetBits(ulong bits, int position, int count)
        {
            Debug.Assert(position + count < 64, "The position+count must be less than 64.");
            return (bits >> (64 - (position + count))) & (ulong.MaxValue >> (64 - count));
        }

        /// <summary>
        /// Sets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to set.</param>
        /// <param name="position">The bit position to set.</param>
        /// <returns>The changed bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SetBit(ulong bits, int position)
        {
            Debug.Assert(position < 64, "The bit position must be less than 64.");
            return bits | (LSB >> position);
        }

        /// <summary>
        /// Sets the range bits at the specified position.
        /// </summary>
        /// <param name="count">The number of bits to set.</param>
        /// <param name="bits">The bits to set.</param>
        /// <param name="position">The starting bit position to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBits(int count, ulong[] bits, int position)
        {
            NativeMethods.bits_set_be64(count, bits, position);
        }

        /// <summary>
        /// Resets the bit at the specified position.
        /// </summary>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The bit position to reset.</param>
        /// <returns>The changed bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetBit(ulong bits, int position)
        {
            Debug.Assert(position < 64, "The bit position must be less than 64.");
            return bits & ~(LSB >> position);
        }

        /// <summary>
        /// Resets the range bits at the specified position.
        /// </summary>
        /// <param name="count">The number of bits to reset.</param>
        /// <param name="bits">The bits to reset.</param>
        /// <param name="position">The starting bit position to reset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetBits(int count, ulong[] bits, int position)
        {
            NativeMethods.bits_reset_be64(count, bits, position);
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
        public static void CopyBits(int count, ulong[] source, int sourcePosition, ulong[] destination, int destinationPosition)
        {
            NativeMethods.bits_copy_be64(count, source, sourcePosition, destination, destinationPosition);
        }

        /// <summary>
        /// Searches the range of values for a first set bit (1).
        /// </summary>
        /// <param name="bits">The bits to search.</param>
        /// <returns>
        /// The bit position of the first set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneForward(ulong bits)
        {
            return NativeMethods.bit_scan_forward_be64(bits);
        }

        /// <summary>
        /// Searches the range of values for a last set bit (1).
        /// </summary>
        /// <param name="bits">The bits to search.</param>
        /// <returns>
        /// The bit position of the last set bit (1) found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanOneReverse(ulong bits)
        {
            return NativeMethods.bit_scan_reverse_be64(bits);
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
        public static int BitScanOneForward(int count, ulong[] bits, int position)
        {
            return NativeMethods.bits_scan_one_forward_be64(count, bits, position);
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
        public static int BitScanOneReverse(int count, ulong[] bits, int position)
        {
            return NativeMethods.bits_scan_one_reverse_be64(count, bits, position);
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
        public static int BitScanZeroForward(int count, ulong[] bits, int position)
        {
            return NativeMethods.bits_scan_zero_forward_be64(count, bits, position);
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
        public static int BitScanZeroReverse(int count, ulong[] bits, int position)
        {
            return NativeMethods.bits_scan_zero_reverse_be64(count, bits, position);
        }

        /// <summary>
        /// Counts the number of one bits (population count) in a 64-bits integer.
        /// </summary>
        /// <param name="bits">The bits to count.</param>
        /// <returns>
        /// The number of one bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOneBits(ulong bits)
        {
            return (int)NativeMethods.bits_popcount_64(bits);
        }

        /// <summary>
        /// Counts the number of zero bits (population count) in a 64-bits integer.
        /// </summary>
        /// <param name="bits">The bits to count.</param>
        /// <returns>
        /// The number of zero bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountZeroBits(ulong bits)
        {
            return 64 - BitUtils64.CountOneBits(bits);
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
        public static int CountOneBits(int count, ulong[] bits, int position)
        {
            return (int)NativeMethods.bits_count_64(count, bits, position);
        }

        /// <summary>
        /// Counts the number of zero bits (population count) in a range of 64-bits integers.
        /// </summary>
        /// <param name="count">The number of bits to count.</param>
        /// <param name="bits">The bits to count.</param>
        /// <param name="position">The starting bit position to count.</param>
        /// <returns>
        /// The number of zero bits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountZeroBits(int count, ulong[] bits, int position)
        {
            return count - BitUtils64.CountOneBits(count, bits, position);
        }

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
        /// <param name="offxy">The index in the <c>xy</c> at which swapping begins.</param>
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
        /// <param name="offx">The index in the <c>x</c> at which swapping begins.</param>
        /// <param name="y">The integers that receive swapped bytes.</param>
        /// <param name="offy">The index in the <c>y</c> at which swapping begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BiteSwap(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bytesswap_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Copies the 64-bits array into 32-bits array.
        /// </summary>
        /// <param name="count">The number of 32-bit words to copy.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        /// <param name="swapBytes">Determines whether the bytes should be swapped while copying.</param>
        public static void Copy64To32(int count, ulong[] x, int offx, uint[] y, int offy, bool swapBytes)
        {
            NativeMethods.bits_copy_be64to32(count, x, offx, y, offy, swapBytes);
        }

        /// <summary>
        /// Copies the 32-bits array into 64-bits array.
        /// </summary>
        /// <param name="count">The number of 32-bit words to copy.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        /// <param name="swapBytes">Determines whether the bytes should be swapped while copying.</param>
        public static void Copy32To64(int count, uint[] x, int offx, ulong[] y, int offy, bool swapBytes)
        {
            NativeMethods.bits_copy_be32to64(count, x, offx, y, offy, swapBytes);
        }

        /// <summary>
        /// Performs logical NOT operation on single 64-bits array element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="xy">The source array.</param>
        /// <param name="offxy">The starting element position in <c>xy</c>.</param>
        public static void WordsNOT(int length, ulong[] xy, int offxy)
        {
            NativeMethods.bits_not1_64(length, xy, offxy);
        }

        /// <summary>
        /// Performs logical NOT operation on two 64-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsNOT(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bits_not2_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical OR operation on two 64-bits arrays bit-wise.
        /// </summary>
        /// <param name="count">The number of bits to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="posx">The starting bit position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="posy">The starting bit position in <c>y</c>.</param>
        public static void BitsOR(int count, ulong[] x, int posx, ulong[] y, int posy)
        {
            NativeMethods.bits_or2_be64(count, x, posx, y, posy);
        }

        /// <summary>
        /// Performs logical OR operation on two 64-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsOR(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bits_or2_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical OR operation on two 64-bits arrays element-wise and puts the results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting element position in <c>a</c>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting element position in <c>b</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsOR(int length, ulong[] a, int offa, ulong[] b, int offb, ulong[] y, int offy)
        {
            NativeMethods.bits_or3_64(length, a, offa, b, offb, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation on two 64-bits arrays bit-wise.
        /// </summary>
        /// <param name="count">The number of bits to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="posx">The starting bit position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="posy">The starting bit position in <c>y</c>.</param>
        public static void BitsAND(int count, ulong[] x, int posx, ulong[] y, int posy)
        {
            NativeMethods.bits_and2_be64(count, x, posx, y, posy);
        }

        /// <summary>
        /// Performs logical AND operation on two 64-bits arrays element-wise.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting element position in <c>x</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsAND(int length, ulong[] x, int offx, ulong[] y, int offy)
        {
            NativeMethods.bits_and2_64(length, x, offx, y, offy);
        }

        /// <summary>
        /// Performs logical AND operation on two 64-bits arrays element-wise and puts the results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="a">The first source array.</param>
        /// <param name="offa">The starting element position in <c>a</c>.</param>
        /// <param name="b">The second source array.</param>
        /// <param name="offb">The starting element position in <c>b</c>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting element position in <c>y</c>.</param>
        public static void WordsAND(int length, ulong[] a, int offa, ulong[] b, int offb, ulong[] y, int offy)
        {
            NativeMethods.bits_and3_64(length, a, offa, b, offb, y, offy);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int byteswap_64(ulong bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bytesswap_ip_64(int n, [In, Out] ulong[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bytesswap_64(int n, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bit_scan_forward_be64(ulong bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bit_scan_reverse_be64(ulong bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_one_forward_be64(int count, ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_one_reverse_be64(int count, ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_zero_forward_be64(int count, ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int bits_scan_zero_reverse_be64(int count, ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_reset_be64(int count, [In, Out] ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_set_be64(int count, [In, Out] ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_copy_be64(int count, [In] ulong[] bitssrc, int possrc, [Out] ulong[] bitsdst, int posdst);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint bits_popcount_64(ulong bits);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern ulong bits_count_64(int count, [In] ulong[] bits, int pos);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_copy_be64to32(int count, [In] ulong[] src, int offsrc, [Out] uint[] dst, int offdst, [MarshalAs(UnmanagedType.Bool)] bool swapBytes);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_copy_be32to64(int count, [In] uint[] src, int offsrc, [Out] ulong[] dst, int offdst, [MarshalAs(UnmanagedType.Bool)] bool swapBytes);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_not1_64(int length, [In, Out] ulong[] xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_not2_64(int length, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or2_64(int length, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or2_be64(int count, [In] ulong[] x, int posx, [Out] ulong[] y, int posy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_or3_64(int length, [In] ulong[] a, int offa, [In] ulong[] b, int offb, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and2_64(int length, [In] ulong[] x, int offx, [Out] ulong[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and2_be64(int count, [In] ulong[] x, int posx, [Out] ulong[] y, int posy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void bits_and3_64(int length, [In] ulong[] a, int offa, [In] ulong[] b, int offb, [Out] ulong[] y, int offy);
        }
    }
} 