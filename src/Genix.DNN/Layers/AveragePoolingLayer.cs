// -----------------------------------------------------------------------
// <copyright file="AveragePoolingLayer.cs" company="Noname, Inc.">
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
    /// Performs average pooling.
    /// </summary>
    public sealed class AveragePoolingLayer : PoolingLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(AP)(\d+)(?:x(\d+))?(?:\+(\d+)(?:x(\d+))?\(S\))?$";

        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePoolingLayer"/> class.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        public AveragePoolingLayer(Shape shape, Kernel kernel)
            : base(shape, kernel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePoolingLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public AveragePoolingLayer(Shape shape, string architecture, RandomNumberGenerator<float> random)
            : base(shape, AveragePoolingLayer.KernelFromArchitecture(architecture))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePoolingLayer"/> class, using the existing <see cref="AveragePoolingLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="AveragePoolingLayer"/> to copy the data from.</param>
        public AveragePoolingLayer(AveragePoolingLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AveragePoolingLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private AveragePoolingLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "AP{0}",
            this.Kernel.ToString(this.Kernel.Width, this.Kernel.Height));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new AveragePoolingLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.AveragePooling(xs[0], this.Kernel) };
        }

        /// <summary>
        /// Extracts the pooling kernel from layer architecture.
        /// </summary>
        /// <param name="architecture">The layer architecture.</param>
        /// <returns>The pooling kernel.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Kernel KernelFromArchitecture(string architecture)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, AveragePoolingLayer.ArchitecturePattern);
            return Layer.ParseKernel(groups, 2, null, false);
        }
    }
}
