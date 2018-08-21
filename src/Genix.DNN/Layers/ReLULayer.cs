// -----------------------------------------------------------------------
// <copyright file="ReLULayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements ReLU nonlinearity element wise
    /// x -> max(0, x)
    /// the output is in [0, inf).
    /// </summary>
    public class ReLULayer : ActivationLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^RELU$";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReLULayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        public ReLULayer(int[] inputShape)
            : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReLULayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public ReLULayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(inputShape)
        {
            Layer.ParseArchitecture(architecture, ReLULayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReLULayer"/> class, using the existing <see cref="ReLULayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ReLULayer"/> to copy the data from.</param>
        public ReLULayer(ReLULayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ReLULayer"/> class from being created.
        /// </summary>
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
