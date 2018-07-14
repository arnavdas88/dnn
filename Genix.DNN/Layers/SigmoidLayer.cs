// -----------------------------------------------------------------------
// <copyright file="SigmoidLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements Sigmoid nonlinearity element wise
    /// x -> 1/(1+e^(-x))
    /// so the output is between 0 and 1.
    /// </summary>
    public class SigmoidLayer : ActivationLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^SIG$";

        /// <summary>
        /// Initializes a new instance of the <see cref="SigmoidLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        public SigmoidLayer(int[] inputShape) : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigmoidLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SigmoidLayer(int[] inputShape, string architecture, RandomNumberGenerator random) : base(inputShape)
        {
            Layer.ParseArchitechture(architecture, SigmoidLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigmoidLayer"/> class, using the existing <see cref="SigmoidLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SigmoidLayer"/> to copy the data from.</param>
        public SigmoidLayer(SigmoidLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SigmoidLayer"/> class from being created.
        /// </summary>
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
