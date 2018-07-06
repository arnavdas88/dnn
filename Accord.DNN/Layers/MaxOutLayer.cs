// -----------------------------------------------------------------------
// <copyright file="MaxOutLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
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
        /// Initializes a new instance of the <see cref="MaxOutLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="groupSize">The number of neurons in maxout unit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MaxOutLayer(int[] inputShape, int groupSize)
            : base(MaxOutLayer.CalculateOutputShape(inputShape, groupSize))
        {
            this.GroupSize = groupSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxOutLayer"/> class, using the existing <see cref="MaxOutLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="MaxOutLayer"/> to copy the data from.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Validated by the base constructor.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MaxOutLayer(MaxOutLayer other) : base(other)
        {
            this.GroupSize = other.GroupSize;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MaxOutLayer"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private MaxOutLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "MO{0}", this.GroupSize);

        /// <summary>
        /// Gets the number of neurons in maxout unit.
        /// </summary>
        /// <value>
        /// The number of neurons in maxout unit.
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
            return new[] { session.MaxReduce(xs[0], (int)Axis.C, this.GroupSize) };
        }

        /// <summary>
        /// Computes the dimensions of the layer's output tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="groupSize">The number of neurons in maxout unit.</param>
        /// <returns>
        /// The dimensions of the layer's output tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape, int groupSize)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            if ((inputShape[(int)Axis.C] % groupSize) != 0)
            {
                throw new ArgumentException("The number of channels must be a multiple of a group size.");
            }

            return Shape.Reshape(inputShape, (int)Axis.C, inputShape[(int)Axis.C] / groupSize);
        }
    }
}
