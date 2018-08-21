// -----------------------------------------------------------------------
// <copyright file="FullyConnectedLayer.cs" company="Noname, Inc.">
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
    /// Declares a layer of neurons that perform weighted addition of all inputs (activations on layer below).
    /// </summary>
    public class FullyConnectedLayer : StochasticLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(N)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="FullyConnectedLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public FullyConnectedLayer(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout, RandomNumberGenerator<float> random)
        {
            this.Initialize(inputShape, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullyConnectedLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public FullyConnectedLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
        {
            List<Group> groups = Layer.ParseArchitecture(architecture, FullyConnectedLayer.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            this.Initialize(inputShape, numberOfNeurons, MatrixLayout.RowMajor, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullyConnectedLayer"/> class, using the existing <see cref="FullyConnectedLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="FullyConnectedLayer"/> to copy the data from.</param>
        public FullyConnectedLayer(FullyConnectedLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="FullyConnectedLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private FullyConnectedLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "{0}N", this.NumberOfNeurons);

        /// <inheritdoc />
        internal override bool NeedsActivation => true;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new FullyConnectedLayer(this);

        /// <summary>
        /// Initializes the <see cref="FullyConnectedLayer"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout, RandomNumberGenerator<float> random)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { mbsize, numberOfNeurons } :
                new[] { numberOfNeurons, mbsize };

            int[] biasesShape = new[] { numberOfNeurons };

            this.Initialize(numberOfNeurons, matrixLayout, weightsShape, biasesShape, random);
            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }
    }
}
