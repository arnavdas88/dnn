// -----------------------------------------------------------------------
// <copyright file="ConvolutionLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// This layer is almost identical to fully connected layer shown above,
    /// but neurons are connected only locally to a few neurons in the layer below (not all of them), and their parameters are shared.
    /// </summary>
    public sealed class ConvolutionLayer : StochasticLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvolutionLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfFilters">The number of filters in the layer.</param>
        /// <param name="kernel">The convolution kernel.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConvolutionLayer(
            int[] inputShape,
            int numberOfFilters,
            Kernel kernel,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
            : base(
                  ConvolutionLayer.CalculateOutputShape(inputShape, numberOfFilters, kernel),
                  numberOfFilters,
                  matrixLayout,
                  ConvolutionLayer.CalculateWeightsShape(inputShape, numberOfFilters, kernel, matrixLayout),
                  ConvolutionLayer.CalculateBiasesShape(numberOfFilters),
                  random)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            this.Kernel = kernel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvolutionLayer"/> class, using the existing <see cref="ConvolutionLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ConvolutionLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConvolutionLayer(ConvolutionLayer other) : base(other)
        {
            this.Kernel = other.Kernel;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ConvolutionLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private ConvolutionLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "{0}C{1}",
            this.NumberOfNeurons,
            this.Kernel.ToString(1, 1));

        /// <summary>
        /// Gets the convolution kernel.
        /// </summary>
        /// <value>
        /// The <see cref="DNN.Kernel"/> object.
        /// </value>
        [JsonProperty("Kernel")]
        public Kernel Kernel { get; private set; }

        /// <inheritdoc />
        internal override bool NeedsActivation => true;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new ConvolutionLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            Tensor x = xs[0];

            IList<Tensor> ys;
            if (this.Kernel.SameAsInput(x.Axes[(int)Axis.X], x.Axes[(int)Axis.Y]))
            {
                ys = base.Forward(session, xs);
            }
            else
            {
                Tensor temp = session.StackKernels(x, this.Kernel);
                ys = base.Forward(session, new[] { temp });
            }

            // reshape the tensor so it matches the layer output
            session.ReshapeIP(ys[0], ConvolutionLayer.CalculateOutputShape(x.Axes, this.NumberOfNeurons, this.Kernel));

            return ys;
        }

        /// <summary>
        /// Computes the dimensions of the layer's output tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfFilters">The number of filters in the layer.</param>
        /// <param name="kernel">The convolution kernel.</param>
        /// <returns>
        /// The dimensions of the layer's output tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape, int numberOfFilters, Kernel kernel)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            return new[]
            {
                inputShape[(int)Axis.B],
                kernel.CalculateOutputWidth(inputShape[(int)Axis.X]),
                kernel.CalculateOutputHeight(inputShape[(int)Axis.Y]),
                numberOfFilters
            };
        }

        /// <summary>
        /// Computes the dimensions of the layer's weights tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfFilters">The number of filters in the layer.</param>
        /// <param name="kernel">The convolution kernel.</param>
        /// <param name="matrixLayout">Specifies whether the matrix is row-major or column-major.</param>
        /// <returns>
        /// The dimensions of the layer's weights tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateWeightsShape(int[] inputShape, int numberOfFilters, Kernel kernel, MatrixLayout matrixLayout)
        {
            int mbsize = kernel.Size * inputShape[(int)Axis.C];

            if (matrixLayout == MatrixLayout.ColumnMajor)
            {
                // column-major matrix organization - each row contains all weights for one neuron
                return new[] { mbsize, numberOfFilters };
            }
            else
            {
                // row-major matrix organization - each column contains all weights for one neuron
                return new[] { numberOfFilters, mbsize };
            }
        }

        /// <summary>
        /// Computes the dimensions of the layer's biases tensor.
        /// </summary>
        /// <param name="numberOfFilters">The number of filters in the layer.</param>
        /// <returns>
        /// The dimensions of the layer's biases tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateBiasesShape(int numberOfFilters)
        {
            return new[] { numberOfFilters };
        }
    }
}
