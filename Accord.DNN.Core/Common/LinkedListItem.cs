// -----------------------------------------------------------------------
// <copyright file="LinkedListItem.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    /// <summary>
    /// Represents a node in a <see cref="LinkedList{T}"/>. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">Specifies the item type of the linked list.</typeparam>
    internal abstract class LinkedListItem<T>
    {
        /// <summary>
        /// Gets or sets the previous item in the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <value>
        /// The previous item in the <see cref="LinkedList{T}"/>.
        /// </value>
        public T Prev { get; internal set; }

        /// <summary>
        /// Gets or sets the next item in the <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <value>
        /// The next item in the <see cref="LinkedList{T}"/>.
        /// </value>
        public T Next { get; internal set; }
    }
}
