// -----------------------------------------------------------------------
// <copyright file="AveragePoolingLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Performs average pooling.
    /// </summary>
    public sealed class AveragePoolingLayer : PoolingLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePoolingLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AveragePoolingLayer(int[] inputShape, Kernel kernel)
            : base(inputShape, kernel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePoolingLayer"/> class, using the existing <see cref="AveragePoolingLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="AveragePoolingLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AveragePoolingLayer(AveragePoolingLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AveragePoolingLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}
