﻿// -----------------------------------------------------------------------
// <copyright file="BidirectionalGraph{TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;

    /// <summary>
    /// A mutable directed graph data structure efficient for sparse graph representation
    /// where out-edge and in-edges need to be enumerated.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    internal class BidirectionalGraph<TVertex> : BidirectionalGraph<TVertex, Edge<TVertex>>
        where TVertex : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalGraph{TVertex}"/> class
        /// that is empty, allows parallel edges, and has the default initial capacity for vertices and edges.
        /// </summary>
        public BidirectionalGraph()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalGraph{TVertex}"/> class
        /// that is empty and has the default initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        public BidirectionalGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalGraph{TVertex}"/> class
        /// that is empty, has the specified initial capacity for vertices and the default initial capacity for edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        public BidirectionalGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalGraph{TVertex}"/> class
        /// that is empty and has the specified initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph can initially store.</param>
        public BidirectionalGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity)
            : base(allowParallelEdges, vertexCapacity, edgeCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalGraph{TVertex}"/> class
        /// using existing graph as a source.
        /// </summary>
        /// <param name="other">The existing <see cref="BidirectionalGraph{TVertex}"/> to create this graph from.</param>
        /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
        public BidirectionalGraph(BidirectionalGraph<TVertex> other, bool cloneVertices)
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
        public bool AddEdge(TVertex source, TVertex target)
        {
            return base.AddEdge(new Edge<TVertex>(source, target));
        }
    }
}