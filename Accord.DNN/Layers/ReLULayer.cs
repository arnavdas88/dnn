// -----------------------------------------------------------------------
// <copyright file="ReLULayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements ReLU nonlinearity element wise
    /// x -> max(0, x)
    /// the output is in [0, inf).
    /// </summary>
    public class ReLULayer : ActivationLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReLULayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReLULayer(int[] inputShape) : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReLULayer"/> class, using the existing <see cref="ReLULayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ReLULayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReLULayer(ReLULayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ReLULayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private ReLULayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "RELU";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new ReLULayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.ReLU(xs[0]) };
        }
    }
}
