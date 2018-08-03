// -----------------------------------------------------------------------
// <copyright file="LRNLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Performs local response normalization.
    /// </summary>
    public sealed class LRNLayer : Layer
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(LRN)(\d+)(?:\(A=((?:\d*\.)?\d+);B=((?:\d*\.)?\d+);K=((?:\d*\.)?\d+)\))?$";

        /// <summary>
        /// The default value for kernel size.
        /// </summary>
        public const int DefaultKernelSize = 5;

        /// <summary>
        /// The default value for α parameter.
        /// </summary>
        public const float DefaultAlpha = 1e-4f;

        /// <summary>
        /// The default value for β parameter.
        /// </summary>
        public const float DefaultBeta = 0.75f;

        /// <summary>
        /// The default value for k parameter.
        /// </summary>
        public const float DefaultK = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRNLayer"/> class, using default parameters.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernelSize">The number of channels to normalize across. Should be odd number.</param>
        public LRNLayer(int[] inputShape, int kernelSize)
            : this(inputShape, kernelSize, LRNLayer.DefaultAlpha, LRNLayer.DefaultBeta, LRNLayer.DefaultK)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRNLayer"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="kernelSize">The number of channels to normalize across. Should be odd number.</param>
        /// <param name="alpha">The α parameter.</param>
        /// <param name="beta">The β parameter.</param>
        /// <param name="k">The k parameter.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a notation used in algorithm description.")]
        public LRNLayer(int[] inputShape, int kernelSize, float alpha, float beta, float k)
            : base(1, inputShape)
        {
            if (kernelSize < 3 || (kernelSize % 2) == 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidLRNKernelSize, nameof(kernelSize));
            }

            this.KernelSize = kernelSize;
            this.Alpha = alpha;
            this.Beta = beta;
            this.K = k;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRNLayer"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public LRNLayer(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
            : base(1, inputShape)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, LRNLayer.ArchitecturePattern);

            this.KernelSize = Convert.ToInt32(groups[2].Value, CultureInfo.InvariantCulture);
            if (this.KernelSize < 3 || (this.KernelSize % 2) == 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidLRNKernelSize, nameof(architecture));
            }

            if (!string.IsNullOrEmpty(groups[3].Value) &&
                !string.IsNullOrEmpty(groups[4].Value) &&
                !string.IsNullOrEmpty(groups[5].Value))
            {
                this.Alpha = Convert.ToSingle(groups[3].Value, CultureInfo.InvariantCulture);
                this.Beta = Convert.ToSingle(groups[4].Value, CultureInfo.InvariantCulture);
                this.K = Convert.ToSingle(groups[5].Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRNLayer"/> class, using the existing <see cref="LRNLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LRNLayer"/> to copy the data from.</param>
        public LRNLayer(LRNLayer other)
            : base(other)
        {
            this.KernelSize = other.KernelSize;
            this.Alpha = other.Alpha;
            this.Beta = other.Beta;
            this.K = other.K;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LRNLayer"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private LRNLayer()
        {
        }

        /// <inheritdoc />
        public override string Architecture
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "LRN{0}", this.KernelSize);

                if (this.Alpha != LRNLayer.DefaultAlpha || this.Beta != LRNLayer.DefaultBeta || this.K != LRNLayer.DefaultK)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "(A={0};B={1};K={2})", this.Alpha, this.Beta, this.K);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the number of channels to normalize across.
        /// </summary>
        /// <value>
        /// The number of channels to normalize across.
        /// </value>
        [JsonProperty("KernelSize")]
        public int KernelSize { get; private set; } = LRNLayer.DefaultKernelSize;

        /// <summary>
        /// Gets the α parameter.
        /// </summary>
        /// <value>
        /// The α parameter.
        /// </value>
        [JsonProperty("Alpha")]
        public float Alpha { get; private set; } = LRNLayer.DefaultAlpha;

        /// <summary>
        /// Gets the β parameter.
        /// </summary>
        /// <value>
        /// The β parameter.
        /// </value>
        [JsonProperty("Beta")]
        public float Beta { get; private set; } = LRNLayer.DefaultBeta;

        /// <summary>
        /// Gets the k parameter.
        /// </summary>
        /// <value>
        /// The k parameter.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a notation used in algorithm description.")]
        [JsonProperty("K")]
        public float K { get; private set; } = LRNLayer.DefaultK;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new LRNLayer(this);

        /// <inheritdoc />
        /// <remarks>
        /// The method performs operation defined as:
        /// y(i) = x(i) * (k + (alpha / kernelSize) * sum(x(j) ^ 2)) ^ -beta.
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Unroll cycles to improve performance.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Unroll cycles to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            return new[] { session.LRN(xs[0], this.KernelSize, this.Alpha, this.Beta, this.K) };
        }
    }
}
