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
    using System.Linq;

    /// <summary>
    /// This class efficiently stores and retrieves arbitrarily sized and positioned
    /// objects in a quad-tree data structure.  This can be used to do efficient hit
    /// detection or visibility checks on objects in a virtualized canvas.
    /// The object does not need to implement any special interface because the Rect Bounds
    /// of those objects is handled as a separate argument to Insert.
    /// </summary>
    /// <typeparam name="T">The type of elements in the tree.</typeparam>
    public class QuadTree<T> where T : class
    {
        ////private readonly IDictionary<T, Quadrant> table = new Dictionary<T, Quadrant>();
        private readonly Quadrant root;

        public QuadTree(Rectangle bounds)
        {
            this.root = new Quadrant(null, bounds);
        }

        /// <summary>
        /// Gets the tree bounds.
        /// </summary>
        /// <value>
        /// The overall tree bounds.
        /// </value>
        /// <remarks>
        /// This determines the overall quad-tree indexing strategy.
        /// </remarks>
        public Rectangle Bounds => this.root.Bounds;

        /// <summary>
        /// Insert a node with specified bounds into this QuadTree.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        /// <param name="bounds">The bounds of this node.</param>
        public void Insert(T node, Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                ////throw new ArgumentException(Properties.Resources.BoundsMustBeNonZero);
            }

            /*this.table[node] =*/
            this.root.Insert(node, bounds);
        }

        /// <summary>
        /// Returns all nodes in this tree.
        /// The nodes are returned in pretty much random order as far as the caller is concerned.
        /// </summary>
        /// <returns>
        /// The sequence of nodes.
        /// </returns>
        public IEnumerable<T> GetNodes()
        {
            return this.root.GetNodes();
        }

        /// <summary>
        /// Get a list of the nodes that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// List of zero or mode nodes found inside the specified bounds.
        /// </returns>
        public IEnumerable<T> GetNodes(Rectangle bounds)
        {
            return bounds.IsEmpty ? Enumerable.Empty<T>() : this.root.GetNodes(bounds);
        }

        /// <summary>
        /// Get a list of the nodes that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// List of zero or mode nodes found inside the specified bounds.
        /// </returns>
        public bool HasNodes(Rectangle bounds)
        {
            return !bounds.IsEmpty && this.root.HasNodes(bounds);
        }

        /*
        /// <summary>
        /// Remove the specified node from this QuadTree.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns>
        /// <b>true</b> if the node was found and removed; otherwise, <b>false</b>.
        /// </returns>
        public bool Remove(T node)
        {
            if (this.table != null)
            {
                Quadrant parent = null;

                if (this.table.TryGetValue(node, out parent))
                {
                    parent.RemoveNode(node);
                    this.table.Remove(node);
                    return true;
                }
            }

            return false;
        }*/

        /// <summary>
        /// The canvas is split up into four Quadrants and objects are stored in the quadrant that contains them
        /// and each quadrant is split up into four child Quadrants recursively.  Objects that overlap more than
        /// one quadrant are stored in the this.nodes list for this Quadrant.
        /// </summary>
        private class Quadrant
        {
            // nodes that overlap the sub quadrant boundaries.
            private Dictionary<T, Rectangle> nodes = null;

            private readonly int centerX;
            private readonly int centerY;
            private readonly bool splittable;

            // The quadrant is subdivided when nodes are inserted that are 
            // completely contained within those subdivisions.
            private Quadrant nw;
            private Quadrant ne;
            private Quadrant sw;
            private Quadrant se;
            private bool split = false;

            /// <summary>
            /// Construct new Quadrant with a specified bounds all nodes stored inside this quadrant
            /// will fit inside this bounds.  
            /// </summary>
            /// <param name="parent">The parent quadrant (if any)</param>
            /// <param name="bounds">The bounds of this quadrant</param>
            public Quadrant(Quadrant parent, Rectangle bounds)
            {
                this.Parent = parent;
                Debug.Assert(bounds.Width != 0 && bounds.Height != 0, "Cannot have empty bound");

                this.Bounds = bounds;
                this.centerX = (bounds.Left + bounds.Right) / 2;
                this.centerY = (bounds.Top + bounds.Bottom) / 2;
                this.splittable = bounds.Width >= 64 && bounds.Height >= 64;
            }

            /// <summary>
            /// The parent Quadrant or null if this is the root.
            /// </summary>
            public Quadrant Parent { get; }

            /// <summary>
            /// The bounds of this quadrant.
            /// </summary>
            public Rectangle Bounds { get; }

            /// <summary>
            /// Insert the specified node.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <param name="bounds">The bounds of that node.</param>
            public void Insert(T node, Rectangle bounds)
            {
                if (this.splittable)
                {
                    if (this.nodes?.Count + 1 > 16)
                    {
                        // split the quadrant
                        foreach (KeyValuePair<T, Rectangle> kvp in this.nodes)
                        {
                            this.InsertIntoTree(kvp.Key, kvp.Value);
                        }

                        this.nodes = null;
                        this.split = true;
                    }

                    if (this.split)
                    {
                        this.InsertIntoTree(node, bounds);
                        return;
                    }
                }

                // add to this quadrant
                if (this.nodes == null)
                {
                    this.nodes = new Dictionary<T, Rectangle>();
                }

                this.nodes[node] = bounds;
            }

            /// <summary>
            /// Returns all nodes in this quadrant.
            /// The nodes are returned in pretty much random order as far as the caller is concerned.
            /// </summary>
            /// <returns>
            /// The sequence of nodes.
            /// </returns>
            public IEnumerable<T> GetNodes()
            {
                if (this.nodes != null)
                {
                    foreach (T node in this.nodes.Keys)
                    {
                        yield return node;
                    }
                }

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

                if (this.sw != null)
                {
                    foreach (T node in this.sw.GetNodes())
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
            }

            /// <summary>
            /// Returns all nodes in this quadrant that intersect the specified bounds.
            /// The nodes are returned in pretty much random order as far as the caller is concerned.
            /// </summary>
            /// <param name="nodes">List of nodes found in the specified bounds</param>
            /// <param name="bounds">The bounds that contains the nodes you want returned</param>
            public IEnumerable<T> GetNodes(Rectangle bounds)
            {
                if (this.nodes != null)
                {
                    foreach (KeyValuePair<T, Rectangle> kvp in this.nodes)
                    {
                        if (kvp.Value.IntersectsWith(bounds))
                        {
                            yield return kvp.Key;
                        }
                    }
                }

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

                if (this.sw != null && this.sw.Bounds.IntersectsWith(bounds))
                {
                    foreach (T node in this.sw.GetNodes(bounds))
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
            }

            /// <summary>
            /// Return true if there are any nodes in this Quadrant that intersect the specified bounds.
            /// </summary>
            /// <param name="bounds">The bounds to test.</param>
            /// <returns>boolean</returns>
            public bool HasNodes(Rectangle bounds)
            {
                if (this.nodes != null)
                {
                    foreach (Rectangle value in this.nodes.Values)
                    {
                        if (value.IntersectsWith(bounds))
                        {
                            return true;
                        }
                    }
                }

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

                if (!found && this.sw != null && this.sw.Bounds.IntersectsWith(bounds))
                {
                    found = this.sw.HasNodes(bounds);
                }

                if (!found && this.se != null && this.se.Bounds.IntersectsWith(bounds))
                {
                    found = this.se.HasNodes(bounds);
                }

                return found;
            }

            /*
            /// <summary>
            /// Remove the specified node from this Quadrant.
            /// </summary>
            /// <param name="node">The node to remove.</param>
            /// <returns><b>true</b> if the node was found and removed; otherwise, <b>false</b>.</returns>
            public bool RemoveNode(T node)
            {
                return this.nodes != null ? this.nodes.Remove(node) : false;
            }*/

            private void InsertIntoTree(T node, Rectangle bounds)
            {
                if (bounds.X < this.centerX)
                {
                    if (bounds.Y < this.centerY)
                    {
                        // top-left quadrant
                        if (this.nw == null)
                        {
                            this.nw = new Quadrant(
                                this,
                                Rectangle.FromLTRB(this.Bounds.Left, this.Bounds.Top, this.centerX, this.centerY));
                        }

                        this.nw.Insert(node, bounds);
                    }
                    else
                    {
                        // bottom-left quadrant
                        if (this.sw == null)
                        {
                            this.sw = new Quadrant(
                                this,
                                Rectangle.FromLTRB(this.Bounds.Left, this.centerY, this.centerX, this.Bounds.Bottom));
                        }

                        this.sw.Insert(node, bounds);
                    }
                }
                else
                {
                    if (bounds.Y < this.centerY)
                    {
                        // top-right quadrant
                        if (this.ne == null)
                        {
                            this.ne = new Quadrant(
                                this,
                                Rectangle.FromLTRB(this.centerX, this.Bounds.Top, this.Bounds.Right, this.centerY));
                        }

                        this.ne.Insert(node, bounds);
                    }
                    else
                    {
                        // bottom-right quadrant
                        if (this.se == null)
                        {
                            this.se = new Quadrant(
                                this,
                                Rectangle.FromLTRB(this.centerX, this.centerY, this.Bounds.Right, this.Bounds.Bottom));
                        }

                        this.se.Insert(node, bounds);
                    }
                }

                this.split = true;
            }
        }
    }
}