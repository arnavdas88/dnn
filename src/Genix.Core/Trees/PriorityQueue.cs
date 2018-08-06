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
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    public class PriorityQueue<T>
        where T : IComparable<T>
    {
        private readonly Heap<T> heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial heap capacity.</param>
        public PriorityQueue(int capacity)
        {
            this.heap = new Heap<T>(capacity);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="PriorityQueue{T}"/>.
        /// </value>
        public int Count => this.heap.Count;

        /// <summary>
        /// Removes all elements from the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.heap.Clear();

        /// <summary>
        /// Adds an element to the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <param name="item">The element to add to the <see cref="PriorityQueue{T}"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item) => this.heap.Push(item);

        /// <summary>
        /// Removes and returns the element at the beginning of the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <returns>
        /// The element with the highest priority that is removed from the beginning of the <see cref="PriorityQueue{T}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="PriorityQueue{T}"/> is empty.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue() => this.heap.Pop();
    }
}
