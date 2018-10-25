// -----------------------------------------------------------------------
// <copyright file="LSTMLayer.cs" company="Noname, Inc.">
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
    /// Long short-term memory (LSTM) layer.
    /// </summary>
    public class LSTMLayer : RNNLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(?:-(\d+))+(LSTM)(?:\(([A-Za-z]+)=([0-9.]+)(?:,([A-Za-z]+)=([0-9.]+))*\))?$";

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the hidden and fully connected layers.</param>
        /// <param name="forgetBias">The bias added to forget gates.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public LSTMLayer(
            int[] inputShape,
            RNNDirection direction,
            IList<int> numberOfNeurons,
            float forgetBias,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            this.Initialize(inputShape, direction, numberOfNeurons, forgetBias, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public LSTMLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, LSTMLayer.ArchitecturePattern);

            List<int> numberOfNeurons = new List<int>()
            {
                Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture),
            };

            foreach (Capture capture in groups[2].Captures)
            {
                numberOfNeurons.Add(Convert.ToInt32(capture.Value, CultureInfo.InvariantCulture));
            }

            if (!Layer.TryParseArchitectureParameter(groups, "LSTM", "Bi", out RNNDirection direction))
            {
                direction = RNNDirection.ForwardOnly;
            }

            if (!Layer.TryParseArchitectureParameter(groups, "LSTM", "ForgetBias", out float forgetBias))
            {
                forgetBias = LSTMCell.DefaultForgetBias;
            }

            this.Initialize(
                inputShape,
                direction,
                numberOfNeurons,
                forgetBias,
                MatrixLayout.RowMajor,
                random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMLayer"/> class, using the existing <see cref="LSTMLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LSTMLayer"/> to copy the data from.</param>
        public LSTMLayer(LSTMLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LSTMLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private LSTMLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture
        {
            get
            {
                LSTMCell cell = this.Graph.Sources.First() as LSTMCell;

                List<string> prms = new List<string>();
                if (cell.Direction != RNNDirection.ForwardOnly)
                {
                    prms.Add("Bi=1");
                }

                if (cell.ForgetBias != LSTMCell.DefaultForgetBias)
                {
                    prms.Add(string.Format(CultureInfo.InvariantCulture, "ForgetBias={0}", cell.ForgetBias));
                }

                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}LSTM{1}",
                    string.Join("-", this.Graph.Vertices.Select(x => ((StochasticLayer)x).NumberOfNeurons)),
                    prms.Count > 0 ? "(" + string.Join(",", prms) + ")" : string.Empty);
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new LSTMLayer(this);

        /// <summary>
        /// Initializes the <see cref="GRULayer"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the hidden and fully connected layers.</param>
        /// <param name="forgetBias">The bias added to forget gates.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            RNNDirection direction,
            IList<int> numberOfNeurons,
            float forgetBias,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
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
                    new LSTMCell(shape, direction, numberOfNeurons[i], forgetBias, matrixLayout, random) as Layer :
                    new FullyConnectedLayer(shape, numberOfNeurons[i], matrixLayout, random) as Layer;

                layers.Add(layer);
                shape = layer.OutputShape;
            }

            // build LSTM graph
            this.Graph.AddEdges(layers);

            // output shape is the output shape of the decoder
            this.OutputShape = shape;
        }
    }
}
