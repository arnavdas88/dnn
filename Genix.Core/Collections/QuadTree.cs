// -----------------------------------------------------------------------
// <copyright file="QuadTree.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;

    /// <summary>
    /// This class efficiently stores and retrieves arbitrarily sized and positioned
    /// objects in a quad-tree data structure.  This can be used to do efficient hit
    /// detection or visibility checks on objects in a virtualized canvas.
    /// The object does not need to implement any special interface because the Rect Bounds
    /// of those objects is handled as a separate argument to Insert.
    /// </summary>
    /// <typeparam name="T">The type of elements in the tree.</typeparam>
    public class QuadTree<T> where T : class, IBoundedObject
    {
        private readonly Point center;

        private readonly int minWidth;
        private readonly int minHeight;
        private readonly bool splittable;

        // nodes that overlap the sub quadrant boundaries.
        private HashSet<T> ownNodes = null;
        private HashSet<T> nodes = null;

        // The quadrant is subdivided when nodes are inserted that are 
        // completely contained within those subdivisions.
        private QuadTree<T> nw;
        private QuadTree<T> ne;
        private QuadTree<T> sw;
        private QuadTree<T> se;
        private bool split = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadTree{T}"/> class, using the specified bounds.
        /// All nodes stored inside this tree will fit inside this bounds.
        /// </summary>
        /// <param name="bounds">The bounds of this tree.</param>
        public QuadTree(Rectangle bounds) : this(null, bounds, 64, 64)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadTree{T}"/> class,
        /// using the specified bounds and a collection of nodes.
        /// All nodes stored inside this tree will fit inside this bounds.
        /// </summary>
        /// <param name="bounds">The bounds of this tree.</param>
        /// <param name="nodes">The nodes to insert.</param>
        public QuadTree(Rectangle bounds, IEnumerable<T> nodes) : this(bounds)
        {
            this.Insert(nodes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadTree{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="QuadTree{T}"/> (if any).</param>
        /// <param name="bounds">The bounds of this tree.</param>
        /// <param name="minWidth">The minimum width of a quadrant that allows splitting.</param>
        /// <param name="minHeight">The minimum height of a quadrant that allows splitting.</param>
        internal QuadTree(QuadTree<T> parent, Rectangle bounds, int minWidth, int minHeight)
        {
            this.Parent = parent;
            Debug.Assert(bounds.Width != 0 && bounds.Height != 0, "Cannot have empty bound");

            this.Bounds = bounds;
            this.center = new Point((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
            this.minWidth = minWidth;
            this.minHeight = minHeight;
            this.splittable = bounds.Width >= minWidth && bounds.Height >= minHeight;
        }

        /// <summary>
        /// Gets the parent <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <value>
        /// The parent <see cref="QuadTree{T}"/>, or <b>null</b> if this is the root.
        /// </value>
        public QuadTree<T> Parent { get; }

        /// <summary>
        /// Gets the bounds of this <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rectangle"/> that contains overall tree bounds.
        /// </value>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Inserts a node into this <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        public void Insert(T node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Rectangle bounds = node.Bounds;
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                ////throw new ArgumentException(Properties.Resources.BoundsMustBeNonZero);
            }

            // check that new node does not cover quadrant center
            // otherwise, store it into quadrant own nodes
            if (!this.splittable ||
                (bounds.X < this.center.X && this.center.X < bounds.Right) ||
                (bounds.Y < this.center.Y && this.center.Y < bounds.Bottom))
            {
                if (this.ownNodes == null)
                {
                    this.ownNodes = new HashSet<T>();
                }

                this.ownNodes.Add(node);
            }
            else
            {
                if (this.splittable)
                {
                    if (this.nodes?.Count + 1 > 16)
                    {
                        // split the quadrant
                        foreach (T n in this.nodes)
                        {
                            this.InsertIntoTree(n);
                        }

                        this.nodes = null;
                        this.split = true;
                    }

                    if (this.split)
                    {
                        this.InsertIntoTree(node);
                        return;
                    }
                }

                // add to this quadrant
                if (this.nodes == null)
                {
                    this.nodes = new HashSet<T>();
                }

                this.nodes.Add(node);
            }
        }

        /// <summary>
        /// Inserts a range of nodes into this <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <param name="nodes">The nodes to insert.</param>
        public void Insert(IEnumerable<T> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            foreach (T node in nodes)
            { 
                this.Insert(node);
            }
        }

        /// <summary>
        /// Remove the specified node from this  <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns>
        /// <b>true</b> if the node was found and removed; otherwise, <b>false</b>.
        /// </returns>
        public bool Remove(T node)
        {
            // look in own nodes first
            if (this.ownNodes != null && this.ownNodes.Remove(node))
            {
                return true;
            }

            if (this.nodes != null)
            {
                return this.nodes.Remove(node);
            }
            else
            {
                QuadTree<T> quadrant;

                Rectangle bounds = node.Bounds;
                if (bounds.X < this.center.X)
                {
                    quadrant = bounds.Y < this.center.Y ? this.nw : this.sw;
                }
                else
                {
                    quadrant = bounds.Y < this.center.Y ? this.ne : this.se;
                }

                return quadrant != null && quadrant.Remove(node);
            }
        }

        /// <summary>
        /// Returns all nodes in this <see cref="QuadTree{T}"/>.
        /// The nodes are returned in pretty much random order as far as the caller is concerned.
        /// </summary>
        /// <returns>
        /// The sequence of nodes.
        /// </returns>
        public IEnumerable<T> GetNodes()
        {
            if (this.ownNodes != null)
            {
                foreach (T node in this.ownNodes)
                {
                    yield return node;
                }
            }

            if (this.nodes != null)
            {
                foreach (T node in this.nodes)
                {
                    yield return node;
                }
            }
            else
            {
                // See if any child quadrants completely contain this node.
                if (this.nw != null)
                {
                    foreach (T node in this.nw.GetNodes())
                    {
                        yield return node;
                    }
                }

                if (this.ne != null)
                {
                    foreach (T node in this.ne.GetNodes())
                    {
                        yield return node;
                    }
                }

                if (this.se != null)
                {
                    foreach (T node in this.se.GetNodes())
                    {
                        yield return node;
                    }
                }

                if (this.sw != null)
                {
                    foreach (T node in this.sw.GetNodes())
                    {
                        yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// Returns all nodes in this <see cref="QuadTree{T}"/> that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// The sequence of nodes found inside the specified bounds.
        /// </returns>
        public IEnumerable<T> GetNodes(Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                yield break;
            }

            if (this.ownNodes != null)
            {
                foreach (T node in this.ownNodes)
                {
                    if (node.Bounds.IntersectsWith(bounds))
                    {
                        yield return node;
                    }
                }
            }

            if (this.nodes != null)
            {
                foreach (T node in this.nodes)
                {
                    if (node.Bounds.IntersectsWith(bounds))
                    {
                        yield return node;
                    }
                }
            }
            else
            {
                // See if any child quadrants completely contain this node.
                if (this.nw != null && this.nw.Bounds.IntersectsWith(bounds))
                {
                    foreach (T node in this.nw.GetNodes(bounds))
                    {
                        yield return node;
                    }
                }

                if (this.ne != null && this.ne.Bounds.IntersectsWith(bounds))
                {
                    foreach (T node in this.ne.GetNodes(bounds))
                    {
                        yield return node;
                    }
                }

                if (this.se != null && this.se.Bounds.IntersectsWith(bounds))
                {
                    foreach (T node in this.se.GetNodes(bounds))
                    {
                        yield return node;
                    }
                }

                if (this.sw != null && this.sw.Bounds.IntersectsWith(bounds))
                {
                    foreach (T node in this.sw.GetNodes(bounds))
                    {
                        yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the this <see cref="QuadTree{T}"/> has nodes that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// <b>true</b> if this <see cref="QuadTree{T}"/> has nodes that intersect the specified bounds; otherwise, <b>false</b>.
        /// </returns>
        public bool HasNodes(Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                return false;
            }

            if (this.ownNodes != null)
            {
                foreach (T node in this.ownNodes)
                {
                    if (node.Bounds.IntersectsWith(bounds))
                    {
                        return true;
                    }
                }
            }

            if (this.nodes != null)
            {
                foreach (T node in this.nodes)
                {
                    if (node.Bounds.IntersectsWith(bounds))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                bool found = false;

                // See if any child quadrants completely contain this node.
                if (this.nw != null && this.nw.Bounds.IntersectsWith(bounds))
                {
                    found = this.nw.HasNodes(bounds);
                }

                if (!found && this.ne != null && this.ne.Bounds.IntersectsWith(bounds))
                {
                    found = this.ne.HasNodes(bounds);
                }

                if (!found && this.se != null && this.se.Bounds.IntersectsWith(bounds))
                {
                    found = this.se.HasNodes(bounds);
                }

                if (!found && this.sw != null && this.sw.Bounds.IntersectsWith(bounds))
                {
                    found = this.sw.HasNodes(bounds);
                }

                return found;
            }
        }

        private void InsertIntoTree(T node)
        {
            Rectangle bounds = node.Bounds;

            if (bounds.X < this.center.X)
            {
                if (bounds.Y < this.center.Y)
                {
                    // top-left quadrant
                    if (this.nw == null)
                    {
                        this.nw = new QuadTree<T>(
                            this,
                            Rectangle.FromLTRB(this.Bounds.Left, this.Bounds.Top, this.center.X, this.center.Y),
                            this.minWidth,
                            this.minHeight);
                    }

                    this.nw.Insert(node);
                }
                else
                {
                    // bottom-left quadrant
                    if (this.sw == null)
                    {
                        this.sw = new QuadTree<T>(
                            this,
                            Rectangle.FromLTRB(this.Bounds.Left, this.center.Y, this.center.X, this.Bounds.Bottom),
                            this.minWidth,
                            this.minHeight);
                    }

                    this.sw.Insert(node);
                }
            }
            else
            {
                if (bounds.Y < this.center.Y)
                {
                    // top-right quadrant
                    if (this.ne == null)
                    {
                        this.ne = new QuadTree<T>(
                            this,
                            Rectangle.FromLTRB(this.center.X, this.Bounds.Top, this.Bounds.Right, this.center.Y),
                            this.minWidth,
                            this.minHeight);
                    }

                    this.ne.Insert(node);
                }
                else
                {
                    // bottom-right quadrant
                    if (this.se == null)
                    {
                        this.se = new QuadTree<T>(
                            this,
                            Rectangle.FromLTRB(this.center.X, this.center.Y, this.Bounds.Right, this.Bounds.Bottom),
                            this.minWidth,
                            this.minHeight);
                    }

                    this.se.Insert(node);
                }
            }

            this.split = true;
        }
    }
}