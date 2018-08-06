// -----------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a node of the <see cref="Tree{T}"/>.
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
        public T Value { get; }

        /// <summary>
        /// Gets the node parent.
        /// </summary>
        /// <value>
        /// The node parent.
        /// </value>
        public TreeNode<T> Parent { get; private set; }

        /// <summary>
        /// Gets the node children.
        /// </summary>
        /// <value>
        /// The node children.
        /// </value>
        public IList<TreeNode<T>> Children => this.children;

        /// <summary>
        /// Adds a new value to this <see cref="TreeNode{T}"/>.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>
        /// The <see cref="TreeNode{T}"/> that contains added value.
        /// </returns>
        public TreeNode<T> Add(T value)
        {
            if (this.children == null)
            {
                this.children = new List<TreeNode<T>>();
            }

            TreeNode<T> node = new TreeNode<T>(value, this);
            this.children.Add(node);
            return node;
        }

        /// <summary>
        /// Adds the specified <see cref="TreeNode{T}"/> to this <see cref="TreeNode{T}"/>.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode{T}"/> to add.</param>
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
