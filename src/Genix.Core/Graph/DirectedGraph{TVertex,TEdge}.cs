// -----------------------------------------------------------------------
// <copyright file="DirectedGraph{TVertex,TEdge}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A mutable directed graph data structure efficient for sparse graph representation
    /// where out-edge and in-edges need to be enumerated.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    /// <typeparam name="TEdge">The type of the edges.</typeparam>
    public class DirectedGraph<TVertex, TEdge>
        where TVertex : ICloneable
        where TEdge : Edge<TVertex>
    {
        /// <summary>
        /// The adjacency lists that hold in vertices and edges.
        /// </summary>
        private readonly Dictionary<TVertex, Vertex<TVertex, TEdge>> vertices;

        /// <summary>
        /// The number of edges that the new graph can initially store.
        /// </summary>
        private readonly int edgeCapacity = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex, TEdge}"/> class
        /// that is empty, allows parallel edges, and has the default initial capacity for vertices and edges.
        /// </summary>
        public DirectedGraph()
            : this(true, -1, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex, TEdge}"/> class
        /// that is empty and has the default initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        public DirectedGraph(bool allowParallelEdges)
            : this(allowParallelEdges, -1, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex, TEdge}"/> class
        /// that is empty, has the specified initial capacity for vertices and the default initial capacity for edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        public DirectedGraph(bool allowParallelEdges, int vertexCapacity)
            : this(allowParallelEdges, vertexCapacity, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex, TEdge}"/> class
        /// that is empty and has the specified initial capacity for vertices and edges.
        /// </summary>
        /// <param name="allowParallelEdges"><b>true</b> to allow parallel edges; otherwise, <b>false</b>.</param>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph can initially store.</param>
        public DirectedGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity)
        {
            this.AllowParallelEdges = allowParallelEdges;

            if (vertexCapacity > -1)
            {
                this.vertices = new Dictionary<TVertex, Vertex<TVertex, TEdge>>(vertexCapacity);
            }
            else
            {
                this.vertices = new Dictionary<TVertex, Vertex<TVertex, TEdge>>();
            }

            this.edgeCapacity = edgeCapacity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{TVertex, TEdge}"/> class
        /// using existing graph as a source.
        /// </summary>
        /// <param name="other">The existing <see cref="DirectedGraph{TVertex, TEdge}"/> to create this graph from.</param>
        /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
        public DirectedGraph(DirectedGraph<TVertex, TEdge> other, bool cloneVertices)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.AllowParallelEdges = other.AllowParallelEdges;
            this.Size = other.Size;

            // clone vertices
            this.vertices = new Dictionary<TVertex, Vertex<TVertex, TEdge>>(other.vertices.Count);
            Dictionary<TVertex, TVertex> clonedVertices = new Dictionary<TVertex, TVertex>(other.vertices.Count);
            foreach (KeyValuePair<TVertex, Vertex<TVertex, TEdge>> kvp in other.vertices)
            {
                TVertex clonedVertex = cloneVertices ? (TVertex)kvp.Key.Clone() : kvp.Key;
                clonedVertices.Add(kvp.Key, clonedVertex);
                this.vertices.Add(clonedVertex, new Vertex<TVertex, TEdge>(kvp.Value.InDegree, kvp.Value.OutDegree));
            }

            // clone edges - preserve the order of edges in vertices
            Dictionary<TEdge, TEdge> clonedEdges = new Dictionary<TEdge, TEdge>();
            foreach (TEdge edge in other.Edges)
            {
                TEdge clonedEdge = (TEdge)edge.Clone();
                clonedEdge.Source = clonedVertices[edge.Source];
                clonedEdge.Target = clonedVertices[edge.Target];
                clonedEdges.Add(edge, clonedEdge);
            }

            // move edges from one graph into another
            foreach (KeyValuePair<TVertex, Vertex<TVertex, TEdge>> kvp in other.vertices)
            {
                Vertex<TVertex, TEdge> clonedVertex = this.vertices[clonedVertices[kvp.Key]];

                foreach (TEdge edge in kvp.Value.InEdges)
                {
                    clonedVertex.InEdges.Add(clonedEdges[edge]);
                }

                foreach (TEdge edge in kvp.Value.OutEdges)
                {
                    clonedVertex.OutEdges.Add(clonedEdges[edge]);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the graph allows parallel edges.
        /// </summary>
        /// <value>
        /// <b>true</b> if the graph allows parallel edges; otherwise, <b>false</b>.
        /// </value>
        public bool AllowParallelEdges { get; } = false;

        /// <summary>
        /// Gets the number of vertices in the graph.
        /// </summary>
        /// <value>
        /// The number of vertices in the graph.
        /// </value>
        public int VertexCount => this.vertices.Count;

        /// <summary>
        /// Gets the number of vertices in the graph.
        /// </summary>
        /// <value>
        /// The number of vertices in the graph.
        /// </value>
        public int Order => this.vertices.Count;

        /// <summary>
        /// Gets the number of edges in the graph.
        /// </summary>
        /// <value>
        /// The number of edges in the graph.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Gets all the vertices in the graph.
        /// </summary>
        /// <value>
        /// The collection of all the vertices in the graph.
        /// </value>
        public IEnumerable<TVertex> Vertices => this.vertices.Keys;

        /// <summary>
        /// Gets all the edges in the graph.
        /// </summary>
        /// <value>
        /// The collection of all the edges in the graph.
        /// </value>
        public IEnumerable<TEdge> Edges
        {
            get
            {
                foreach (var vertex in this.vertices.Values)
                {
                    foreach (TEdge edge in vertex.OutEdges)
                    {
                        yield return edge;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a graph source vertices.
        /// </summary>
        /// <value>
        /// A graph source vertices.
        /// </value>
        public IEnumerable<TVertex> Sources => this.vertices.Where(x => x.Value.InDegree == 0).Select(x => x.Key);

        /// <summary>
        /// Gets a graph sink vertices.
        /// </summary>
        /// <value>
        /// A graph sink vertices.
        /// </value>
        public IEnumerable<TVertex> Sinks => this.vertices.Where(x => x.Value.OutDegree == 0).Select(x => x.Key);

        /// <inheritdoc />
        public override string ToString()
        {
            // get vertices
            Dictionary<TVertex, string> verticesCodes = this.vertices.Keys
                .Select((x, i) => Tuple.Create(x, VertexNameFromIndex(i)))
                .ToDictionary(x => x.Item1, x => x.Item2);

            List<List<string>> edges = new List<List<string>>(this.Size);
            FormatEdges();

            List<string> codes = new List<string>(verticesCodes.Count);
            codes.AddRange(verticesCodes.Select(kvp => this.Degree(kvp.Key) == 0 ? kvp.Key.ToString() : string.Join("=", kvp.Value, kvp.Key.ToString())));

            return string.Join(", ", codes.Concat(edges.Select(x => string.Join("~", x))));

            string VertexNameFromIndex(int vertexIndex)
            {
                int dividend = vertexIndex + 1;
                string vertexName = string.Empty;

                while (dividend > 0)
                {
                    int modulo = (dividend - 1) % 26;
                    vertexName = Convert.ToChar(65 + modulo).ToString() + vertexName;
                    dividend = (int)((dividend - modulo) / 26);
                }

                return vertexName;
            }

            void FormatEdges()
            {
                // get edges
                foreach (TEdge edge in this.Edges)
                {
                    edges.Add(new List<string>()
                    {
                        verticesCodes[edge.Source],
                        verticesCodes[edge.Target],
                    });
                }

                // combine edges
                for (int i = 0, ii = edges.Count; i < ii; i++)
                {
                    for (int j = 0; j < ii; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        List<string> edge1 = edges[i];
                        List<string> edge2 = edges[j];

                        if (edge1.First() == edge2.Last())
                        {
                            edge2.AddRange(edge1.Skip(1));
                            edges.RemoveAt(i);

                            if (i < j)
                            {
                                j--;
                            }

                            i--;
                            ii--;
                            break;
                        }
                        else if (edge2.First() == edge1.Last())
                        {
                            edge1.AddRange(edge2.Skip(1));
                            edges.RemoveAt(j);

                            if (j < i)
                            {
                                i--;
                            }

                            j--;
                            ii--;
                        }
                    }
                }

                // substitute codes of unique vertices with their names
                foreach (List<string> combinedEdge in edges)
                {
                    for (int i = 0, ii = combinedEdge.Count; i < ii; i++)
                    {
                        string s = combinedEdge[i];
                        if (edges.SelectMany(x => x).Count(x => x == s) == 1)
                        {
                            KeyValuePair<TVertex, string> kvp = verticesCodes.FirstOrDefault(x => x.Value == s);
                            combinedEdge[i] = kvp.Key.ToString();
                            verticesCodes.Remove(kvp.Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual DirectedGraph<TVertex, TEdge> Clone(bool cloneVertices) =>
            new DirectedGraph<TVertex, TEdge>(this, cloneVertices);

        /// <summary>
        /// Determines whether the specified vertex is already part of the graph.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns><b>true</b> if the specified vertex is already part of the graph; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsVertex(TVertex vertex) => this.vertices.ContainsKey(vertex);

        /// <summary>
        /// Determines whether the specified edge is already part of the graph.
        /// </summary>
        /// <param name="sourceVertex">The source vertex.</param>
        /// <param name="targetVertex">The target vertex.</param>
        /// <returns><b>true</b> if the specified edge is already part of the graph; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsEdge(TVertex sourceVertex, TVertex targetVertex)
        {
            if (!this.TryGetOutEdges(sourceVertex, out IList<TEdge> edges))
            {
                return false;
            }

            foreach (TEdge edge in edges)
            {
                if (object.Equals(edge.Target, targetVertex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the number of edges coming out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns>The number of edges coming out of the specified vertex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int OutDegree(TVertex vertex) => this.vertices[vertex].OutDegree;

        /// <summary>
        /// Returns the number of edges coming into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns>The number of edges coming into the specified vertex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int InDegree(TVertex vertex) => this.vertices[vertex].InDegree;

        /// <summary>
        /// Returns the total number of edges coming in and out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns>The total number of edges coming in and out of the specified vertex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Degree(TVertex vertex) => this.vertices[vertex].Degree;

        /// <summary>
        /// Returns the edges coming out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns>The collection edges coming out of the specified vertex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<TEdge> OutEdges(TVertex vertex) => this.vertices[vertex].OutEdges;

        /// <summary>
        /// Returns the edges coming into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns>The collection edges coming into the specified vertex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<TEdge> InEdges(TVertex vertex) => this.vertices[vertex].InEdges;

        /// <summary>
        /// Determines whether there are edges coming out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns><b>true</b> when <see cref="OutDegree"/> is greater than zero; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasOutEdges(TVertex vertex) => this.OutDegree(vertex) > 0;

        /// <summary>
        /// Determines whether there are edges coming into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to test.</param>
        /// <returns><b>true</b> when <see cref="InDegree"/> is greater than zero; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasInEdges(TVertex vertex) => this.InDegree(vertex) > 0;

        /// <summary>
        /// Removes all vertices and edges from the graph.
        /// </summary>
        public void Clear()
        {
            this.vertices.Clear();
            this.Size = 0;
        }

        /// <summary>
        /// Executes a provide action on each vertex in the graph.
        /// </summary>
        /// <param name="action">The action to executes.</param>
        public void ForEachVertex(Action<TVertex> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (TVertex vertex in this.vertices.Keys)
            {
                action(vertex);
            }
        }

        /// <summary>
        /// Executes a provide action on each edge in the graph.
        /// </summary>
        /// <param name="action">The action to executes.</param>
        public void ForEachEdge(Action<TEdge> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var vertex in this.vertices.Values)
            {
                foreach (TEdge edge in vertex.OutEdges)
                {
                    action(edge);
                }
            }
        }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns>
        /// <b>true</b> if the specified vertex was added to the graph; <b>false</b> if the specified vertex is already part of the graph.
        /// </returns>
        public bool AddVertex(TVertex vertex)
        {
            if (this.ContainsVertex(vertex))
            {
                return false;
            }

            if (this.edgeCapacity > 0)
            {
                this.vertices.Add(vertex, new Vertex<TVertex, TEdge>(this.edgeCapacity, this.edgeCapacity));
            }
            else
            {
                this.vertices.Add(vertex, new Vertex<TVertex, TEdge>());
            }

            this.OnVertexAdded(vertex);

            return true;
        }

        /// <summary>
        /// Removed a vertex from the graph.
        /// </summary>
        /// <param name="vertex">The vertex to remove.</param>
        /// <returns>
        /// <b>true</b> if the specified vertex was removed to the graph; <b>false</b> if the specified vertex is not a part of the graph.
        /// </returns>
        public bool RemoveVertex(TVertex vertex)
        {
            if (!this.TryGetVertex(vertex, out Vertex<TVertex, TEdge> edges))
            {
                return false;
            }

            // remove each in edge touching the vertex
            foreach (TEdge edge in edges.InEdges)
            {
                this.OnEdgeRemoved(edge);

                if (this.vertices.TryGetValue(edge.Source, out Vertex<TVertex, TEdge> s))
                {
                    s.OutEdges.Remove(edge);
                }
            }

            // remove each out edge touching the vertex
            foreach (TEdge edge in edges.OutEdges)
            {
                this.OnEdgeRemoved(edge);

                if (this.vertices.TryGetValue(edge.Target, out Vertex<TVertex, TEdge> t))
                {
                    t.InEdges.Remove(edge);
                }
            }

            this.Size -= edges.InEdges.Count + edges.OutEdges.Count;

            this.vertices.Remove(vertex);
            this.OnVertexRemoved(vertex);

            return true;
        }

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        /// <returns>
        /// <b>true</b> if the specified edge was added to the graph; <b>false</b> if the specified edge is already part of the graph.
        /// </returns>
        public bool AddEdge(TEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!this.AllowParallelEdges && this.ContainsEdge(edge.Source, edge.Target))
            {
                return false;
            }

            this.AddVertex(edge.Source);
            this.AddVertex(edge.Target);

            this.vertices[edge.Target].InEdges.Add(edge);
            this.vertices[edge.Source].OutEdges.Add(edge);

            this.Size++;

            this.OnEdgeAdded(edge);

            return true;
        }

        /// <summary>
        /// Adds a collection of edges to the graph.
        /// </summary>
        /// <param name="edges">The edges to add.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(IEnumerable<TEdge> edges)
        {
            if (edges == null)
            {
                throw new ArgumentNullException(nameof(edges));
            }

            int count = 0;
            foreach (TEdge edge in edges)
            {
                if (this.AddEdge(edge))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Removes an edge from the graph.
        /// </summary>
        /// <param name="edge">The edge to remove.</param>
        /// <param name="removeIsolatedVertices">The value that indicates whether the vertices that become isolated after the edge removal should be removed from the graph.</param>
        /// <returns>
        /// <b>true</b> if the specified edge was removed to the graph; <b>false</b> if the specified edge is not part of the graph.
        /// </returns>
        public bool RemoveEdge(TEdge edge, bool removeIsolatedVertices)
        {
            if (this.vertices.TryGetValue(edge.Source, out Vertex<TVertex, TEdge> s) &&
                this.vertices.TryGetValue(edge.Target, out Vertex<TVertex, TEdge> t))
            {
                this.OnEdgeRemoved(edge);

                s.OutEdges.Remove(edge);
                t.InEdges.Remove(edge);

                // remove isolated vertices
                if (removeIsolatedVertices)
                {
                    if (s.Degree == 0)
                    {
                        this.vertices.Remove(edge.Source);
                    }

                    if (t.Degree == 0)
                    {
                        this.vertices.Remove(edge.Target);
                    }
                }

                this.Size--;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a collection of edges from the graph.
        /// </summary>
        /// <param name="edges">The edges to remove.</param>
        /// <param name="removeIsolatedVertices">The value that indicates whether the vertices that become isolated after the edge removal should be removed from the graph.</param>
        /// <returns>The number of edges that were removed.</returns>
        public int RemoveEdges(IEnumerable<TEdge> edges, bool removeIsolatedVertices)
        {
            if (edges == null)
            {
                throw new ArgumentNullException(nameof(edges));
            }

            int count = 0;
            foreach (TEdge edge in edges)
            {
                if (this.RemoveEdge(edge, removeIsolatedVertices))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Adds a specified graph to this graph.
        /// </summary>
        /// <param name="graph">The graph to add.</param>
        /// <returns>
        /// <b>true</b> if at least one edge was added to the graph;otherwise, <b>false</b>.
        /// </returns>
        public bool AddGraph(DirectedGraph<TVertex, TEdge> graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            return this.AddEdges(graph.Edges) > 0;
        }

        internal bool TryGetVertex(TVertex vertex, out Vertex<TVertex, TEdge> value)
        {
            return this.vertices.TryGetValue(vertex, out value);
        }

        internal bool TryGetInEdges(TVertex vertex, out IList<TEdge> edges)
        {
            if (this.vertices.TryGetValue(vertex, out Vertex<TVertex, TEdge> v))
            {
                edges = v.InEdges;
                return true;
            }

            edges = null;
            return false;
        }

        internal bool TryGetOutEdges(TVertex vertex, out IList<TEdge> edges)
        {
            if (this.vertices.TryGetValue(vertex, out Vertex<TVertex, TEdge> v))
            {
                edges = v.OutEdges;
                return true;
            }

            edges = null;
            return false;
        }

        /// <summary>
        /// Called when a vertex is added to the graph.
        /// </summary>
        /// <param name="vertex">The vertex that was added.</param>
        protected virtual void OnVertexAdded(TVertex vertex)
        {
        }

        /// <summary>
        /// Called when a vertex is removed to the graph.
        /// </summary>
        /// <param name="vertex">The vertex that was removed.</param>
        protected virtual void OnVertexRemoved(TVertex vertex)
        {
        }

        /// <summary>
        /// Called when an edge is added to the graph.
        /// </summary>
        /// <param name="edge">The edge that was added.</param>
        protected virtual void OnEdgeAdded(TEdge edge)
        {
        }

        /// <summary>
        /// Called when an edge is removed to the graph.
        /// </summary>
        /// <param name="edge">The edge that was removed.</param>
        protected virtual void OnEdgeRemoved(TEdge edge)
        {
        }
    }
}
