// -----------------------------------------------------------------------
// <copyright file="NetworkGraphJsonConverter.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using Genix.DNN.Layers;
    using Genix.Graph;

    internal class NetworkGraphJsonConverter : BidirectionalGraphJsonConverter<NetworkGraph, Layer, NetworkEdge>
    {
    }
}
