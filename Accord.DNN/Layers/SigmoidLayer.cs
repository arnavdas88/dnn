// -----------------------------------------------------------------------
// <copyright file="SigmoidLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements Sigmoid nonlinearity element wise
    /// x -> 1/(1+e^(-x))
    /// so the output is between 0 and 1.
    /// </summary>
    public class SigmoidLayer : ActivationLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SigmoidLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SigmoidLayer(int[] inputShape) : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigmoidLayer"/> class, using the existing <see cref="SigmoidLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SigmoidLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SigmoidLayer(SigmoidLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SigmoidLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private SigmoidLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "SIG";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SigmoidLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.Sigmoid(xs[0]) };
        }
    }
}
