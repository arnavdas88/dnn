// -----------------------------------------------------------------------
// <copyright file="LossLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the loss layer. This is an abstract class.
    /// </summary>
    /// <seealso cref="SoftMaxLayer"/>
    public abstract class LossLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LossLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LossLayer(int[] inputShape)
            : base(1, LossLayer.CalculateOutputShape(inputShape))
        {
            this.NumberOfClasses = this.OutputShape.Skip(1).Aggregate(1, (total, next) => total * next);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LossLayer"/> class, using the existing <see cref="LossLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LossLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LossLayer(LossLayer other) : base(other)
        {
            this.NumberOfClasses = other.NumberOfClasses;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LossLayer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LossLayer()
        {
        }

        /// <summary>
        /// Gets the number of discrete classes the layer predicts.
        /// </summary>
        /// <value>
        /// The number of discrete classes.
        /// </value>
        [JsonProperty("NumberOfClasses")]
        public int NumberOfClasses { get; private set; }

        /// <summary>
        /// Gets or sets the masking tensor that the input tensor is multiplied by before calculations.
        /// </summary>
        /// <value>
        /// The <see cref="Tensor"/> object.
        /// </value>
        internal Tensor Mask { get; set; }

        /// <summary>
        /// Computes the dimensions of the layer's source and destination tensors.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <returns>
        /// The dimensions of the layer's destination tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);

            return new[] { inputShape[0], mbsize };
        }
    }
}
