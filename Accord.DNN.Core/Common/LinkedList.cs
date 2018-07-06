// -----------------------------------------------------------------------
// <copyright file="LinkedList.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a doubly linked list.
    /// </summary>
    /// <typeparam name="T">Specifies the item type of the linked list.</typeparam>
    internal class LinkedList<T> : IEnumerable<T> where T : LinkedListItem<T>
    {
        /// <summary>
        /// The number of items in the list.
        /// </summary>
        private int count;

        /// <summary>
        /// The head item.
        /// </summary>
        private T head;

        /// <summary>
        /// The tail item.
        /// </summary>
        private T tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedList{T}"/> class that is empty.
        /// </summary>
        public LinkedList()
        {
        }

        /// <summary>
        /// Gets the number of items actually contained in the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <value>
        /// The number of items actually contained in the <see cref="LinkedList{T}"/>.
        /// </value>
        public int Count => this.count;

        /// <summary>
        /// Gets the head item of the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <value>
        /// The head item of the <see cref="LinkedList{T}"/>.
        /// </value>
        public T Head => this.head;

        /// <summary>
        /// Gets the tail item of the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <value>
        /// The head tail of the <see cref="LinkedList{T}"/>.
        /// </value>
        public T Tail => this.tail;

        /// <summary>
        /// Removes all items from the <see cref="LinkedList{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            this.head = null;
            this.tail = null;
            this.count = 0;
        }

        /// <summary>
        /// Adds a new item to the head of the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHead(T item)
        {
            item.Prev = null;
            item.Next = this.head;

            if (this.head != null)
            {
                this.head.Prev = item;
            }

            this.head = item;
            this.count++;
        }

        /// <summary>
        /// Adds a new item to the tail of the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTail(T item)
        {
            item.Prev = this.tail;
            item.Next = null;

            if (this.tail != null)
            {
                this.tail.Next = item;
            }

            this.tail = item;
            this.count++;
        }

        /// <summary>
        /// Adds the specified item before the specified existing item in the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="item">The item before which to add <c>newNode</c>.</param>
        /// <param name="newItem">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBefore(T item, T newItem)
        {
            T prev = item.Prev;

            newItem.Prev = prev;
            newItem.Next = item;

            item.Prev = newItem;
            if (prev != null)
            {
                prev.Next = newItem;
            }
            else
            {
                this.head = newItem;
            }

            this.count++;
        }

        /// <summary>
        /// Adds the specified item after the specified existing item in the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="item">The item after which to add <c>newNode</c>.</param>
        /// <param name="newItem">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAfter(T item, T newItem)
        {
            T next = item.Next;

            newItem.Prev = item;
            newItem.Next = next;

            item.Next = newItem;
            if (next != null)
            {
                next.Prev = newItem;
            }
            else
            {
                this.tail = newItem;
            }

            this.count++;
        }

        /// <summary>
        /// Removes the head item from the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <returns>
        /// The item removed from the list.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RemoveHead()
        {
            T item = this.head;
            if (item != null)
            {
                T next = item.Next;
                if (next != null)
                {
                    next.Prev = null;
                }

                this.head = next;
                this.count--;

                item.Next = null;
            }

            return item;
        }

        /// <summary>
        /// Removes the tail item from the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <returns>
        /// The item removed from the list.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RemoveTail()
        {
            T item = this.tail;
            if (item != null)
            {
                T prev = item.Prev;
                if (prev != null)
                {
                    prev.Next = null;
                }

                this.tail = prev;
                this.count--;

                item.Prev = null;
            }

            return item;
        }

        /// <summary>
        /// Removes the specified item from the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item)
        {
            if (this.head == item)
            {
                this.head = item.Next;
            }

            if (this.tail == item)
            {
                this.tail = item.Prev;
            }

            if (item.Prev != null)
            {
                item.Prev.Next = item.Next;
            }

            if (item.Next != null)
            {
                item.Next.Prev = item.Prev;
            }

            item.Next = null;
            item.Prev = null;

            this.count--;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct Enumerator : IEnumerator<T>
        {
            private readonly LinkedList<T> list;
            private T item;
            private T current;

            public Enumerator(LinkedList<T> list)
            {
                this.list = list;
                this.item = list.head;
                this.current = default(T);
            }

            public T Current => this.current;

            object IEnumerator.Current => this.current;

            bool IEnumerator.MoveNext()
            {
                if (this.item == null)
                {
                    return false;
                }

                this.current = this.item;
                this.item = this.item.Next;
                return true;
            }

            void IEnumerator.Reset()
            {
                this.item = this.list.head;
                this.current = default(T);
            }

            public void Dispose()
            {
            }
        }
    }
}
