// -----------------------------------------------------------------------
// <copyright file="LinkedCollectionItem.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a node in a <see cref="LinkedCollection{T}"/>. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">Specifies the item type of the linked list.</typeparam>
    public abstract class LinkedCollectionItem<T>
    {
        /// <summary>
        /// Gets the previous item in the <see cref="LinkedCollection{T}"/>.
        /// </summary>
        /// <value>
        /// The previous item in the <see cref="LinkedCollection{T}"/>.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Prev", Justification = "The abbreviation for previous to keep in sync with Next.")]
        public T Prev { get; internal set; }

        /// <summary>
        /// Gets the next item in the <see cref="LinkedCollection{T}"/>.
        /// </summary>
        /// <value>
        /// The next item in the <see cref="LinkedCollection{T}"/>.
        /// </value>
        public T Next { get; internal set; }
    }
}
