// -----------------------------------------------------------------------
// <copyright file="MaxPoolingLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Performs max pooling.
    /// </summary>
    public sealed class MaxPoolingLayer : PoolingLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(MP)(\d+)(?:x(\d+))?(?:\+(\d+)(?:x(\d+))?\(S\))?$";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxPoolingLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        public MaxPoolingLayer(int[] inputShape, Kernel kernel)
            : base(inputShape, kernel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxPoolingLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public MaxPoolingLayer(int[] inputShape, string architecture, RandomNumberGenerator random)
            : base(inputShape, MaxPoolingLayer.KernelFromArchitecture(architecture))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxPoolingLayer"/> class, using the existing <see cref="MaxPoolingLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="MaxPoolingLayer"/> to copy the data from.</param>
        public MaxPoolingLayer(MaxPoolingLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MaxPoolingLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private MaxPoolingLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "MP{0}",
            this.Kernel.ToString(this.Kernel.Width, this.Kernel.Height));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new MaxPoolingLayer(this);

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Unroll cycles to improve performance.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Unroll cycles to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.MaxPooling(xs[0], this.Kernel) };
        }

        /// <summary>
        /// Extracts the pooling kernel from layer architecture.
        /// </summary>
        /// <param name="architecture">The layer architecture.</param>
        /// <returns>The pooling kernel.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Kernel KernelFromArchitecture(string architecture)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, MaxPoolingLayer.ArchitecturePattern);
            return Layer.ParseKernel(groups, 2, null, false);
        }
    }
}
