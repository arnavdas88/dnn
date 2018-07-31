// -----------------------------------------------------------------------
// <copyright file="PoolingLayer.cs" company="Noname, Inc.">
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
    /// Represents the pooling layer. This is an abstract class.
    /// </summary>
    public abstract class PoolingLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoolingLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        protected PoolingLayer(int[] inputShape, Kernel kernel)
            : base(1, PoolingLayer.CalculateOutputShape(inputShape, kernel))
        {
            this.Kernel = kernel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolingLayer"/> class, using the existing <see cref="PoolingLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="PoolingLayer"/> to copy the data from.</param>
        protected PoolingLayer(PoolingLayer other)
            : base(other)
        {
            this.Kernel = other.Kernel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolingLayer"/> class.
        /// </summary>
        [JsonConstructor]
        protected PoolingLayer()
        {
        }

        /// <summary>
        /// Gets or sets the pooling kernel.
        /// </summary>
        /// <value>
        /// The <see cref="MachineLearning.Kernel"/> object.
        /// </value>
        [JsonProperty("Kernel")]
        public Kernel Kernel { get; protected set; }

        /// <summary>
        /// Computes the dimensions of the layer's output tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        /// <returns>
        /// The dimensions of the layer's output tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape, Kernel kernel)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            int[] axes = inputShape.ToArray();
            axes[(int)Axis.X] = kernel.CalculateOutputWidth(axes[(int)Axis.X]);
            axes[(int)Axis.Y] = kernel.CalculateOutputHeight(axes[(int)Axis.Y]);

            return axes;
        }
    }
}
