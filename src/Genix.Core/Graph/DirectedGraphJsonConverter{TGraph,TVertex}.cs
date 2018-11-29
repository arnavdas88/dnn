// -----------------------------------------------------------------------
// <copyright file="DirectedGraphJsonConverter{TGraph,TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;

    /// <summary>
    /// Represents a Json.NET converter for <see cref="DirectedGraphJsonConverter{TGraph, TVertex}"/> class.
    /// </summary>
    /// <typeparam name="TGraph">The type of the graph.</typeparam>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    public class DirectedGraphJsonConverter<TGraph, TVertex>
        : DirectedGraphJsonConverter<TGraph, TVertex, Edge<TVertex>>
        where TGraph : DirectedGraph<TVertex>
        where TVertex : ICloneable
    {
    }
}
