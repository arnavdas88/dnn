// -----------------------------------------------------------------------
// <copyright file="PriorityQueue.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a <see cref="Queue{T}"/>-like interface,
    /// except that objects "pushed" in arbitrary order are "popped" in order of priority.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the key the elements are ordered by.</typeparam>
    /// <typeparam name="TValue">Specifies the type of elements in the queue.</typeparam>
    public class PriorityQueue<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly IHeap<TKey, TValue> heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial queue capacity.</param>
        public PriorityQueue(int initialCapacity)
        {
            this.heap = new BinaryHeap<TKey, TValue>(initialCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial queue capacity.</param>
        /// <param name="maximumSize">The maximum recommended number of elements in the queue. The actual number of elements might be larger.</param>
        public PriorityQueue(int initialCapacity, int maximumSize)
        {
            this.heap = new BinaryHeap<TKey, TValue>(initialCapacity, maximumSize);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </value>
        public int Count => this.heap.Count;

        /// <summary>
        /// Removes all elements from the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.heap.Clear();

        /// <summary>
        /// Adds an element to the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The element to add to the <see cref="PriorityQueue{TKey, TValue}"/>.</param>
        /// <returns>
        /// <b>true</b> if the element was added to the queue; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Enqueue(TKey key, TValue value) => this.heap.Push(key, value);

        /// <summary>
        /// Removes and returns the element at the beginning of the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// The element with the highest priority that is removed from the beginning of the <see cref="PriorityQueue{TKey, TValue}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="PriorityQueue{TKey, TValue}"/> is empty.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (TKey key, TValue value) Dequeue() => this.heap.Pop();
    }
}
