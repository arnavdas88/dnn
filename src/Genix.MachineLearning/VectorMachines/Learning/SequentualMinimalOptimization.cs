// -----------------------------------------------------------------------
// <copyright file="SequentualMinimalOptimization.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines.Learning
{
    using System;
    using Genix.Core;
    using Genix.MachineLearning.Classifcation;
    using Genix.MachineLearning.Kernels;

    /// <summary>
    /// The Sequential Minimal Optimization (SMO) Algorithm for Support Vector Machines (SVM) learning.
    /// </summary>
    public class SequentualMinimalOptimization
    {
        /// <summary>
        /// The kernel used by this machine.
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentualMinimalOptimization"/> class.
        /// </summary>
        /// <param name="kernel">The kernel function to use.</param>
        public SequentualMinimalOptimization(IKernel kernel)
        {
            this.kernel = kernel;
        }

        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="x">The model inputs.</param>
        /// <param name="labels">The expected outputs associated with each <paramref name="x"/>.</param>
        /// <param name="weights">The weight of importance for each input-output pair (if supported by the learning algorithm).</param>
        /// <returns>
        /// A model that has learned how to produce <paramref name="labels" /> given <paramref name="x" />.
        /// </returns>
        public SupportVectorMachine Learn(float[][] x, bool[] labels, float[] weights = null)
        {
            const float complexity = 1;
            const float positiveWeight = 1;
            const float negativeWeight = 1;

            // count positive and negative labels
            Labels.GetRatio(labels, out int positives, out int negatives);

            // if all labels are either positive or negative
            // create machine that will always produce one kind of output
            if (positives == 0 || negatives == 0)
            {
                /*Model.SupportVectors = new TInput[0];
                Model.Weights = new double[0];
                Model.Threshold = (positives == 0) ? -1 : +1;
                return Model;*/
            }

            // calculate complexity
            float positiveComplexity = complexity * positiveWeight;
            float negativeComplexity = complexity * negativeWeight;

            // calculate costs associated with each input
            float[] costs = new float[labels.Length];
            for (int i = 0, ii = labels.Length; i < ii; i++)
            {
                costs[i] = labels[i] ? positiveComplexity : negativeComplexity;
            }

            if (weights != null)
            {
                Mathematics.Mul(costs.Length, costs, 0, weights, 0);
            }

            // create expected values (+/-1)
            int[] expected = new int[labels.Length];
            for (int i = 0, ii = labels.Length; i < ii; i++)
            {
                expected[i] = labels[i] ? 1 : -1;
            }

            // lagrange multipliers
            ////this.alpha = new double[samples];

            Func<int, int[], int, float[], float[]> q = (int i, int[] indices, int length, float[] result) =>
            {
                for (int j = 0; j < length; j++)
                {
                    ////result[j] = y[i] * y[indices[j]] * this.kernel.Execute(x[i], x[indices[j]]);
                }

                return result;
            };

            ////var s = new FanChenLinQuadraticOptimization(alpha.Length, q, minusOnes, y)
            {
                /*Tolerance = tolerance,
                Shrinking = this.shrinking,
                Solution = alpha,
                Token = Token,
                UpperBounds = c*/
            };

            ////ISupportVectorMachine<double[]> svm = base.Learn(x, labels, weights);
            ////return (SupportVectorMachine)svm;

            return null;
        }
    }
}
