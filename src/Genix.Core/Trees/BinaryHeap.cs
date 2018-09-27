// -----------------------------------------------------------------------
// <copyright file="BinaryHeap.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the binary heap.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the "heap" property.</typeparam>
    /// <typeparam name="TValue">Specifies the type of elements in the heap.</typeparam>
    /// <remarks>
    /// <each>
    /// Each element in the heap satisfies the "heap" property condition that it smaller or equal to all its children.
    /// </each>
    /// </remarks>
    public class BinaryHeap<TKey, TValue>
        : IHeap<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly int maximumCapacity;
        private TKey maximumKey = default(TKey);
        private (TKey key, TValue value)[] heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryHeap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial heap capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryHeap(int initialCapacity)
            : this(initialCapacity, int.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryHeap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial heap capacity.</param>
        /// <param name="maximumCapacity">The maximum recommended number of elements in the heap. The actual number of elements might be larger.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryHeap(int initialCapacity, int maximumCapacity)
        {
            this.heap = new (TKey, TValue)[initialCapacity];
            this.maximumCapacity = maximumCapacity;
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.Count = 0;

        /// <inheritdoc />
        public bool Push(TKey key, TValue value)
        {
            bool updateMaximumKey = false;

            // compare element key with maximum key in the heap
            // if it is greater then update the key if the heap is smaller than allowed; otherwise, quit
            if (this.Count == 0)
            {
                this.maximumKey = key;
            }
            else if (key.CompareTo(this.maximumKey) >= 0)
            {
                if (this.Count >= this.maximumCapacity)
                {
                    return false;
                }

                this.maximumKey = key;
            }
            else if (this.Count >= this.maximumCapacity)
            {
                // remove element with maximum key from the heap
                // it should be among elements that do not parents
                int index = this.FindMaximumKeyIndex();
                if (index != -1)
                {
                    if (index != this.Count - 1)
                    {
                        // insert last element into the position of element with maximum key
                        this.Insert(index, this.heap[this.Count - 1]);
                    }

                    this.Count--;
                }

                updateMaximumKey = true;
            }

            // resize the heap if necessary
            if (this.Count == this.heap.Length)
            {
                Array.Resize(ref this.heap, this.Count * 2);
            }

            // traverse the tree up starting from last (bottom-right) element
            // at each step we compare value with its parent and
            // exchange them if the value is better
            this.Insert(this.Count, (key, value));
            this.Count++;

            // find new maximum key if element with maximum key was removed from the heap
            if (updateMaximumKey)
            {
                this.FindMaximumKey(out this.maximumKey);
            }

            return true;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="BinaryHeap{TKey, TValue}"/> is empty.
        /// </exception>
        public (TKey key, TValue value) Pop()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            int count = this.Count;
            (TKey key, TValue value)[] h = this.heap;

            (TKey key, TValue value) top = h[0];
            if (count > 1)
            {
                // traverse the tree down starting from top element
                // at each step we exchange parent with its best child
                int parent = 0;
                int left = BinaryHeap<TKey, TValue>.LeftChild(parent);

                while (left < count)
                {
                    int right = BinaryHeap<TKey, TValue>.RightFromLeft(left);
                    int best = (right < count && h[right].key.CompareTo(h[left].key) < 0) ? right : left;

                    // move bestChild up to fill the gap left by parent
                    h[parent] = h[best];

                    // the gap left by best becomes a parent for next iteration
                    parent = best;
                    left = BinaryHeap<TKey, TValue>.LeftChild(parent);
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

            if (--this.Count == 0)
            {
                // reset maximum key if last element was removed
                this.maximumKey = default(TKey);
            }

            return top;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="BinaryHeap{TKey, TValue}"/> is empty.
        /// </exception>
        public (TKey key, TValue value) Peek()
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
        private static int Parent(int child) => (child - 1) / 2;

        /// <summary>
        /// Returns the left child index of the specified parent element.
        /// </summary>
        /// <returns>
        /// The zero-based index of left child of <paramref name="parent"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LeftChild(int parent) => (parent * 2) + 1;

        /// <summary>
        /// Returns the right sibling index of the specified left child element.
        /// </summary>
        /// <returns>
        /// The zero-based index of right sibling of left <paramref name="child"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RightFromLeft(int child) => child + 1;

        /// <summary>
        /// Inserts new element along the path starting at the bottom <paramref name="index"/> position.
        /// </summary>
        /// <param name="index">The starting insertion position.</param>
        /// <param name="item">The element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, (TKey key, TValue value) item)
        {
            (TKey key, TValue value)[] h = this.heap;

            // we start traversing the tree from the element at index position
            // at each step we compare the parent element of the element at index with value
            // and move parent element down if value is better
            while (index > 0)
            {
                int parent = BinaryHeap<TKey, TValue>.Parent(index);
                if (item.key.CompareTo(h[parent].key) < 0)
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

            h[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindMaximumKeyIndex()
        {
            (TKey key, TValue value)[] h = this.heap;

            int ii = this.Count - 1;
            int i = BinaryHeap<TKey, TValue>.Parent(ii) + 1;

            for (; i <= ii; i++)
            {
                if (this.maximumKey.CompareTo(h[i].key) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindMaximumKey(out TKey max)
        {
            (TKey key, TValue value)[] h = this.heap;

            int ii = this.Count - 1;
            int i = BinaryHeap<TKey, TValue>.Parent(ii) + 1;

            int win = i;
            max = h[i].key;

            while (++i <= ii)
            {
                if (max.CompareTo(h[i].key) > 0)
                {
                    win = i;
                    max = h[i].key;
                }
            }

            return win;
        }
    }
}
