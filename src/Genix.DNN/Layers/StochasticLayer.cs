// -----------------------------------------------------------------------
// <copyright file="StochasticLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the layer of randomly initialized and trainable neurons. This is an abstract class.
    /// </summary>
    /// <seealso cref="FullyConnectedLayer"/>
    /// <seealso cref="ConvolutionLayer"/>
    /// <seealso cref="RNNCell"/>
    public abstract class StochasticLayer : TrainableLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticLayer"/> class.
        /// </summary>
        /// <param name="outputShape">The dimensions of the layer's output tensor.</param>
        /// <remarks>
        /// After using this constructor, the <see cref="StochasticLayer"/> should be initialized by calling <see cref="Initialize"/> method.
        /// </remarks>
        protected StochasticLayer(int[] outputShape)
            : base(outputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticLayer"/> class, using the existing <see cref="StochasticLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="StochasticLayer"/> to copy the data from.</param>
        protected StochasticLayer(StochasticLayer other)
            : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.NumberOfNeurons = other.NumberOfNeurons;
            this.MatrixLayout = other.MatrixLayout;

            this.W = other.W?.Clone() as Tensor;
            this.B = other.B?.Clone() as Tensor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticLayer"/> class.
        /// </summary>
        protected StochasticLayer()
        {
        }

        /// <summary>
        /// Gets the number of neurons in the layer.
        /// </summary>
        /// <value>
        /// The number of neurons.
        /// </value>
        [JsonProperty("NumberOfNeurons")]
        public int NumberOfNeurons { get; private set; }

        /// <summary>
        /// Gets the value indicating the layout of weight matrices.
        /// </summary>
        /// <value>
        /// The <see cref="Genix.Core.MatrixLayout"/> enumeration.
        /// </value>
        [JsonProperty("matrixLayout")]
        public MatrixLayout MatrixLayout { get; private set; }

        /// <summary>
        /// Gets the weights for the layer.
        /// </summary>
        /// <value>
        /// The tensor that contains weights for the layer.
        /// </value>
        [JsonProperty("W")]
        public Tensor W { get; private set; }

        /// <summary>
        /// Gets the biases for the layer.
        /// </summary>
        /// <value>
        /// The <see cref="Tensor"/> object that contains biases for the layer.
        /// </value>
        [JsonProperty("B")]
        public Tensor B { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void OptimizeForLearning()
        {
            // learning is faster when MatrixLayout is RowMajor
            if (this.MatrixLayout == MatrixLayout.ColumnMajor)
            {
                this.W.Transpose(MatrixLayout.ColumnMajor);
                this.MatrixLayout = MatrixLayout.RowMajor;
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void OptimizeForTesting()
        {
            // testing is faster when MatrixLayout is ColumnMajor
            if (this.MatrixLayout == MatrixLayout.RowMajor)
            {
                this.W.Transpose(MatrixLayout.RowMajor);
                this.MatrixLayout = MatrixLayout.ColumnMajor;
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // calculate output tensor in column-major mode
            // y += W * x (product of weight and input matrices)
            // input and output matrices are column major (one column per mini-batch item)
            // weights matrix might have to be transposed to have a row per neuron
            return new[]
            {
                session.MxM(
                    MatrixLayout.ColumnMajor,
                    this.W,
                    this.MatrixLayout == MatrixLayout.RowMajor,
                    xs[0],
                    false,
                    this.B),
            };
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<(Tensor, float, float)> EnumGradients()
        {
            yield return (this.W, 1.0f, 1.0f);
            yield return (this.B, 0.0f, 0.0f);
        }

        /// <summary>
        /// Initializes the <see cref="StochasticLayer"/>.
        /// </summary>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="weightsShape">The dimensions of the layer's weights tensor.</param>
        /// <param name="biasesShape">The dimensions of the layer's biases tensor.</param>
        /// <param name="random">The random numbers generator.</param>
        private protected void Initialize(
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            int[] weightsShape,
            int[] biasesShape,
            RandomNumberGenerator<float> random)
        {
            if (weightsShape == null)
            {
                throw new ArgumentNullException(nameof(weightsShape));
            }

            if (biasesShape == null)
            {
                throw new ArgumentNullException(nameof(biasesShape));
            }

            this.NumberOfNeurons = numberOfNeurons;
            this.MatrixLayout = matrixLayout;

            this.W = new Tensor("weights", weightsShape);
            this.W.Randomize(random ?? new GaussianGenerator(0.0, Math.Sqrt((double)numberOfNeurons / this.W.Length)));

            this.B = new Tensor("biases", biasesShape);
        }
    }
}
