// -----------------------------------------------------------------------
// <copyright file="InputLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// A dummy layer that essentially declares the size of input volume and must be first layer in the network.
    /// Inputs other than real-valued numbers are currently not supported.
    /// </summary>
    public class InputLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+|-1)x(\d+)x(\d+)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayer"/> class.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        public InputLayer(Shape shape)
            : base(1, shape)
        {
            this.Shape = shape;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public InputLayer(Shape shape, string architecture, RandomNumberGenerator<float> random)
            : base(1, shape /* temp */)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, InputLayer.ArchitecturePattern);

            this.Shape = this.OutputShape = new Shape(
                Shape.BWHC,
                -1,
                Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(groups[2].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(groups[3].Value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayer"/> class, using the existing <see cref="InputLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="InputLayer"/> to copy the data from.</param>
        public InputLayer(InputLayer other)
            : base(other)
        {
            this.Shape = other.Shape;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="InputLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private InputLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Join("x", this.Shape.Axes.Skip(1));

        /// <summary>
        /// Gets the shape of the layer's input tensor.
        /// </summary>
        /// <value>
        /// The <see cref="Shape"/> object.
        /// </value>
        [JsonProperty("shape")]
        public Shape Shape { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new InputLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // validate input tensors
            if (xs.Count != 1)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidInputTensor_InvalidCount);
            }

            int[] axes = this.Shape.Axes;
            Tensor x = xs[0];

            // validate tensor layout
            if (x.Rank != axes.Length)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_InvalidInputTensor_InvalidRank,
                    x.Rank,
                    axes.Length));
            }

            for (int i = 0, ii = axes.Length; i < ii; i++)
            {
                if (axes[i] >= 0 && axes[i] != x.Axes[i])
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidInputTensor_InvalidDimension,
                        x.Axes[i],
                        i,
                        axes[i]));
                }
            }

            return xs;
        }
    }
}
