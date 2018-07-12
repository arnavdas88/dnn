// -----------------------------------------------------------------------
// <copyright file="HasPredecessorAlgorithm.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an algorithm that determines whether a vertex has a predecessor that matches the conditions defined by the specified predicate.
    /// </summary>
    public static class HasPredecessorAlgorithm
    {
        /// <summary>
        /// Determines whether a vertex has a predecessor that matches the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertices.</typeparam>
        /// <typeparam name="TEdge">The type of the edges.</typeparam>
        /// <param name="graph">The graph to search.</param>
        /// <param name="vertex">The vertex, which predecessor to search for.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the element to search for.</param>
        /// <returns><b>true</b> if the <c>vertex</c> has a predecessor that matches the conditions defined by the specified predicate; otherwise, <b>false</b>.</returns>
        public static bool HasPredecessor<TVertex, TEdge>(
            this BidirectionalGraph<TVertex, TEdge> graph,
            TVertex vertex,
            Predicate<TVertex> match)
            where TEdge : Edge<TVertex>
            where TVertex : ICloneable
        {
            return HasPredecessor(graph, vertex, match, new HashSet<TVertex>());
        }

        private static bool HasPredecessor<TVertex, TEdge>(
            BidirectionalGraph<TVertex, TEdge> graph,
            TVertex vertex,
            Predicate<TVertex> match,
            HashSet<TVertex> cache)
            where TEdge : Edge<TVertex>
            where TVertex : ICloneable
        {
            IList<TEdge> edges;
            if (!graph.TryGetInEdges(vertex, out edges))
            {
                return false;
            }

            foreach (TVertex source in edges.Select(x => x.Source).Where(x => !cache.Contains(x)))
            {
                if (match(source))
                {
                    return true;
                }

                cache.Add(source);

                if (HasPredecessor(graph, source, match, cache))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
