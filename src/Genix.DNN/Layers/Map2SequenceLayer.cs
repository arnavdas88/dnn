﻿// -----------------------------------------------------------------------
// <copyright file="Map2SequenceLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Maps an input <see cref="Tensor"/> into a mini-batch <see cref="Tensor"/> suitable for RNN training.
    /// </summary>
    public class Map2SequenceLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^M2S$";

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2SequenceLayer"/> class.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        public Map2SequenceLayer(Shape shape)
            : base(1, Map2SequenceLayer.CalculateOutputShape(shape))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2SequenceLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public Map2SequenceLayer(Shape shape, string architecture, RandomNumberGenerator<float> random)
            : base(1, Map2SequenceLayer.CalculateOutputShape(shape))
        {
            Layer.ParseArchitecture(architecture, Map2SequenceLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2SequenceLayer"/> class, using the existing <see cref="Map2SequenceLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Map2SequenceLayer"/> to copy the data from.</param>
        public Map2SequenceLayer(Map2SequenceLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Map2SequenceLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private Map2SequenceLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "M2S";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new Map2SequenceLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.Squeeze(xs[0], 0) };
        }

        /// <summary>
        /// Computes the shape of the layer's output tensor.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <returns>
        /// The shape of the layer's output tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Shape CalculateOutputShape(Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            int[] axes = shape.Axes;
            int mbsize = axes.Skip(2).Aggregate(1, (total, next) => total * next);
            return new Shape(new int[] { axes[1], mbsize });
        }
    }
}
