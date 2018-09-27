// -----------------------------------------------------------------------
// <copyright file="FibonacciHeap.cs" company="Noname, Inc.">
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
    /// Represents the Fibonacci heap.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the "heap" property.</typeparam>
    /// <typeparam name="TValue">Specifies the type of elements in the heap.</typeparam>
    /// <remarks>
    /// <each>
    /// Each element in the heap satisfies the "heap" property condition that it smaller or equal to all its children.
    /// </each>
    /// </remarks>
    public class FibonacciHeap<TKey, TValue>
        : IHeap<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly List<FibonacciTree> heaps = new List<FibonacciTree>(100);
        private int minimumHeapIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="FibonacciHeap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="capacity">The initial heap capacity.</param>
        public FibonacciHeap(int capacity)
        {
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public void Clear()
        {
            this.heaps.Clear();
            this.Count = 0;
            this.minimumHeapIndex = -1;
        }

        /// <inheritdoc />
        public bool Push(TKey key, TValue value)
        {
            // to push an element into the heap
            // we create a new heap containing value
            FibonacciTree newheap = new FibonacciTree((key, value));
            this.heaps.Add(newheap);
            this.Count++;

            // now we update position of current min element
            if (this.minimumHeapIndex == -1 || key.CompareTo(this.heaps[this.minimumHeapIndex].RootItem.key) < 0)
            {
                this.minimumHeapIndex = this.heaps.Count - 1;
            }

            return true;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="FibonacciHeap{TKey, TValue}"/> is empty.
        /// </exception>
        public (TKey key, TValue value) Pop()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            // the minimum element is always the root of the first heap
            FibonacciTreeNode node = this.heaps[this.minimumHeapIndex].Root;
            (TKey key, TValue value) value = node.Value;

            // 1. remove min element from the heap
            // push all its children into the list of heaps
            this.heaps.RemoveAt(this.minimumHeapIndex);
            this.Count--;

            if (node.Rank > 0)
            {
                for (int i = 0, ii = node.Children.Count; i < ii; i++)
                {
                    this.heaps.Add(new FibonacciTree(node.Children[i] as FibonacciTreeNode));
                }
            }

            // 2. link heaps with the same rank together
            FibonacciTree.Link(this.heaps);

            // 3 update the position of new min element
            if (this.heaps.Count == 0)
            {
                this.minimumHeapIndex = -1;
            }
            else
            {
                this.minimumHeapIndex = 0;

                for (int i = 1, ii = this.heaps.Count; i < ii; i++)
                {
                    if (this.heaps[i].RootItem.key.CompareTo(this.heaps[this.minimumHeapIndex].RootItem.key) < 0)
                    {
                        this.minimumHeapIndex = i;
                    }
                }
            }

            return value;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// The <see cref="FibonacciHeap{TKey, TValue}"/> is empty.
        /// </exception>
        public (TKey key, TValue value) Peek()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            // the minimum element is at the position found previously
            return this.heaps[this.minimumHeapIndex].RootItem;
        }

        private class FibonacciTree
            : Tree<(TKey key, TValue value), FibonacciTreeNode>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FibonacciTree"/> class.
            /// </summary>
            public FibonacciTree()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FibonacciTree"/> class.
            /// </summary>
            /// <param name="rootItem">The root value of the tree.</param>
            public FibonacciTree((TKey key, TValue value) rootItem)
                : base(rootItem)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FibonacciTree"/> class.
            /// </summary>
            /// <param name="node">The new root node of the tree.</param>
            public FibonacciTree(FibonacciTreeNode node)
                : base(node)
            {
            }

            /// <summary>
            /// Merges two trees.
            /// </summary>
            /// <param name="tree1">The first tree to merge.</param>
            /// <param name="tree2">The second tree to merge.</param>
            /// <returns>
            /// The merged tree. Could be either <paramref name="tree1"/> or <paramref name="tree2"/>.
            /// </returns>
            /// <remarks>
            /// The tree with its root element smaller than root element of another tree
            /// becomes the child of root element of another tree.
            /// </remarks>
            public static FibonacciTree Merge(FibonacciTree tree1, FibonacciTree tree2)
            {
                if (tree1.RootItem.key.CompareTo(tree2.RootItem.key) < 0)
                {
                    tree1.Root.Add(tree2.Root);
                    return tree1;
                }
                else
                {
                    tree2.Root.Add(tree1.Root);
                    return tree2;
                }
            }

            /// <summary>
            /// Link trees together.
            /// </summary>
            /// <param name="trees">The trees to link.</param>
            /// <remarks>
            /// We keep finding any two trees whose roots have the same rank and merge them.
            /// </remarks>
            public static void Link(List<FibonacciTree> trees)
            {
                for (int i = 0, ii = trees.Count; i + 1 < ii; i++)
                {
                    FibonacciTreeNode inode = trees[i].Root;
                    int irank = inode.Rank;

                    for (int j = i + 1; j < ii; j++)
                    {
                        FibonacciTreeNode jnode = trees[j].Root;

                        if (irank == jnode.Rank)
                        {
                            if (inode.Value.key.CompareTo(jnode.Value.key) < 0)
                            {
                                inode.Add(jnode);
                                trees.RemoveAt(j);
                            }
                            else
                            {
                                jnode.Add(inode);
                                trees.RemoveAt(i);
                            }

                            // restart previous search
                            // we do not need to look at elements before i
                            // as the tree ranks only increases
                            ii--;
                            i--;
                            break;
                        }
                    }
                }
            }
        }

        private class FibonacciTreeNode
            : TreeNode<(TKey key, TValue value)>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FibonacciTreeNode"/> class.
            /// </summary>
            public FibonacciTreeNode()
            {
            }

            public bool IsMarked { get; private set; }
        }
    }
}
