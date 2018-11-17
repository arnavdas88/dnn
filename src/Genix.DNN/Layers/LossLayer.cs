// -----------------------------------------------------------------------
// <copyright file="LossLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.MachineLearning;
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
        /// <param name="shape">The shape of the layer's input tensor.</param>
        protected LossLayer(Shape shape)
            : base(1, LossLayer.CalculateOutputShape(shape))
        {
            this.NumberOfClasses = this.OutputShape.Axes.Skip(1).Aggregate(1, (total, next) => total * next);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LossLayer"/> class, using the existing <see cref="LossLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LossLayer"/> to copy the data from.</param>
        protected LossLayer(LossLayer other)
            : base(other)
        {
            this.NumberOfClasses = other.NumberOfClasses;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LossLayer"/> class.
        /// </summary>
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
        /// Computes the shape of the layer's source and destination tensors.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <returns>
        /// The shape of the layer's destination tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Shape CalculateOutputShape(Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            int[] axes = shape.Axes;
            int mbsize = axes.Skip(1).Aggregate(1, (total, next) => total * next);

            return new Shape(new int[] { axes[0], mbsize });
        }
    }
}
