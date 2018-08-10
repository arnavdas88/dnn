﻿// -----------------------------------------------------------------------
// <copyright file="CommonParallel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Used to simplify parallel code.
    /// </summary>
    public static class CommonParallel
    {
        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        public static void For(int fromInclusive, int toExclusive, Action<int, int> body, ParallelOptions parallelOptions)
        {
            int maxDegreeOfParallelism = CommonParallel.GetMaxDegreeOfParallelism(parallelOptions);
            For(fromInclusive, toExclusive, Math.Max(1, (toExclusive - fromInclusive) / maxDegreeOfParallelism), body, parallelOptions);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="rangeSize">The partition size for splitting work into smaller pieces.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        public static void For(int fromInclusive, int toExclusive, int rangeSize, Action<int, int> body, ParallelOptions parallelOptions)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (fromInclusive < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fromInclusive));
            }

            if (fromInclusive > toExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(toExclusive));
            }

            if (rangeSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeSize));
            }

            int length = toExclusive - fromInclusive;
            if (length > 0)
            {
                int maxDegreeOfParallelism = CommonParallel.GetMaxDegreeOfParallelism(parallelOptions);
                if (maxDegreeOfParallelism < 2 || (rangeSize * 2) > length)
                {
                    // Special case: not worth to parallelize, inline
                    body(fromInclusive, toExclusive);
                }
                else
                {
                    // Common case
                    Parallel.ForEach(
                        Partitioner.Create(fromInclusive, toExclusive, rangeSize),
                        parallelOptions,
                        range => body(range.Item1, range.Item2));
                }
            }
        }

        /// <summary>
        /// Executes each of the provided actions inside a discrete, asynchronous task.
        /// </summary>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <param name="actions">An array of actions to execute.</param>
        /// <exception cref="ArgumentException">The actions array contains a <c>null</c> element.</exception>
        /// <exception cref="AggregateException">At least one invocation of the actions threw an exception.</exception>
        public static void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            // Special case: no action
            if (actions.Length == 0)
            {
                return;
            }

            // Special case: single action, inline
            if (actions.Length == 1)
            {
                actions[0]();
                return;
            }

            // Special case: straight execution without parallelism
            int maxDegreeOfParallelism = CommonParallel.GetMaxDegreeOfParallelism(parallelOptions);
            if (maxDegreeOfParallelism < 2)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i]();
                }

                return;
            }

            // Common case
            Parallel.Invoke(parallelOptions, actions);
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <typeparam name="T">The type of selected items.</typeparam>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reduce">The function to select the item of selection from the subsets.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <returns>The selected value.</returns>
        public static T Aggregate<T>(int fromInclusive, int toExclusive, Func<int, T> select, Func<T[], T> reduce, ParallelOptions parallelOptions)
        {
            if (select == null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (reduce == null)
            {
                throw new ArgumentNullException(nameof(reduce));
            }

            // Special case: no action
            if (fromInclusive >= toExclusive)
            {
                return reduce(new T[0]);
            }

            // Special case: single action, inline
            if (fromInclusive == (toExclusive - 1))
            {
                return reduce(new[] { select(fromInclusive) });
            }

            // Special case: straight execution without parallelism
            int maxDegreeOfParallelism = CommonParallel.GetMaxDegreeOfParallelism(parallelOptions);
            if (maxDegreeOfParallelism < 2)
            {
                var mapped = new T[toExclusive - fromInclusive];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k + fromInclusive);
                }

                return reduce(mapped);
            }

            // Common case
            var intermediateResults = new List<T>();
            var syncLock = new object();
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive),
                parallelOptions,
                () => new List<T>(),
                (range, loop, localData) =>
                {
                    var mapped = new T[range.Item2 - range.Item1];
                    for (int k = 0; k < mapped.Length; k++)
                    {
                        mapped[k] = select(k + range.Item1);
                    }

                    localData.Add(reduce(mapped));
                    return localData;
                },
                localResult =>
                {
                    lock (syncLock)
                    {
                        intermediateResults.Add(reduce(localResult.ToArray()));
                    }
                });

            return reduce(intermediateResults.ToArray());
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <typeparam name="T">The type of selected items.</typeparam>
        /// <typeparam name="TOut">The type of selected value.</typeparam>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reduce">The function to select the item of selection from the subsets.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <returns>The selected value.</returns>
        public static TOut Aggregate<T, TOut>(T[] array, Func<int, T, TOut> select, Func<TOut[], TOut> reduce, ParallelOptions parallelOptions)
        {
            if (select == null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (reduce == null)
            {
                throw new ArgumentNullException(nameof(reduce));
            }

            // Special case: no action
            if (array == null || array.Length == 0)
            {
                return reduce(new TOut[0]);
            }

            // Special case: single action, inline
            if (array.Length == 1)
            {
                return reduce(new[] { select(0, array[0]) });
            }

            // Special case: straight execution without parallelism
            int maxDegreeOfParallelism = CommonParallel.GetMaxDegreeOfParallelism(parallelOptions);
            if (maxDegreeOfParallelism < 2)
            {
                var mapped = new TOut[array.Length];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k, array[k]);
                }

                return reduce(mapped);
            }

            // Common case
            var intermediateResults = new List<TOut>();
            var syncLock = new object();
            Parallel.ForEach(
                Partitioner.Create(0, array.Length),
                parallelOptions,
                () => new List<TOut>(),
                (range, loop, localData) =>
                {
                    var mapped = new TOut[range.Item2 - range.Item1];
                    for (int k = 0; k < mapped.Length; k++)
                    {
                        mapped[k] = select(k + range.Item1, array[k + range.Item1]);
                    }

                    localData.Add(reduce(mapped));
                    return localData;
                },
                localResult =>
                {
                    lock (syncLock)
                    {
                        intermediateResults.Add(reduce(localResult.ToArray()));
                    }
                });

            return reduce(intermediateResults.ToArray());
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <typeparam name="T">The type of selected items.</typeparam>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reducePair">The function to select the item of selection from the subsets.</param>
        /// <param name="reduceDefault">Default result of the reduce function on an empty set.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <returns>The selected value.</returns>
        public static T Aggregate<T>(int fromInclusive, int toExclusive, Func<int, T> select, Func<T, T, T> reducePair, T reduceDefault, ParallelOptions parallelOptions)
        {
            return Aggregate(
                fromInclusive,
                toExclusive,
                select,
                results =>
                {
                    if (results == null || results.Length == 0)
                    {
                        return reduceDefault;
                    }

                    if (results.Length == 1)
                    {
                        return results[0];
                    }

                    T result = results[0];
                    for (int i = 1; i < results.Length; i++)
                    {
                        result = reducePair(result, results[i]);
                    }

                    return result;
                },
                parallelOptions);
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <typeparam name="T">The type of selected items.</typeparam>
        /// <typeparam name="TOut">The type of selected value.</typeparam>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reducePair">The function to select the item of selection from the subsets.</param>
        /// <param name="reduceDefault">Default result of the reduce function on an empty set.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <returns>The selected value.</returns>
        public static TOut Aggregate<T, TOut>(T[] array, Func<int, T, TOut> select, Func<TOut, TOut, TOut> reducePair, TOut reduceDefault, ParallelOptions parallelOptions)
        {
            return Aggregate(
                array,
                select,
                results =>
                {
                    if (results == null || results.Length == 0)
                    {
                        return reduceDefault;
                    }

                    if (results.Length == 1)
                    {
                        return results[0];
                    }

                    TOut result = results[0];
                    for (int i = 1; i < results.Length; i++)
                    {
                        result = reducePair(result, results[i]);
                    }

                    return result;
                },
                parallelOptions);
        }

        private static int GetMaxDegreeOfParallelism(ParallelOptions parallelOptions)
        {
            int maxDegreeOfParallelism = parallelOptions.MaxDegreeOfParallelism;
            if (maxDegreeOfParallelism == -1)
            {
                maxDegreeOfParallelism = Math.Min(System.Environment.ProcessorCount, 512);
            }

            return maxDegreeOfParallelism;
        }
    }
}
