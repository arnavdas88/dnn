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
    /// <typeparam name="T">Specifies the type of elements in the heap.</typeparam>
    /// <remarks>
    /// <each>
    /// Each element in the heap satisfies the "heap" property condition that it smaller or equal to all its children.
    /// </each>
    /// </remarks>
    public interface IHeap<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="IHeap{T}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="IHeap{T}"/>.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Removes all elements from the <see cref="IHeap{T}"/>.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds the specified element to the heap.
        /// </summary>
        /// <param name="value">The element to add.</param>
        void Push(T value);

        /// <summary>
        /// Removes and returns the element at the top of the <see cref="IHeap{T}"/>.
        /// </summary>
        /// <returns>
        /// The element that is removed from the top of the <see cref="IHeap{T}"/>.
        /// </returns>
        T Pop();

        /// <summary>
        /// Returns the element at the top of the <see cref="IHeap{T}"/> without removing it.
        /// </summary>
        /// <returns>
        /// The element at the top of the <see cref="IHeap{T}"/>.
        /// </returns>
        T Peek();
    }
}
