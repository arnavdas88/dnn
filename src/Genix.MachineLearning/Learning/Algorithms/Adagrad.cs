// -----------------------------------------------------------------------
// <copyright file="Adagrad.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Adagrad (Adaptive Gradient) algorithm for training neural nets.
    /// </summary>
    public class Adagrad : ITrainingAlgorithm
    {
        /// <summary>
        /// The weights accumulator.
        /// </summary>
        private readonly GradientAccumulators<float[]> accumulators = new GradientAccumulators<float[]>();

        /// <summary>
        /// Gets or sets the learning rate for the Adagrad algorithm.
        /// </summary>
        /// <value>
        /// The learning rate for the Adagrad algorithm. Default value is 0.01f.
        /// </value>
        public float LearningRate { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the epsilon parameter for the Adagrad algorithm.
        /// </summary>
        /// <value>
        /// The epsilon parameter for the Adam algorithm. Default value is 1e-8f.
        /// </value>
        public float Eps { get; set; } = 1e-8f;

        /// <inheritdoc />
        public void ComputeDeltas(int epoch, float[] gradient, int totalSamples)
        {
            float[] gsum = this.accumulators.GetAccumulator(
                gradient,
                g => new float[g.Length]);

            float learningRate = -this.LearningRate;
            float eps = this.Eps;

            for (int i = 0, ii = gradient.Length; i < ii; i++)
            {
                float g = gradient[i];

                gsum[i] += g * g;
                gradient[i] = learningRate * g / ((float)Math.Sqrt(gsum[i]) + eps);
            }
        }
    }
}
