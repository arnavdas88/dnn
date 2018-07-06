﻿// -----------------------------------------------------------------------
// <copyright file="DropoutLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Drops random neurons from the layer.
    /// </summary>
    public sealed class DropoutLayer : Layer
    {
        /// <summary>
        /// The random numbers generator.
        /// </summary>
        private readonly RandomNumberGenerator random = new RandomGenerator();

        /// <summary>
        /// Initializes a new instance of the <see cref="DropoutLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="probability">The dropout probability.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DropoutLayer(int[] inputShape, double probability) : base(1, inputShape)
        {
            this.Probability = probability;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropoutLayer"/> class, using the existing <see cref="DropoutLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="DropoutLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DropoutLayer(DropoutLayer other) : base(other)
        {
            this.Probability = other.Probability;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DropoutLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private DropoutLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "D{0}", this.Probability);

        /// <summary>
        /// Gets the dropout probability.
        /// </summary>
        /// <value>
        /// The dropout probability. The value between 0 and 1.
        /// </value>
        [JsonProperty("Probability")]
        public double Probability { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new DropoutLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.Dropout(xs[0], this.random, (float)this.Probability) };
        }
    }
}