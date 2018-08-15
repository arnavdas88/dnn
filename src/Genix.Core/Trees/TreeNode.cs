// -----------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a node of the <see cref="Tree{T, TNode}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value the node stores.</typeparam>
    public class TreeNode<T>
    {
        private List<TreeNode<T>> children = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
        /// </summary>
        public TreeNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
        /// </summary>
        /// <param name="value">The node value.</param>
        public TreeNode(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
        /// </summary>
        /// <param name="value">The node value.</param>
        /// <param name="parent">The node parent.</param>
        public TreeNode(T value, TreeNode<T> parent)
        {
            this.Value = value;
            this.Parent = parent;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <value>
        /// The node value.
        /// </value>
        public T Value { get; internal set; }

        /// <summary>
        /// Gets the node parent.
        /// </summary>
        /// <value>
        /// The node parent.
        /// </value>
        public TreeNode<T> Parent { get; internal set; }

        /// <summary>
        /// Gets the node children.
        /// </summary>
        /// <value>
        /// The node children.
        /// </value>
        public IList<TreeNode<T>> Children => this.children;

        /// <summary>
        /// Gets the number of node children.
        /// </summary>
        /// <value>
        /// The number of node children.
        /// </value>
        public int Rank => this.children?.Count ?? 0;

        /// <summary>
        /// Adds a new value to this <see cref="TreeNode{T}"/>.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>
        /// The <see cref="TreeNode{T}"/> that contains added value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNode<T> Add(T value)
        {
            TreeNode<T> node = new TreeNode<T>(value);
            this.children.Add(node);
            return node;
        }

        /// <summary>
        /// Adds the specified <see cref="TreeNode{T}"/> to this <see cref="TreeNode{T}"/>.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode{T}"/> to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TreeNode<T> node)
        {
            if (this.children == null)
            {
                this.children = new List<TreeNode<T>>();
            }

            node.Parent = this;
            this.children.Add(node);
        }
    }
}
