// -----------------------------------------------------------------------
// <copyright file="FullyConnectedLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Declares a layer of neurons that perform weighted addition of all inputs (activations on layer below).
    /// </summary>
    public class FullyConnectedLayer : StochasticLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullyConnectedLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FullyConnectedLayer(
            int[] inputShape,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
            : base(
                  FullyConnectedLayer.CalculateOutputShape(inputShape, numberOfNeurons),
                  numberOfNeurons,
                  matrixLayout,
                  FullyConnectedLayer.CalculateWeightsShape(inputShape, numberOfNeurons, matrixLayout),
                  FullyConnectedLayer.CalculateBiasesShape(numberOfNeurons),
                  random)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullyConnectedLayer"/> class, using the existing <see cref="FullyConnectedLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="FullyConnectedLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FullyConnectedLayer(FullyConnectedLayer other) : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="FullyConnectedLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Computes the dimensions of the layer's destination tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <returns>
        /// The dimensions of the layer's destination tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape, int numberOfNeurons)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            return new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }

        /// <summary>
        /// Computes the dimensions of the layer's weights tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrix is row-major or column-major.</param>
        /// <returns>
        /// The dimensions of the layer's weights tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateWeightsShape(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout)
        {
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);

            if (matrixLayout == MatrixLayout.ColumnMajor)
            {
                // column-major matrix organization - each row contains all weights for one neuron
                return new[] { mbsize, numberOfNeurons };
            }
            else
            {
                // row-major matrix organization - each column contains all weights for one neuron
                return new[] { numberOfNeurons, mbsize };
            }
        }

        /// <summary>
        /// Computes the dimensions of the layer's biases tensor.
        /// </summary>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <returns>
        /// The dimensions of the layer's biases tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateBiasesShape(int numberOfNeurons)
        {
            return new[] { numberOfNeurons };
        }
    }
}
