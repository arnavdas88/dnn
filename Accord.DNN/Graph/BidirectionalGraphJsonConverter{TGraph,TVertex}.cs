// -----------------------------------------------------------------------
// <copyright file="BidirectionalGraphJsonConverter{TGraph,TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;

    internal class BidirectionalGraphJsonConverter<TGraph, TVertex>
        : BidirectionalGraphJsonConverter<TGraph, TVertex, Edge<TVertex>>
        where TGraph : BidirectionalGraph<TVertex>
        where TVertex : ICloneable
    {
    }
}
