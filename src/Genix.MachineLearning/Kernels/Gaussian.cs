// -----------------------------------------------------------------------
// <copyright file="Gaussian.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    using System;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the Gaussian kernel.
    /// </summary>
    public class Gaussian
        : IKernel
    {
        [JsonProperty("sigma")]
        private readonly float sigma;

        [JsonProperty("gamma")]
        private readonly float gamma;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gaussian"/> class.
        /// </summary>
        /// <param name="sigma">The kernel's sigma parameter.</param>
        public Gaussian(float sigma)
        {
            this.sigma = sigma;
            this.gamma = 1.0f / (2.0f * sigma * sigma);
        }

        /// <inheritdoc />
        public float Execute(int length, float[] x, int offx, float[] y, int offy)
        {
            float norm = Vectors.EuclideanDistanceSquared(length, x, offx, y, offy);
            return (float)Math.Exp(-this.gamma * norm);
        }
    }
}
