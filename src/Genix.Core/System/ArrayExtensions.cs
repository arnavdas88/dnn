// -----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
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
        /// Adds an element to the end of the array.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <param name="array">The array to add the element to.</param>
        /// <param name="element">The element to be added to the end of the array. The value can be null for reference types.</param>
        /// <returns>The modified array.</returns>
        public static T[] Add<T>(this T[] array, T element)
        {
            if (array == null)
            {
                return new T[1] { element };
            }

            int oldLength = array.Length;
            T[] newarray = new T[oldLength + 1];

            if (oldLength > 0)
            {
                Array.Copy(array, 0, newarray, 0, oldLength);
            }

            newarray[oldLength] = element;
            return newarray;
        }

        /// <summary>
        /// Expands the arrays by inserting the specified number of elements at the specified position.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to insert the element into.</param>
        /// <param name="position">The insertion position.</param>
        /// <param name="count">The number of elements to insert.</param>
        /// <returns>The modified array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Expand<T>(this T[] array, int position, int count)
        {
            int oldLength = array.Length;
            T[] newarray = new T[oldLength + count];

            if (position > 0)
            {
                Array.Copy(array, 0, newarray, 0, position);
            }

            if (position < oldLength)
            {
                Array.Copy(array, position, newarray, position + count, oldLength - position);
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
        /// Splits the array into partitions of fixed size.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="partitionSize">The number of elements in each partition.</param>
        /// <returns>
        /// The sequence of partitions.
        /// </returns>
        public static IEnumerable<T[]> Partition<T>(this T[] array, int partitionSize)
        {
            for (int i = 0, ii = array.Length - partitionSize; i <= ii; i += partitionSize)
            {
                T[] buffer = new T[partitionSize];
                Array.Copy(array, i, buffer, 0, partitionSize);
                yield return buffer;
            }
        }

        /// <summary>
        /// Computes the sum of all elements in the array of floats.
        /// </summary>
        /// <param name="source">The array of <see cref="float"/> values to calculate the sum of.</param>
        /// <returns>
        /// The sum of all elements in the array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this float[] source) => Vectors.Sum(source.Length, source, 0);

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

            return Vectors.Variance(source.Length, source, 0);
        }
    }
}
