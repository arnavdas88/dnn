// -----------------------------------------------------------------------
// <copyright file="DirectedGraph{TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A mutable directed graph data structure efficient for sparse graph representation
    /// where out-edge and in-edges need to be enumerated.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    public class DirectedGraph<TVertex> : DirectedGraph<TVertex, Edge<TVertex>>
        where TVertex : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex}"/> class
        /// that is empty, allows parallel edges, and has the default initial capacity for vertices and edges.
        /// </summary>
        public DirectedGraph()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex}"/> class
        /// that is empty and has the default initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        public DirectedGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex}"/> class
        /// that is empty, has the specified initial capacity for vertices and the default initial capacity for edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        public DirectedGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex}"/> class
        /// that is empty and has the specified initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph can initially store.</param>
        public DirectedGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity)
            : base(allowParallelEdges, vertexCapacity, edgeCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex}"/> class
        /// using existing graph as a source.
        /// </summary>
        /// <param name="other">The existing <see cref="DirectedGraph{TVertex}"/> to create this graph from.</param>
        /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
        public DirectedGraph(DirectedGraph<TVertex> other, bool cloneVertices)
            : base(other, cloneVertices)
        {
        }

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <returns>
        /// <b>true</b> if the edge was added to the graph; <b>false</b> if the edge is already part of the graph.
        /// </returns>
        public bool AddEdge(TVertex source, TVertex target) => this.AddEdge(new Edge<TVertex>(source, target));

        /// <summary>
        /// Adds a collection of edges to the graph.
        /// </summary>
        /// <param name="edges">The edges to add.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(IEnumerable<(TVertex source, TVertex target)> edges)
        {
            if (edges == null)
            {
                throw new ArgumentNullException(nameof(edges));
            }

            int count = 0;
            foreach ((TVertex source, TVertex target) in edges)
            {
                if (this.AddEdge(new Edge<TVertex>(source, target)))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
