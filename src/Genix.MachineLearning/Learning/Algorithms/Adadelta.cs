// -----------------------------------------------------------------------
// <copyright file="Adadelta.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// ADADELTA algorithm for training neural nets.
    /// </summary>
    public class Adadelta : ITrainingAlgorithm
    {
        /// <summary>
        /// The weights accumulator.
        /// </summary>
        private readonly GradientAccumulators<(float[], float[])> accumulators = new GradientAccumulators<(float[], float[])>();

        /// <summary>
        /// Gets or sets the learning rate for the ADADELTA algorithm.
        /// </summary>
        /// <value>
        /// The learning rate for the ADADELTA algorithm. Default value is 1.0f.
        /// </value>
        public float LearningRate { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the learning rate decay for the ADADELTA algorithm.
        /// </summary>
        /// <value>
        /// The learning rate decay for the ADADELTA algorithm. Default value is 0.0f.
        /// After each epoch, the current learning rate is calculated as:
        /// rate := LearningRate / (1.0f + Decay * epoch).
        /// </value>
        public float Decay { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the rho parameter for the ADADELTA algorithm.
        /// </summary>
        /// <value>
        /// The rho parameter for the ADADELTA algorithm. Default value is 0.95f.
        /// </value>
        public float Rho { get; set; } = 0.95f;

        /// <summary>
        /// Gets or sets the epsilon parameter for the ADADELTA algorithm.
        /// </summary>
        /// <value>
        /// The epsilon parameter for the ADADELTA algorithm. Default value is 1e-8f.
        /// </value>
        public float Eps { get; set; } = 1e-8f;

        /// <inheritdoc />
        public void ComputeDeltas(int epoch, int length, float[] gradient, int totalSamples)
        {
            (float[] gsum, float[] xsum) = this.accumulators.GetAccumulator(
                gradient,
                () => (new float[length], new float[length]));

            /*float learningRate = -this.LearningRate;
            if (this.Decay != 0.0f && epoch > 0)
            {
                learningRate /= 1.0f + (this.Decay * epoch);
            }*/

            NativeMethods.adadelta(gradient.Length, gradient, gsum, xsum, this.Rho, this.Eps);

            /*float rho = this.Rho;
            float rhoinv = 1.0f - rho;
            float eps = this.Eps;

            for (int i = 0, ii = gradient.Length; i < ii; i++)
            {
                float g = gradient[i];

                gsum[i] = (rho * gsum[i]) + (rhoinv * g * g);
                g *= -(float)Math.Sqrt((xsum[i] + eps) / (gsum[i] + eps));
                xsum[i] = (rho * xsum[i]) + (rhoinv * g * g); // yes, xsum lags behind gsum by 1.

                Debug.Assert(!float.IsNaN(g), "Tensor contains invalid weight.");
                gradient[i] = g;
            }*/
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.MachineLearning.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void adadelta(
                int n,
                [In, Out] float[] gradient,
                [In, Out] float[] gsum,
                [In, Out] float[] xsum,
                float rho,
                float eps);
        }
    }
}
