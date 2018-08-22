// -----------------------------------------------------------------------
// <copyright file="RMSProp.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// RMSProp (Root Mean Square Propagation) algorithm for training neural nets.
    /// </summary>
    public class RMSProp : ITrainingAlgorithm
    {
        /// <summary>
        /// The weights accumulator.
        /// </summary>
        private readonly GradientAccumulators<float[]> accumulators = new GradientAccumulators<float[]>();

        /// <summary>
        /// Gets or sets the learning rate for the RMSProp algorithm.
        /// </summary>
        /// <value>
        /// The learning rate for the RMSProp algorithm. Default value is 0.001f.
        /// </value>
        public float LearningRate { get; set; } = 0.001f;

        /// <summary>
        /// Gets or sets the rho parameter for the RMSProp algorithm.
        /// </summary>
        /// <value>
        /// The the rho parameter for the RMSProp algorithm. Default value is 0.95f.
        /// </value>
        public float Rho { get; set; } = 0.95f;

        /// <summary>
        /// Gets or sets the epsilon parameter for the RMSProp algorithm.
        /// </summary>
        /// <value>
        /// The epsilon parameter for the RMSProp algorithm. Default value is 1e-8f.
        /// </value>
        public float Eps { get; set; } = 1e-8f;

        /// <inheritdoc />
        public void ComputeDeltas(int epoch, float[] gradient, int totalSamples)
        {
            float[] gsum = this.accumulators.GetAccumulator(
                gradient,
                g => new float[g.Length]);

            float learningRate = -this.LearningRate;
            float rho = this.Rho;
            float rhoinv = 1.0f - rho;
            float eps = this.Eps;

            for (int i = 0, ii = gradient.Length; i < ii; i++)
            {
                float g = gradient[i];

                gsum[i] = (rho * gsum[i]) + (rhoinv * g * g);
                gradient[i] = learningRate * g / ((float)Math.Sqrt(gsum[i]) + eps);
            }
        }
    }
}
