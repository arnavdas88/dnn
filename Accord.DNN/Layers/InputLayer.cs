// -----------------------------------------------------------------------
// <copyright file="InputLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// A dummy layer that essentially declares the size of input volume and must be first layer in the network.
    /// Inputs other than real-valued numbers are currently not supported.
    /// </summary>
    public class InputLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InputLayer(int[] inputShape) : base(1, inputShape)
        {
            this.Shape = inputShape;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayer"/> class, using the existing <see cref="InputLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="InputLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InputLayer(InputLayer other) : base(other)
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
        public override string Architecture => string.Join("x", this.Shape.Skip(1));

        /// <summary>
        /// Gets the dimensions of the layer's input tensor.
        /// </summary>
        /// <value>
        /// The array of <see cref="Shape"/> objects that contains layer dimensions.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonProperty("Shape")]
        public int[] Shape { get; private set; }

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

            int[] shape = this.Shape;
            Tensor x = xs[0];

            // validate tensor layout
            if (x.Rank != shape.Length)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_InvalidInputTensor_InvalidRank,
                    x.Rank,
                    shape.Length));
            }

            for (int i = 0, ii = shape.Length; i < ii; i++)
            {
                if (shape[i] >= 0 && shape[i] != x.Axes[i])
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidInputTensor_InvalidDimension,
                        x.Axes[i],
                        i,
                        shape[i]));
                }
            }

            return xs;
        }
    }
}
