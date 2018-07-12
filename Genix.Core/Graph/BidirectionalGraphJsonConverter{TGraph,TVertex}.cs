// -----------------------------------------------------------------------
// <copyright file="BidirectionalGraphJsonConverter{TGraph,TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;

    /// <summary>
    /// Represents a Json.NET converter for <see cref="BidirectionalGraphJsonConverter{TGraph, TVertex}"/> class.
    /// </summary>
    /// <typeparam name="TGraph">The type of the graph.</typeparam>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    public class BidirectionalGraphJsonConverter<TGraph, TVertex>
        : BidirectionalGraphJsonConverter<TGraph, TVertex, Edge<TVertex>>
        where TGraph : BidirectionalGraph<TVertex>
        where TVertex : ICloneable
    {
    }
}
