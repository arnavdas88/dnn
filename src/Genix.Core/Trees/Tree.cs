// -----------------------------------------------------------------------
// <copyright file="Tree.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Represents the tree each node of which can contain arbitrary number of other nodes.
    /// </summary>
    /// <typeparam name="T">The type of values the tree stores.</typeparam>
    public class Tree<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T}"/> class.
        /// </summary>
        public Tree()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T}"/> class.
        /// </summary>
        /// <param name="rootValue">The root value of the tree.</param>
        public Tree(T rootValue)
        {
            this.Root = new TreeNode<T>(rootValue);
        }

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        /// <value>
        /// The <see cref="TreeNode{T}"/> object that holds the root value.
        /// </value>
        public TreeNode<T> Root { get; private set; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Tree{T}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="Tree{T}"/>.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Removes all elements from the <see cref="Tree{T}"/>.
        /// </summary>
        public void Clear()
        {
            this.Root = null;
            this.Count = 0;
        }
    }
}
