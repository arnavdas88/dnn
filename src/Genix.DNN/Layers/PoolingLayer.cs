﻿// -----------------------------------------------------------------------
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
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="kernel">The pooling kernel.</param>
        protected PoolingLayer(Shape shape, Kernel kernel)
            : base(1, kernel.CalculateOutputShape(shape))
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
    }
}
