// -----------------------------------------------------------------------
// <copyright file="ScaleLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements a scaling operation on a <see cref="Tensor"/>.
    /// A scaling operation is defined as follows: x -> alpha * x .
    /// </summary>
    public class ScaleLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="alpha">The scaling factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ScaleLayer(int[] inputShape, float alpha) : base(1, inputShape)
        {
            this.Alpha = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleLayer"/> class, using the existing <see cref="ScaleLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ScaleLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ScaleLayer(ScaleLayer other) : base(other)
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
