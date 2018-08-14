// -----------------------------------------------------------------------
// <copyright file="BinaryHeap.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the binary heap.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the heap.</typeparam>
    /// <remarks>
    /// <each>
    /// Each element in the heap satisfies the "heap" property condition that it smaller or equal to all its children.
    /// </each>
    /// </remarks>
    public class BinaryHeap<T>
        : IHeap<T>
        where T : IComparable<T>
    {
        private T[] heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryHeap{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial heap capacity.</param>
        public BinaryHeap(int capacity)
        {
            this.heap = new T[capacity];
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.Count = 0;

        /// <inheritdoc />
        public void Push(T value)
        {
            // resize the heap if necessary
            if (this.Count == this.heap.Length)
            {
                Array.Resize(ref this.heap, this.Count * 2);
            }

            // traverse the tree up starting from last (bottom-right) element
            // at each step we compare value with its parent and
            // exchange them if the value is better
            this.Insert(this.Count, value);
            this.Count++;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="BinaryHeap{T}"/> is empty.
        /// </exception>
        public T Pop()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            int count = this.Count;
            T[] h = this.heap;

            T top = h[0];
            if (count > 1)
            {
                // traverse the tree down starting from top element
                // at each step we exchange parent with its best child
                int parent = 0;
                int left = BinaryHeap<T>.LeftChild(parent);

                while (left < count)
                {
                    int right = BinaryHeap<T>.RightFromLeft(left);
                    int best = (right < count && h[right].CompareTo(h[left]) < 0) ? right : left;

                    // move bestChild up to fill the gap left by parent
                    h[parent] = h[best];

                    // the gap left by best becomes a parent for next iteration
                    parent = best;
                    left = BinaryHeap<T>.LeftChild(parent);
                }

                // move the last (bottom-right) element to the last gap
                if (parent != count - 1)
                {
                    // traverse the tree up starting from last gap
                    // at each step we compare bottom-right element with the path parent and
                    // exchange parent and child along the path if the bottom-right element is better
                    // finally, we fill the gap with bottom-right element
                    this.Insert(parent, h[count - 1]);
                }
            }

            this.Count--;
            return top;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="BinaryHeap{T}"/> is empty.
        /// </exception>
        public T Peek()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            return this.heap[0];
        }

        /// <summary>
        /// Returns the parent index of the specified child element.
        /// </summary>
        /// <returns>
        /// The zero-based index of parent of <paramref name="child"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Parent(int child)
        {
            return (child - 1) / 2;
        }

        /// <summary>
        /// Returns the left child index of the specified parent element.
        /// </summary>
        /// <returns>
        /// The zero-based index of left child of <paramref name="parent"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LeftChild(int parent)
        {
            return (parent * 2) + 1;
        }

        /// <summary>
        /// Returns the right sibling index of the specified left child element.
        /// </summary>
        /// <returns>
        /// The zero-based index of right sibling of left <paramref name="child"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RightFromLeft(int child)
        {
            return child + 1;
        }

        /// <summary>
        /// Inserts new element along the path starting at the bottom <paramref name="index"/> position.
        /// </summary>
        /// <param name="index">The starting insertion position.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, T value)
        {
            T[] h = this.heap;

            // we start traversing the tree from the element at index position
            // at each step we compare the parent element of the element at index with value
            // and move parent element down if value is better
            while (index > 0)
            {
                int parent = BinaryHeap<T>.Parent(index);
                if (value.CompareTo(h[parent]) < 0)
                {
                    // value is better than parent - exchange places
                    h[index] = h[parent];
                    index = parent;
                }
                else
                {
                    // insert here
                    break;
                }
            }

            h[index] = value;
        }
    }
}
