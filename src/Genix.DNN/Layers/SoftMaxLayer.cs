// -----------------------------------------------------------------------
// <copyright file="SoftMaxLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Predicts a set of discrete classes.
    /// The outputs are probabilities that sum to 1.
    /// </summary>
    public class SoftMaxLayer : LossLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^SM$";

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftMaxLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        public SoftMaxLayer(int[] inputShape)
            : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftMaxLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SoftMaxLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(inputShape)
        {
            Layer.ParseArchitechture(architecture, SoftMaxLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftMaxLayer"/> class, using the existing <see cref="SoftMaxLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SoftMaxLayer"/> to copy the data from.</param>
        public SoftMaxLayer(SoftMaxLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SoftMaxLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private SoftMaxLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "SM";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SoftMaxLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            Tensor x = xs[0];

            if (this.Mask != null)
            {
                if (x.Rank == 1)
                {
                    x = session.Multiply(x, this.Mask);
                }
                else
                {
                    int mb = x.Axes[0];
                    x = session.Multiply(x, session.Tile(this.Mask, 0, mb));
                }
            }

            return new[] { session.SoftMax(x) };
        }
    }
}
