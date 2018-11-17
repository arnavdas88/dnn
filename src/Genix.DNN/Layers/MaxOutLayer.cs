// -----------------------------------------------------------------------
// <copyright file="MaxOutLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements Maxout nonlinearity that computes
    /// x -> max(x)
    /// where x is a vector of size groupSize.
    /// Ideally of course, the input size should be exactly divisible by groupSize.
    /// </summary>
    /// <remarks>
    /// <para>
    /// References:
    /// <list type="bullet">
    /// <item><description>
    /// <a href="https://arxiv.org/pdf/1302.4389v4.pdf">https://arxiv.org/pdf/1302.4389v4.pdf</a>
    /// </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class MaxOutLayer : ActivationLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(MO)(\d+)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxOutLayer"/> class.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="groupSize">The number of neurons in max out unit.</param>
        public MaxOutLayer(Shape shape, int groupSize)
        {
            this.Initialize(shape, groupSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxOutLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public MaxOutLayer(Shape shape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, MaxOutLayer.ArchitecturePattern);
            int groupSize = Convert.ToInt32(groups[2].Value, CultureInfo.InvariantCulture);
            this.Initialize(shape, groupSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxOutLayer"/> class, using the existing <see cref="MaxOutLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="MaxOutLayer"/> to copy the data from.</param>
        public MaxOutLayer(MaxOutLayer other)
            : base(other)
        {
            this.GroupSize = other.GroupSize;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MaxOutLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private MaxOutLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "MO{0}", this.GroupSize);

        /// <summary>
        /// Gets the number of neurons in max out unit.
        /// </summary>
        /// <value>
        /// The number of neurons in max out unit.
        /// </value>
        [JsonProperty("GroupSize")]
        public int GroupSize { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new MaxOutLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // compute the channel axis
            int axis = xs[0].GetAxisIndex(Axis.C);

            return new[] { session.MaxReduce(xs[0], axis, this.GroupSize) };
        }

        /// <summary>
        /// Initializes the <see cref="MaxOutLayer"/>.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="groupSize">The number of neurons in max out unit.</param>
        private void Initialize(Shape shape, int groupSize)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            // compute the channel axis
            int axis = shape.GetAxis(Axis.C);
            if ((axis % groupSize) != 0)
            {
                throw new ArgumentException("The number of channels must be a multiple of a group size.");
            }

            this.GroupSize = groupSize;
            this.OutputShape = shape.Reshape(Axis.C, axis / groupSize);
        }
    }
}
