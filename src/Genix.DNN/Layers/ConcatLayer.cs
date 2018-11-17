// -----------------------------------------------------------------------
// <copyright file="ConcatLayer.cs" company="Noname, Inc.">
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
    /// Concatenates a group of <see cref="Tensor"/>s.
    /// </summary>
    public class ConcatLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^CONCAT$";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class.
        /// </summary>
        /// <param name="shapes">The shapes of the layer's input tensor.</param>
        public ConcatLayer(IList<Shape> shapes)
            : base(1, Shape.Concat(shapes, Axis.C))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shapes">The shapes of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public ConcatLayer(IList<Shape> shapes, string architecture, RandomNumberGenerator<float> random)
            : base(1, Shape.Concat(shapes, Axis.C))
        {
            Layer.ParseArchitecture(architecture, ConcatLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class, using the existing <see cref="ConcatLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ConcatLayer"/> to copy the data from.</param>
        public ConcatLayer(ConcatLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ConcatLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private ConcatLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "CONCAT";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new ConcatLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // compute the channel axis
            int axis = xs[0].Shape.GetAxisIndex(Axis.C);

            return new[] { session.Concat(xs, axis) };
        }
    }
}
