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
    /// <typeparam name="TNode">The type tree node.</typeparam>
    public class Tree<T, TNode>
        where TNode : TreeNode<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T, TNode}"/> class.
        /// </summary>
        public Tree()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T, TNode}"/> class.
        /// </summary>
        /// <param name="rootItem">The root item of the tree.</param>
        public Tree(T rootItem)
        {
            this.Root = new TNode
            {
                Value = rootItem,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T, TNode}"/> class.
        /// </summary>
        /// <param name="node">The new root node of the tree.</param>
        public Tree(TNode node)
        {
            this.Root = node;
            this.Root.Parent = null;
        }

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        /// <value>
        /// The <see cref="TreeNode{T}"/> object that holds the root value.
        /// </value>
        public TNode Root { get; private set; }

        /// <summary>
        /// Gets the item in the root node of the tree.
        /// </summary>
        /// <value>
        /// The root node item.
        /// </value>
        public T RootItem => this.Root.Value;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Tree{T, TNode}"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="Tree{T, TNode}"/>.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Removes all elements from the <see cref="Tree{T, TNode}"/>.
        /// </summary>
        public void Clear()
        {
            this.Root = null;
            this.Count = 0;
        }
    }
}
