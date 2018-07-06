﻿// -----------------------------------------------------------------------
// <copyright file="SRNLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Simple recurrent network (SRN) layer.
    /// </summary>
    public class SRNLayer : RNNLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SRNLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the hidden and fully connected layers.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SRNLayer(
            int[] inputShape,
            IList<int> numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
            : base(inputShape)
        {
            if (numberOfNeurons == null)
            {
                throw new ArgumentNullException(nameof(numberOfNeurons));
            }

            if (numberOfNeurons.Count < 2)
            {
                throw new ArgumentException("Recurrent neural network must have at least two layers.");
            }

            // create layers
            List<Layer> layers = new List<Layer>(numberOfNeurons.Count);

            int[] il = inputShape;
            for (int i = 0, ii = numberOfNeurons.Count; i < ii; i++)
            {
                Layer layer = i + 1 < ii ?
                    new SRNCell(il, numberOfNeurons[i], matrixLayout, random) as Layer :
                    new FullyConnectedLayer(il, numberOfNeurons[i], matrixLayout, random) as Layer;

                layers.Add(layer);
                il = layer.OutputShape;
            }

            // build SRN graph
            this.Graph.AddEdges(layers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNLayer"/> class, using the existing <see cref="SRNLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SRNLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SRNLayer(SRNLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SRNLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SRNLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "{0}SRN",
            string.Join("-", this.Graph.Vertices.Select(x => ((StochasticLayer)x).NumberOfNeurons)));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SRNLayer(this);
    }
}
