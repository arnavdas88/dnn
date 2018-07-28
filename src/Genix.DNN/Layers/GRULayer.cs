// -----------------------------------------------------------------------
// <copyright file="GRULayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Gated recurrent unit (GRU) layer.
    /// </summary>
    public class GRULayer : RNNLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(-\d+)+(GRU)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="GRULayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the hidden and fully connected layers.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRULayer(int[] inputShape, IList<int> numberOfNeurons, MatrixLayout matrixLayout, RandomNumberGenerator random)
        {
            this.Initialize(inputShape, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRULayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRULayer(int[] inputShape, string architecture, RandomNumberGenerator random)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, GRULayer.ArchitecturePattern);

            List<int> numberOfNeurons = new List<int>()
            {
                Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture),
            };

            foreach (Capture capture in groups[2].Captures)
            {
                numberOfNeurons.Add(Convert.ToInt32(capture.Value.TrimStart('-'), CultureInfo.InvariantCulture));
            }

            this.Initialize(inputShape, numberOfNeurons, MatrixLayout.RowMajor, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRULayer"/> class, using the existing <see cref="GRULayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="GRULayer"/> to copy the data from.</param>
        public GRULayer(GRULayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="GRULayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private GRULayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "{0}GRU",
            string.Join("-", this.Graph.Vertices.Select(x => ((StochasticLayer)x).NumberOfNeurons)));

        /// <inheritdoc />
        public override object Clone() => new GRULayer(this);

        /// <summary>
        /// Initializes the <see cref="GRULayer"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the hidden and fully connected layers.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            IList<int> numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

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

            int[] shape = inputShape;
            for (int i = 0, ii = numberOfNeurons.Count; i < ii; i++)
            {
                Layer layer = i + 1 < ii ?
                    new GRUCell(shape, numberOfNeurons[i], matrixLayout, random) as Layer :
                    new FullyConnectedLayer(shape, numberOfNeurons[i], matrixLayout, random) as Layer;

                layers.Add(layer);
                shape = layer.OutputShape;
            }

            // build GRU graph
            this.Graph.AddEdges(layers);

            // output shape is the output shape of the decoder
            this.OutputShape = shape;
        }
    }
}
