﻿// -----------------------------------------------------------------------
// <copyright file="RNNLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Base recurrent network layer. This is an abstract class.
    /// </summary>
    public abstract class RNNLayer : TrainableLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RNNLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RNNLayer(int[] inputShape) : base(inputShape)
        {
            this.Graph = new NetworkGraph();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNLayer"/> class, using the existing <see cref="RNNLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="RNNLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RNNLayer(RNNLayer other) : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Graph = new NetworkGraph(other.Graph, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNLayer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        protected RNNLayer()
        {
        }

        /// <summary>
        /// Gets the recurrent network graph.
        /// </summary>
        /// <value>
        /// The recurrent network graph.
        /// </value>
        [JsonProperty("Graph", Order = 2)]
        internal NetworkGraph Graph { get; private set; }

        /// <inheritdoc />
        internal override bool NeedsActivation => true;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { this.Graph.Forward(session, xs[0]) };
        }

        /// <inheritdoc />
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<(Tensor, float, float)> EnumGradients()
        {
            return this.Graph.Vertices.OfType<StochasticLayer>().SelectMany(x => x.EnumGradients());
        }
    }
}