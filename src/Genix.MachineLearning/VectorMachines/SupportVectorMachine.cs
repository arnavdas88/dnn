// -----------------------------------------------------------------------
// <copyright file="SupportVectorMachine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines
{
    using Genix.MachineLearning.Kernels;

    public class SupportVectorMachine<TInput, TKernel>
        where TKernel : IKernel<TInput>
    {
        /// <summary>
        /// The kernel used by this machine.
        /// </summary>
        private readonly TKernel kernel;

        /// <summary>
        /// The collection of support vectors used by this machine.
        /// </summary>
        private TInput[] supportVectors;

        /// <summary>
        /// The collection of weights used by this machine.
        /// </summary>
        private float[] weights;

        /// <summary>
        /// The threshold(bias) term for this machine.
        /// </summary>
        private float threshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVectorMachine{TInput, TKernel}"/> class.
        /// </summary>
        /// <param name="inputs">The length of the input vectors expected by the machine.</param>
        /// <param name="kernel">The kernel function to use.</param>
        public SupportVectorMachine(int inputs, TKernel kernel)
        {
            /*this.NumberOfInputs = inputs;
            this.NumberOfOutputs = 1;
            this.NumberOfClasses = 2;*/
            this.kernel = kernel;
        }

        /// <summary>
        /// Computes a score measuring association between the specified <paramref name="input" /> vector and each class.
        /// </summary>
        /// <param name="input">The input vector.</param>
        /// <param name="result">An array where the result will be stored, avoiding unnecessary memory allocations.</param>
        /// <returns>An array of <see cref="float"/> that contains the calculated scores.</returns>
        public float[] Score(TInput[] input, float[] result)
        {
            for (int i = 0, ii = input.Length; i < ii; i++)
            {
                float sum = this.threshold;
                for (int j = 0; j < this.supportVectors.Length; j++)
                {
                    sum += this.weights[j] * this.kernel.Execute(this.supportVectors[j], input[i]);
                }

                result[i] = sum;
            }

            return result;
        }

        /// <summary>
        /// Predicts a class label vector for the given input vectors, returning the
        /// log-likelihood that the input vector belongs to its predicted class.
        /// </summary>
        /// <param name="input">The input vector.</param>
        /// <param name="result">An array where the log-likelihoods will be stored, avoiding unnecessary memory allocations.</param>
        /// <returns>An array of <see cref="float"/> that contains the log-likelihoods scores.</returns>
        public float[] LogLikelihood(TInput[] input, float[] result)
        {
            result = this.Score(input, result);
            for (int i = 0, ii = input.Length; i < ii; i++)
            {
                ////result[i] = -Special.Log1pexp(-result[i]);
            }

            return result;
        }
    }
}
