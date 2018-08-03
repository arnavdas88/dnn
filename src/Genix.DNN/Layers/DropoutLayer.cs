// -----------------------------------------------------------------------
// <copyright file="DropoutLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Drops random neurons from the layer.
    /// </summary>
    public sealed class DropoutLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(D)(0\.\d+)$";

        /// <summary>
        /// The random numbers generator.
        /// </summary>
        private readonly RandomNumberGenerator<float> random = new RandomGenerator();

        /// <summary>
        /// Initializes a new instance of the <see cref="DropoutLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="probability">The dropout probability.</param>
        public DropoutLayer(int[] inputShape, double probability)
            : base(1, inputShape)
        {
            this.Probability = probability;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropoutLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public DropoutLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(1, inputShape)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, DropoutLayer.ArchitecturePattern);
            this.Probability = Convert.ToDouble(groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropoutLayer"/> class, using the existing <see cref="DropoutLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="DropoutLayer"/> to copy the data from.</param>
        public DropoutLayer(DropoutLayer other)
            : base(other)
        {
            this.Probability = other.Probability;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DropoutLayer"/> class from being created.
        /// </summary>
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
