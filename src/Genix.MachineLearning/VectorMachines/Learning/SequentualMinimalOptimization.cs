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
    using System.Threading;
    using Genix.Core;
    using Genix.MachineLearning.Classifcation;
    using Genix.MachineLearning.Kernels;

    /// <summary>
    /// The Sequential Minimal Optimization (SMO) Algorithm for Support Vector Machines (SVM) learning.
    /// </summary>
    public class SequentualMinimalOptimization : ISupportVectorMachineLearning
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
        ///  Gets or sets the complexity parameter C.
        /// </summary>
        /// <value>
        /// The complexity parameter C. The default value is 1.
        /// </value>
        /// <remarks>
        /// <para>
        /// A regularization parameter that trades off wide margin with a small number of margin failures.
        /// Increasing the value of C forces the creation of a more accurate model that may not generalize well.
        /// </para>
        /// </remarks>
        public float Complexity { get; set; } = 1;

        /// <summary>
        /// Gets or sets the positive class weight.
        /// </summary>
        /// <value>
        /// The positive class weight. The default value is 1.
        /// </value>
        /// <remarks>
        /// A value higher than zero indicating how much of the <see cref="Complexity"/>
        /// parameter should be applied to instances having positive label.
        /// </remarks>
        public float PositiveWeight { get; set; } = 1;

        /// <summary>
        /// Gets or sets the negative class weight.
        /// </summary>
        /// <value>
        /// The negative class weight. The default value is 1.
        /// </value>
        /// <remarks>
        /// A value higher than zero indicating how much of the <see cref="Complexity"/>
        /// parameter should be applied to instances having negative label.
        /// </remarks>
        public float NegativeWeight { get; set; } = 1;

        /// <summary>
        /// Gets or sets the convergence tolerance.
        /// </summary>
        /// <value>
        /// The criterion for completing the training process. The default is 0.01.
        /// </value>
        public float Tolerance { get; set; } = 0.01f;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="x"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of elements in <paramref name="y"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="weights"/> is not <b>null</b> and the number of elements in <paramref name="weights"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// </exception>
        public SupportVectorMachine Learn(
            IList<float[]> x,
            IList<bool> y,
            IList<float> weights,
            CancellationToken cancellationToken)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (y.Count != x.Count)
            {
                throw new ArgumentException("The number of output labels must match the number of input vectors.", nameof(y));
            }

            if (weights != null && weights.Count != x.Count)
            {
                throw new ArgumentException("The number of weights must match the number of input vectors.", nameof(weights));
            }

            int sampleCount = x.Count;

            // count positive and negative labels
            Labels.GetRatio(y, out int positives, out int negatives);

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

            // create expected values (+/-1)
            int[] expected = new int[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                expected[i] = y[i] ? 1 : -1;
            }

            // calculate complexity
            float positiveComplexity = this.Complexity * this.PositiveWeight;
            float negativeComplexity = this.Complexity * this.NegativeWeight;

            // calculate costs associated with each input
            float[] c = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                c[i] = y[i] ? positiveComplexity : negativeComplexity;
            }

            if (weights != null)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    c[i] *= weights[i];
                }
            }

            switch (this.Algorithm)
            {
                case SMOAlgorithm.LibSVM:
                    Func<int, int[], int, float[], float[]> q = (int i, int[] indices, int length, float[] result) =>
                    {
                        for (int j = 0; j < length; j++)
                        {
                            result[j] = expected[i] *
                                        expected[indices[j]] *
                                        this.kernel.Execute(x[i].Length, x[i], 0, x[indices[j]], 0);
                        }

                        return result;
                    };

                    LibSVMOptimization s = new LibSVMOptimization()
                    {
                        Tolerance = this.Tolerance,
                    };

                    s.Optimize(
                        sampleCount,
                        c,
                        Arrays.Create(sampleCount, -1.0f),
                        expected,
                        q,
                        out float[] solution,
                        out float rho);

                    /*HashSet<int> activeExamples = new HashSet<int>();
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
                        -(b_lower + b_upper) / 2);*/

                    return CreateMachine(solution, -rho);

                default:
                    throw new ArgumentException("The SMO optimization algorithm is invalid.");
            }

            SupportVectorMachine CreateMachine(float[] alphas, float bias)
            {
                // count number of non-bound support vectors
                int count = 0;
                for (int i = 0; i < alphas.Length; i++)
                {
                    if (alphas[i] > 0)
                    {
                        count++;
                    }
                }

                // create support vectors and weights
                float[][] v = new float[count][];
                float[] w = new float[count];

                for (int i = 0, j = 0; i < alphas.Length; i++)
                {
                    if (alphas[i] > 0)
                    {
                        v[j] = x[i].ToArray();
                        w[j] = alphas[i] * expected[i];
                        j++;
                    }
                }

                return new SupportVectorMachine(this.kernel, v, w, bias);
            }
        }
    }
}
