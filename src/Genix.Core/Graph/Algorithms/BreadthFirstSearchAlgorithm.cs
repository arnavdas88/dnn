// -----------------------------------------------------------------------
// <copyright file="BreadthFirstSearchAlgorithm.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a breadth-first search algorithm.
    /// </summary>
    public static class BreadthFirstSearchAlgorithm
    {
        /// <summary>
        /// Performs a breadth-first search on a directed graph.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertices.</typeparam>
        /// <typeparam name="TEdge">The type of the edges.</typeparam>
        /// <param name="graph">The graph to search.</param>
        /// <param name="vertex">The vertex to start search from.</param>
        /// <param name="processVertexEarly">The <see cref="Action{TVertex}"/> that is called after the vertex was discovered.</param>
        /// <param name="processVertexLate">The <see cref="Action{TVertex}"/> that is called after the vertex was processed.</param>
        /// <param name="processEdge">The <see cref="Action{TVertex}"/> that is called after the edge was processed.</param>
        public static void BreadthFirstSearch<TVertex, TEdge>(
            this DirectedGraph<TVertex, TEdge> graph,
            TVertex vertex,
            Action<TVertex> processVertexEarly,
            Action<TVertex> processVertexLate,
            Action<TEdge> processEdge)
            where TVertex : ICloneable
            where TEdge : Edge<TVertex>
        {
            const int Undiscovered = default;
            const int Discovered = 1;
            const int Processed = 2;

            Queue<TVertex> queue = new Queue<TVertex>();
            Dictionary<TVertex, int> colors = new Dictionary<TVertex, int>(graph.VertexCount + 1);

            queue.Enqueue(vertex);
            colors[vertex] = Discovered;

            while (queue.Count > 0)
            {
                TVertex source = queue.Dequeue();
                processVertexEarly?.Invoke(source);
                colors[source] = Processed;

                foreach (TEdge edge in graph.OutEdges(source))
                {
                    TVertex target = edge.Target;
                    colors.TryGetValue(target, out int color);

                    if (color == Undiscovered)
                    {
                        // discover vertex
                        queue.Enqueue(target);
                        colors[target] = Discovered;
                    }

                    if (color != Processed)
                    {
                        processEdge?.Invoke(edge);
                    }
                }

                processVertexLate?.Invoke(source);
            }
        }
    }
}
