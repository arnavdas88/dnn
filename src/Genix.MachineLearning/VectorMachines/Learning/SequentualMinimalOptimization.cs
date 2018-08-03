// -----------------------------------------------------------------------
// <copyright file="SequentualMinimalOptimization.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines.Learning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="kernel"/> is <b>null</b>.
        /// </exception>
        public SequentualMinimalOptimization(IKernel kernel)
        {
            this.kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        /// <summary>
        /// Gets or sets the pair selection optimization algorithm.
        /// </summary>
        /// <value>
        /// The <see cref="SMOAlgorithm"/> enumeration. The default is <see cref="SMOAlgorithm.LibSVM"/>.
        /// </value>
        public SMOAlgorithm Algorithm { get; set; } = SMOAlgorithm.LibSVM;

        /// <summary>
        /// Gets or sets the convergence tolerance.
        /// </summary>
        /// <value>
        /// The criterion for completing the training process. The default is 0.01.
        /// </value>
        public float Tolerance { get; set; } = 0.01f;

        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="samples">
        /// The samples used for learning.
        /// Each sample consists of input vector <c>x</c>,
        /// expected output <c>y</c>,
        /// and the <c>weight</c> of importance (if supported by the learning algorithm).
        /// A model that has learned how to produce <paramref name="samples" />.y given <paramref name="samples" />.x.
        /// </param>
        /// <returns>
        /// The <see cref="SupportVectorMachine"/> learned by this method.
        /// </returns>
        public SupportVectorMachine Learn(IList<(float[] x, bool y, float weight)> samples)
        {
            const float complexity = 1;
            const float positiveWeight = 1;
            const float negativeWeight = 1;

            // count positive and negative labels
            Labels.GetRatio(samples.Select(x => x.y), out int positives, out int negatives);

            // if all labels are either positive or negative
            // create machine that will always produce one kind of output
            if (positives == 0 || negatives == 0)
            {
                return new SupportVectorMachine(
                    this.kernel,
                    new float[0][],
                    new float[0],
                    positives == 0 ? -1 : 1);
            }

            // calculate complexity
            float positiveComplexity = complexity * positiveWeight;
            float negativeComplexity = complexity * negativeWeight;

            // calculate costs associated with each input
            float[] c = new float[samples.Count];
            for (int i = 0, ii = samples.Count; i < ii; i++)
            {
                c[i] = (samples[i].y ? positiveComplexity : negativeComplexity) * samples[i].weight;
            }

            // create expected values (+/-1)
            int[] y = new int[samples.Count];
            for (int i = 0, ii = samples.Count; i < ii; i++)
            {
                y[i] = samples[i].y ? 1 : -1;
            }

            switch (this.Algorithm)
            {
                case SMOAlgorithm.LibSVM:
                    Func<int, int[], int, float[], float[]> q = (int i, int[] indices, int length, float[] result) =>
                    {
                        for (int j = 0; j < length; j++)
                        {
                            result[j] = y[i] *
                                        y[indices[j]] *
                                        this.kernel.Execute(samples[i].x.Length, samples[i].x, 0, samples[indices[j]].x, 0);
                        }

                        return result;
                    };

                    LibSVMOptimization s = new LibSVMOptimization()
                    {
                        Tolerance = this.Tolerance,
                    };

                    s.Optimize(
                        samples.Count,
                        c,
                        Arrays.Create(samples.Count, -1.0f),
                        y,
                        q,
                        out float[] solution,
                        out float rho);

                    HashSet<int> activeExamples = new HashSet<int>();
                    for (int i = 0; i < solution.Length; i++)
                    {
                        if (solution[i] > 0)
                        {
                            activeExamples.Add(i);
                        }
                    }

                    float b_lower = rho;
                    float b_upper = rho;

                    float[][] vectors = new float[activeExamples.Count][];
                    float[] weights = new float[activeExamples.Count];

                    int index = 0;
                    foreach (var j in activeExamples)
                    {
                        vectors[index] = samples[j].x.ToArray();
                        weights[index] = solution[j] * y[j];
                        index++;
                    }

                    return new SupportVectorMachine(
                        this.kernel,
                        vectors,
                        weights,
                        -(b_lower + b_upper) / 2);

                default:
                    throw new ArgumentException("The SMO optimization algorithm is invalid.");
            }
        }
    }
}
