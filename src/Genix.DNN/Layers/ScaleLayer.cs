// -----------------------------------------------------------------------
// <copyright file="ScaleLayer.cs" company="Noname, Inc.">
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
    /// Implements a scaling operation on a <see cref="Tensor"/>.
    /// A scaling operation is defined as follows: x -> alpha * x .
    /// </summary>
    public class ScaleLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(S)(-?\d*\.?\d+)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="alpha">The scaling factor.</param>
        public ScaleLayer(int[] inputShape, float alpha)
            : base(1, inputShape)
        {
            this.Alpha = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public ScaleLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(1, inputShape)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, ScaleLayer.ArchitecturePattern);
            this.Alpha = Convert.ToSingle(groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleLayer"/> class, using the existing <see cref="ScaleLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ScaleLayer"/> to copy the data from.</param>
        public ScaleLayer(ScaleLayer other)
            : base(other)
        {
            this.Alpha = other.Alpha;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ScaleLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private ScaleLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "S{0}", this.Alpha);

        /// <summary>
        /// Gets the scaling factor.
        /// </summary>
        /// <value>
        /// The scaling factor.
        /// </value>
        [JsonProperty("Alpha")]
        public float Alpha { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new ScaleLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            int numberOfOutputs = this.NumberOfOutputs;

            Tensor[] ys = new Tensor[numberOfOutputs];
            for (int i = 0; i < numberOfOutputs; i++)
            {
                ys[i] = session.Multiply(xs[i], this.Alpha);
            }

            return ys;
        }
    }
}
