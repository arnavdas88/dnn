// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.Linq
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for the <see cref="IEnumerable{T}"/> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Randomizes the sequence using default random numbers generator.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to randomize.</param>
        /// <param name="bufferSize">The size of intermediate buffer.</param>
        /// <returns>
        /// The randomized sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="source"/> is <b>null</b>.</para>
        /// </exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, int bufferSize)
        {
            return source.Shuffle(bufferSize, new Random(0));
        }

        /// <summary>
        /// Randomizes the sequence using specified random numbers generator.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to randomize.</param>
        /// <param name="bufferSize">The size of intermediate buffer.</param>
        /// <param name="random">The random numbers generator.</param>
        /// <returns>
        /// The randomized sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="source"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="random"/> is <b>null</b>.</para>
        /// </exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, int bufferSize, Random random)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return source.ShuffleIterator(random, 10);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random random, int bufferSize)
        {
            List<T> buffer = new List<T>(bufferSize);

            // enumerate through entire sequence
            // maintaining buffer of fixed size and releasing random elements from it
            foreach (T element in source)
            {
                buffer.Add(element);

                if (buffer.Count >= bufferSize)
                {
                    int i = random.Next(0, buffer.Count);
                    yield return buffer[i];
                    buffer.RemoveAt(i);
                }
            }

            // release the buffer using Fisher-Yates-Durstenfeld shuffle
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = random.Next(i, buffer.Count);
                yield return buffer[j];

                if (i != j)
                {
                    buffer[j] = buffer[i];
                }
            }
        }
    }
}
