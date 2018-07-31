﻿// -----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides extension methods for the <see cref="Array"/> class.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Inserts an element into the array at the specified position.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to insert the element into.</param>
        /// <param name="position">The insertion position.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The modified array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InsertAt<T>(this T[] array, int position, T value)
        {
            int rank = array?.Length ?? throw new ArgumentNullException(nameof(array));

            if (position < 0 || position > rank)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            T[] newarray = new T[++rank];

            for (int i = 0; i < position; i++)
            {
                newarray[i] = array[i];
            }

            newarray[position] = value;

            for (int i = position + 1; i < rank; i++)
            {
                newarray[i] = array[i - 1];
            }

            return newarray;
        }

        /// <summary>
        /// Removes an element at the specified position from the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to remove the element from.</param>
        /// <param name="position">The removal position.</param>
        /// <returns>The modified array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RemoveAt<T>(this T[] array, int position)
        {
            int rank = array?.Length ?? throw new ArgumentNullException(nameof(array));

            if (position < 0 || position >= rank)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            T[] newarray = new T[--rank];

            for (int i = 0; i < position; i++)
            {
                newarray[i] = array[i];
            }

            for (int i = position + 1; i <= rank; i++)
            {
                newarray[i - 1] = array[i];
            }

            return newarray;
        }

        /// <summary>
        /// Swaps two elements of the array at the specified positions.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to swap.</param>
        /// <param name="position1">The position of the first element.</param>
        /// <param name="position2">The position of the second element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this T[] array, int position1, int position2)
        {
            T temp = array[position1];
            array[position1] = array[position2];
            array[position2] = temp;
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(this int[] srcdst, int alpha)
        {
            Mathematics.AddC(srcdst.Length, alpha, srcdst, 0);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 32-bit unsigned integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(this uint[] srcdst, uint alpha)
        {
            Mathematics.AddC(srcdst.Length, alpha, srcdst, 0);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(this long[] srcdst, long alpha)
        {
            Mathematics.AddC(srcdst.Length, alpha, srcdst, 0);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of 64-bit unsigned integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void AddC(this ulong[] srcdst, ulong alpha)
        {
            Mathematics.AddC(srcdst.Length, alpha, srcdst, 0);
        }

        /// <summary>
        /// Adds a constant value to each element of an array of floats.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddC(this float[] srcdst, float alpha)
        {
            Mathematics.AddC(srcdst.Length, alpha, srcdst, 0);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="src">The array that contains source data.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += src(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this int[] srcdst, int[] src)
        {
            Mathematics.Add(srcdst.Length, src, 0, srcdst, 0);
        }

        /// <summary>
        /// Adds the elements of two arrays of 32-bit unsigned integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="src">The array that contains source data.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += src(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(this uint[] srcdst, uint[] src)
        {
            Mathematics.Add(srcdst.Length, src, 0, srcdst, 0);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="src">The array that contains source data.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += src(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this long[] srcdst, long[] src)
        {
            Mathematics.Add(srcdst.Length, src, 0, srcdst, 0);
        }

        /// <summary>
        /// Adds the elements of two arrays of 64-bit unsigned integers.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="src">The array that contains source data.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += src(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void Add(this ulong[] srcdst, ulong[] src)
        {
            Mathematics.Add(srcdst.Length, src, 0, srcdst, 0);
        }

        /// <summary>
        /// Adds the elements of two arrays of floats.
        /// </summary>
        /// <param name="srcdst">The array that contains source and destination data.</param>
        /// <param name="src">The array that contains source data.</param>
        /// <remarks>
        /// The method performs operation defined as <c>srcdst(i) += src(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this float[] srcdst, float[] src)
        {
            Mathematics.Add(srcdst.Length, src, 0, srcdst, 0);
        }

        /// <summary>
        /// Computes the sum of all elements in the array of floats.
        /// </summary>
        /// <param name="source">The array of <see cref="float"/> values to calculate the sum of.</param>
        /// <returns>
        /// The sum of all elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this float[] source)
        {
            return Mathematics.Sum(source.Length, source, 0);
        }

        /// <summary>
        /// Computes the variance of all elements in the array of floats.
        /// </summary>
        /// <param name="source">The array of <see cref="float"/> values to calculate the variance.</param>
        /// <returns>
        /// The variance of all elements in the array.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>source</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <c>source</c> contains no elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Variance(this float[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Length == 0)
            {
                throw new InvalidOperationException(Genix.Core.Properties.Resources.E_SequenceIsEmpty);
            }

            return Mathematics.Variance(source.Length, source, 0);
        }
    }
}