// -----------------------------------------------------------------------
// <copyright file="RNNCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Base recurrent network cell. This is an abstract class.
    /// </summary>
    /// <seealso cref="SRNCell"/>
    /// <seealso cref="LSTMCell"/>
    /// <seealso cref="GRUCell"/>
    public abstract class RNNCell : StochasticLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RNNCell"/> class.
        /// </summary>
        /// <param name="outputShape">The dimensions of the layer's output tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        protected RNNCell(int[] outputShape, RNNCellDirection direction)
            : base(outputShape)
        {
            this.Direction = direction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNCell"/> class, using the existing <see cref="RNNCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="RNNCell"/> to copy the data from.</param>
        protected RNNCell(RNNCell other)
            : base(other)
        {
            this.Direction = other.Direction;
            this.U = other.U?.Clone() as Tensor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNCell"/> class.
        /// </summary>
        protected RNNCell()
        {
        }

        /// <summary>
        /// Gets the direction of this <see cref="RNNCell"/> (forward-only or bi-directional).
        /// </summary>
        /// <value>
        /// The <see cref="RNNCellDirection"/> enumeration.
        /// </value>
        [JsonProperty("direction")]
        public RNNCellDirection Direction { get; private set; } = RNNCellDirection.ForwardOnly;

        /// <summary>
        /// Gets the hidden weights for the layer.
        /// </summary>
        /// <value>
        /// The tensor that contains hidden weights for the layer.
        /// </value>
        [JsonProperty("U")]
        public Tensor U { get; private set; }

        /// <inheritdoc />
        internal override bool NeedsActivation => false;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void OptimizeForLearning()
        {
            // learning is faster when MatrixLayout is RowMajor
            if (this.MatrixLayout == MatrixLayout.ColumnMajor)
            {
                this.U.Transpose(MatrixLayout.ColumnMajor);
                base.OptimizeForLearning();
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void OptimizeForTesting()
        {
            // testing is faster when MatrixLayout is ColumnMajor
            if (this.MatrixLayout == MatrixLayout.RowMajor)
            {
                this.U.Transpose(MatrixLayout.RowMajor);
                base.OptimizeForTesting();
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<(Tensor, float, float)> EnumGradients()
        {
            return base.EnumGradients().Append((this.U, 1.0f, 1.0f));
        }

        /// <summary>
        /// Initializes the <see cref="StochasticLayer"/>.
        /// </summary>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="weightsShape">The dimensions of the layer's weights tensor.</param>
        /// <param name="hiddenShape">The dimensions of the layer's hidden weights tensor.</param>
        /// <param name="biasesShape">The dimensions of the layer's biases tensor.</param>
        /// <param name="random">The random numbers generator.</param>
        private protected void Initialize(
            RNNCellDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            int[] weightsShape,
            int[] hiddenShape,
            int[] biasesShape,
            RandomNumberGenerator<float> random)
        {
            if (hiddenShape == null)
            {
                throw new ArgumentNullException(nameof(hiddenShape));
            }

            if (random == null)
            {
                random = new RandomRangeGenerator(-0.08f, 0.08f);
            }

            if (direction == RNNCellDirection.BiDirectional && (numberOfNeurons % 2) != 0)
            {
                throw new ArgumentException("The number of neurons in a bi-directional RNN must be even.", nameof(numberOfNeurons));
            }

            this.Direction = direction;

            this.Initialize(numberOfNeurons, matrixLayout, weightsShape, biasesShape, random);

            this.U = new Tensor("hidden weights", hiddenShape);
            this.U.Randomize(random);
        }
    }
}
