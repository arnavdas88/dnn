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
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="weightsShape">The dimensions of the layer's weights tensor.</param>
        /// <param name="hiddenShape">The dimensions of the layer's hidden weights tensor. Can be <b>null</b>.</param>
        /// <param name="biasesShape">The dimensions of the layer's biases tensor.</param>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RNNCell(
            int[] outputShape,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            int[] weightsShape,
            int[] hiddenShape,
            int[] biasesShape,
            RandomNumberGenerator random)
            : base(outputShape, numberOfNeurons, matrixLayout, weightsShape, biasesShape, random ?? new RandomRangeGenerator(-0.08f, 0.08f))
        {
            if (hiddenShape == null)
            {
                throw new ArgumentNullException(nameof(hiddenShape));
            }

            this.U = new Tensor("hidden weights", hiddenShape);
            this.U.Randomize(random ?? new RandomRangeGenerator(-0.08f, 0.08f));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNCell"/> class, using the existing <see cref="RNNCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="RNNCell"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RNNCell(RNNCell other) : base(other)
        {
            this.U = other.U?.Clone() as Tensor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RNNCell"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RNNCell()
        {
        }

        /// <summary>
        /// Gets the hidden weights for the layer.
        /// </summary>
        /// <value>
        /// The tensor that contains hidden weights for the layer.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Stands for hidden weights matrix according to DNN notation.")]
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
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<(Tensor, float, float)> EnumGradients()
        {
            return base.EnumGradients().Append((this.U, 1.0f, 1.0f));
        }
    }
}
