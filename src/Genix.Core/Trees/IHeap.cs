// -----------------------------------------------------------------------
// <copyright file="IHeap.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;

    /// <summary>
    /// Defines a contract for a heap.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the "heap" property.</typeparam>
    /// <typeparam name="TValue">Specifies the type of elements in the heap.</typeparam>
    /// <remarks>
    /// <each>
    /// Each element in the heap satisfies the "heap" property condition that it smaller or equal to all its children.
    /// </each>
    /// </remarks>
    public interface IHeap<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="IHeap{TKey, TValue}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="IHeap{TKey, TValue}"/>.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Removes all elements from the <see cref="IHeap{TKey, TValue}"/>.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds the specified element to the heap.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The element to add.</param>
        /// <returns>
        /// <b>true</b> if the element was added to the heap; otherwise, <b>false</b>.
        /// </returns>
        bool Push(TKey key, TValue value);

        /// <summary>
        /// Removes and returns the element at the top of the <see cref="IHeap{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// The element that is removed from the top of the <see cref="IHeap{TKey, TValue}"/>.
        /// </returns>
        (TKey key, TValue value) Pop();

        /// <summary>
        /// Returns the element at the top of the <see cref="IHeap{TKey, TValue}"/> without removing it.
        /// </summary>
        /// <returns>
        /// The element at the top of the <see cref="IHeap{TKey, TValue}"/>.
        /// </returns>
        (TKey key, TValue value) Peek();
    }
}
