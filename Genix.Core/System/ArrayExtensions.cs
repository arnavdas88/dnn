// -----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Runtime.CompilerServices;

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
        /// Calculates a variance (second central moment) of a sequence of <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="Single"/> values to calculate the variance of.</param>
        /// <returns>The variance of the values in the sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>source</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <c>source</c> contains no elements.
        /// </exception>
        public static float Variance(this float[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int length = source.Length;
            if (length == 0)
            {
                throw new InvalidOperationException(Genix.Core.Properties.Resources.E_SequenceIsEmpty);
            }

            float mean = 0.0f;
            for (int i = 0; i < length; i++)
            {
                mean += source[i];
            }

            mean /= length;

            float variance = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float delta = source[i] - mean;
                variance += delta * delta;
            }

            return variance / length;
        }
    }
}
