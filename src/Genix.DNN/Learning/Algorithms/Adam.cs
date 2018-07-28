// -----------------------------------------------------------------------
// <copyright file="Adam.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Adam (Adaptive Moment Estimation) algorithm for training neural nets.
    /// </summary>
    public class Adam : ITrainingAlgorithm
    {
        /// <summary>
        /// The weights accumulator.
        /// </summary>
#pragma warning disable SA1009 // Closing parenthesis must be spaced correctly
        private readonly GradientAccumulators<(float[], float[])> accumulators = new GradientAccumulators<(float[], float[])>();
#pragma warning restore SA1009 // Closing parenthesis must be spaced correctly

        /// <summary>
        /// Gets or sets the learning rate for the Adam algorithm.
        /// </summary>
        /// <value>
        /// The learning rate for the Adam algorithm. Default value is 0.01f.
        /// </value>
        public float LearningRate { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the beta1 parameter for the Adam algorithm.
        /// </summary>
        /// <value>
        /// The the beta1 parameter for the Adam algorithm. Default value is 0.9f.
        /// </value>
        public float Beta1 { get; set; } = 0.9f;

        /// <summary>
        /// Gets or sets the beta2 parameter for the Adam algorithm.
        /// </summary>
        /// <value>
        /// The the beta2 parameter for the Adam algorithm. Default value is 0.999f.
        /// </value>
        public float Beta2 { get; set; } = 0.999f;

        /// <summary>
        /// Gets or sets the epsilon parameter for the Adam algorithm.
        /// </summary>
        /// <value>
        /// The epsilon parameter for the Adam algorithm. Default value is 1e-8f.
        /// </value>
        public float Eps { get; set; } = 1e-8f;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Skip argument validation to improve performance.")]
        public void ComputeDeltas(int epoch, float[] gradient, int totalSamples)
        {
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            (float[] gsum, float[] xsum) = this.accumulators.GetAccumulator(
                gradient,
                g => (new float[g.Length], new float[g.Length]));
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly

            float learningRate = -this.LearningRate;
            float beta1 = this.Beta1;
            float beta1inv = 1.0f - beta1;
            float beta1bias = 1.0f - (float)Math.Pow(beta1, totalSamples);
            float beta2 = this.Beta2;
            float beta2inv = 1.0f - beta2;
            float beta2bias = 1.0f - (float)Math.Pow(beta2, totalSamples);
            float eps = this.Eps;

            for (int i = 0, ii = gradient.Length; i < ii; i++)
            {
                float g = gradient[i];

                gsum[i] = (beta1 * gsum[i]) + (beta1inv * g); // update biased first moment estimate
                xsum[i] = (beta2 * xsum[i]) + (beta2inv * g * g); // update biased second moment estimate
                float bias1 = gsum[i] * beta1bias; // correct bias first moment estimate
                float bias2 = xsum[i] * beta2bias; // correct bias second moment estimate

                gradient[i] = learningRate * bias1 / ((float)Math.Sqrt(bias2) + eps);
            }
        }
    }
}
