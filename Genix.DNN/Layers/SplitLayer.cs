// -----------------------------------------------------------------------
// <copyright file="SplitLayer.cs" company="Noname, Inc.">
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
    /// Implements a split operation on a <see cref="Tensor"/>.
    /// </summary>
    public class SplitLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfOutputs">The number of output tensors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitLayer(int[] inputShape, int numberOfOutputs)
            : base(numberOfOutputs, inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitLayer"/> class, using the existing <see cref="SplitLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SplitLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitLayer(SplitLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SplitLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
