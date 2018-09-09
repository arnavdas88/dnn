// -----------------------------------------------------------------------
// <copyright file="SGD.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System.Diagnostics.CodeAnalysis;
    using Genix.Core;

    /// <summary>
    /// SGD (Stochastic Gradient Descent) algorithm for training neural nets.
    /// </summary>
    public class SGD : ITrainingAlgorithm
    {
        /// <summary>
        /// The weights accumulator.
        /// </summary>
        private readonly GradientAccumulators<float[]> accumulators = new GradientAccumulators<float[]>();

        /// <summary>
        /// Gets or sets the learning rate for the SGD algorithm.
        /// </summary>
        /// <value>
        /// The learning rate for the SGD algorithm. Default value is 0.01f.
        /// </value>
        public float LearningRate { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the learning rate decay for the SGD algorithm.
        /// </summary>
        /// <value>
        /// The learning rate decay for the SGD algorithm. Default value is 0.0f.
        /// After each epoch, the current learning rate is calculated as:
        /// rate := LearningRate / (1.0f + Decay * epoch).
        /// </value>
        public float Decay { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the momentum for the SGD algorithm.
        /// </summary>
        /// <value>
        /// The momentum for the SGD algorithm. Default value is 0.9f.
        /// </value>
        public float Momentum { get; set; } = 0.9f;

        /// <summary>
        /// Gets or sets a value indicating whether to apply Nesterov momentum.
        /// </summary>
        /// <value>
        /// <b>true</b> to apply Nesterov momentum; otherwise, <b>false</b>. Default value is 0.9f.
        /// </value>
        public bool Nesterov { get; set; } = false;

        /// <inheritdoc />
        public void ComputeDeltas(int epoch, float[] gradient, int totalSamples)
        {
            float learningRate = this.LearningRate;
            if (this.Decay != 0.0f && epoch > 0)
            {
                learningRate /= 1.0f + (this.Decay * epoch);
            }

            float momentum = this.Momentum;
            if (momentum > 0.0f)
            {
                // get accumulator
                float[] velocity = this.accumulators.GetAccumulator(
                    gradient,
                    g => new float[g.Length]);

                if (this.Nesterov)
                {
                    // apply Nesterov momentum
                    // dx = velocity = momentum^2 * velocity - (1 + momentum) * learningRate * g
                    Mathematics.MultiplyAndAdd(gradient.Length, momentum * momentum, velocity, 0, -(1.0f + momentum) * learningRate, gradient, 0);
                    Vectors.Copy(gradient.Length, gradient, 0, velocity, 0);
                }
                else
                {
                    // momentum update
                    // dx = velocity = momentum * velocity - learningRate * g
                    Mathematics.MultiplyAndAdd(gradient.Length, momentum, velocity, 0, -learningRate, gradient, 0);
                    Vectors.Copy(gradient.Length, gradient, 0, velocity, 0);
                }
            }
            else
            {
                // vanilla sgd
                // dx = -learningRate * g
                Vectors.MulC(gradient.Length, -learningRate, gradient, 0);
            }
        }
    }
}
