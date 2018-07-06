// -----------------------------------------------------------------------
// <copyright file="BidirectionalVertex{TVertex,TEdge}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System.Collections.Generic;

    /// <summary>
    /// The default bi-directional graph vertex implementation.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    /// <typeparam name="TEdge">The type of the edges.</typeparam>
    internal class BidirectionalVertex<TVertex, TEdge>
        where TEdge : Edge<TVertex>
    {
        /// <summary>
        /// The adjacency list that holds in edges.
        /// </summary>
        private readonly List<TEdge> inpEdges;

        /// <summary>
        /// The adjacency list that holds out edges.
        /// </summary>
        private readonly List<TEdge> outEdges;

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalVertex{TVertex, TEdge}"/> class
        /// that is empty and has the default initial capacity.
        /// </summary>
        public BidirectionalVertex() : this(-1, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalVertex{TVertex, TEdge}"/> class
        /// that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="inpCapacity">The number of in edges that the vertex can initially store.</param>
        /// <param name="outCapacity">The number of out edges that the vertex can initially store.</param>
        public BidirectionalVertex(int inpCapacity, int outCapacity)
        {
            this.inpEdges = inpCapacity > 0 ? new List<TEdge>(inpCapacity) : new List<TEdge>();
            this.outEdges = outCapacity > 0 ? new List<TEdge>(outCapacity) : new List<TEdge>();
        }

        /// <summary>
        /// Gets the number of edges coming out of the specified vertex.
        /// </summary>
        /// <value>The number of edges coming out of the specified vertex.</value>
        public int OutDegree => this.outEdges.Count;

        /// <summary>
        /// Gets the number of edges coming into the specified vertex.
        /// </summary>
        /// <value>The number of edges coming into the specified vertex.</value>
        public int InDegree => this.inpEdges.Count;

        /// <summary>
        /// Gets the total number of edges coming in and out of the specified vertex.
        /// </summary>
        /// <value>The total number of edges coming in and out of the specified vertex.</value>
        public int Degree => this.OutDegree + this.InDegree;

        /// <summary>
        /// Gets the edges coming out of the specified vertex.
        /// </summary>
        /// <value>The collection edges coming out of the specified vertex.</value>
        public IList<TEdge> OutEdges => this.outEdges;

        /// <summary>
        /// Gets the edges coming into the specified vertex.
        /// </summary>
        /// <value>The collection edges coming into the specified vertex.</value>
        public IList<TEdge> InEdges => this.inpEdges;

        /// <summary>
        /// Removes all edges from the vertex.
        /// </summary>
        public void Clear()
        {
            this.outEdges.Clear();
            this.inpEdges.Clear();
        }
    }
}
