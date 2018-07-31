// -----------------------------------------------------------------------
// <copyright file="TanhLayer.cs" company="Noname, Inc.">
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
    /// Implements Tanh nonlinearity element wise
    /// x -> tanh(x)
    /// so the output is between -1 and 1.
    /// </summary>
    public class TanhLayer : ActivationLayer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^TH$";

        /// <summary>
        /// Initializes a new instance of the <see cref="TanhLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        public TanhLayer(int[] inputShape)
            : base(inputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TanhLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public TanhLayer(int[] inputShape, string architecture, RandomNumberGenerator random)
            : base(inputShape)
        {
            Layer.ParseArchitechture(architecture, TanhLayer.ArchitecturePattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TanhLayer"/> class, using the existing <see cref="TanhLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="TanhLayer"/> to copy the data from.</param>
        public TanhLayer(TanhLayer other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TanhLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private TanhLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture => "TH";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new TanhLayer(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            session.TanhIP(xs[0]);
            return xs;
            ////return new[] { session.Tanh(xs[0]) };
        }
    }
}
