// -----------------------------------------------------------------------
// <copyright file="SplitLayer.cs" company="Noname, Inc.">
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
    /// Implements a split operation on a <see cref="Tensor"/>.
    /// </summary>
    public class SplitLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(SP)(\d+)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfOutputs">The number of output tensors.</param>
        public SplitLayer(int[] inputShape, int numberOfOutputs)
            : base(numberOfOutputs, inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SplitLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(1 /* temp */, inputShape)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, SplitLayer.ArchitecturePattern);
            this.NumberOfOutputs = Convert.ToInt32(groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitLayer"/> class, using the existing <see cref="SplitLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SplitLayer"/> to copy the data from.</param>
        public SplitLayer(SplitLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SplitLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private SplitLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "SP{0}", this.NumberOfOutputs);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SplitLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return session.Repeat(xs[0], this.NumberOfOutputs);
        }
    }
}
