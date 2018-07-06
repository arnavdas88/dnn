// -----------------------------------------------------------------------
// <copyright file="TrainableLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the layer that can be trained. This is an abstract class.
    /// </summary>
    /// <seealso cref="StochasticLayer"/>
    /// <seealso cref="SRNLayer"/>
    /// <seealso cref="LSTMLayer"/>
    public abstract class TrainableLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrainableLayer"/> class.
        /// </summary>
        /// <param name="outputShape">The dimensions of the layer's output tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TrainableLayer(int[] outputShape)
            : base(1, outputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrainableLayer"/> class, using the existing <see cref="TrainableLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="TrainableLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TrainableLayer(TrainableLayer other) : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrainableLayer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TrainableLayer()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this layer should be trained.
        /// </summary>
        /// <value>
        /// <b>true</b> if this layer should be trained; otherwise, <b>false</b>.
        /// </value>
        public bool IsTrainable { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether this layer needs an activation layer after it.
        /// </summary>
        /// <value>
        /// <b>true</b> if this layer needs an activation layer; otherwise, <b>false</b>.
        /// </value>
        internal abstract bool NeedsActivation { get; }

        /// <summary>
        /// Returns a collection of trainable weights along with their gradients.
        /// </summary>
        /// <returns>The sequence of tuples that contains gradient tensors.</returns>
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        internal abstract IEnumerable<(Tensor, float, float)> EnumGradients();
    }
}
