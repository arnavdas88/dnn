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

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> 
        /// according to specified key selector and element selector functions.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> that contains values of type <typeparamref name="TElement"/> selected from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="source"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="keySelector"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="elementSelector"/> is <b>null</b>.</para>
        /// </exception>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TKey> keySelector,
            Func<TSource, int, TElement> elementSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            Dictionary<TKey, TElement> result =
                source is ICollection<TSource> collection ?
                new Dictionary<TKey, TElement>(collection.Count) :
                new Dictionary<TKey, TElement>();

            int index = -1;
            foreach (TSource element in source)
            {
                index++;
                result.Add(keySelector(element, index), elementSelector(element, index));
            }

            return result;
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
