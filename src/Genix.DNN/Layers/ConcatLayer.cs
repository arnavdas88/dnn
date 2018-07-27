// -----------------------------------------------------------------------
// <copyright file="ConcatLayer.cs" company="Noname, Inc.">
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
        /// The axis to concatenate along.
        /// </summary>
        private const int Axis = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class.
        /// </summary>
        /// <param name="inputShapes">The dimensions of the layer's input tensors.</param>
        public ConcatLayer(IList<int[]> inputShapes)
            : base(1, Shape.Concat(inputShapes, ConcatLayer.Axis))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShapes">The dimensions of the layer's input tensors.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public ConcatLayer(IList<int[]> inputShapes, string architecture, RandomNumberGenerator random)
            : base(1, Shape.Concat(inputShapes, ConcatLayer.Axis))
        {
            Layer.ParseArchitechture(architecture, ConcatLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatLayer"/> class, using the existing <see cref="ConcatLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ConcatLayer"/> to copy the data from.</param>
        public ConcatLayer(ConcatLayer other) : base(other)
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
            return new[] { session.Concat(xs, ConcatLayer.Axis) };
        }
    }
}
