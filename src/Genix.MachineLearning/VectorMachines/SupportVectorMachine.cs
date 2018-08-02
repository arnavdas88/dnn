// -----------------------------------------------------------------------
// <copyright file="SupportVectorMachine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Genix.Core;
    using Genix.MachineLearning.Kernels;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the Support Vector Machine (SVM).
    /// </summary>
    public class SupportVectorMachine
    {
        /// <summary>
        /// The kernel used by this machine.
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVectorMachine"/> class.
        /// </summary>
        /// <param name="numberOfInputs">The length of the input vectors expected by the machine.</param>
        /// <param name="numberOfSupportVectors">The number of support vectors the machine would use.</param>
        /// <param name="kernel">The kernel function to use.</param>
        /// <param name="random">The random numbers generator.</param>
        public SupportVectorMachine(int numberOfInputs, int numberOfSupportVectors, IKernel kernel, RandomNumberGenerator random)
        {
            this.kernel = kernel;

            this.Weights = new Tensor("weights", new int[] { numberOfSupportVectors, 1 });
            this.Weights.Randomize(random ?? new GaussianGenerator(0.0, Math.Sqrt(1.0 / numberOfSupportVectors)));

            this.Bias = new Tensor("biases", new int[] { 1 });

            this.Vectors = new Tensor("support vectors", new int[] { numberOfInputs, numberOfSupportVectors });
            this.Vectors.Randomize(random ?? new GaussianGenerator(0.0, Math.Sqrt(1.0 / numberOfInputs)));
        }

        /// <summary>
        /// Gets the weights used by this machine.
        /// </summary>
        /// <value>
        /// The tensor that contains weights used by this machine.
        /// </value>
        /// <remarks>
        /// The <see cref="Weights"/> is a rank-1 tensor.
        /// </remarks>
        [JsonProperty("weights")]
        public Tensor Weights { get; private set; }

        /// <summary>
        /// Gets the bias used by this machine.
        /// </summary>
        /// <value>
        /// The <see cref="Tensor"/> object that contains bias used by this machine.
        /// </value>
        /// <remarks>
        /// The <see cref="Bias"/> is a rank-1 tensor with a single position occupied by bias value.
        /// </remarks>
        [JsonProperty("bias")]
        public Tensor Bias { get; private set; }

        /// <summary>
        /// Gets the support vectors used by this machine.
        /// </summary>
        /// <value>
        /// The <see cref="Tensor"/> object that contains support vectors used by this machine.
        /// </value>
        /// <remarks>
        /// The <see cref="Vectors"/> is a rank-1 tensor.
        /// </remarks>
        [JsonProperty("vectors")]
        public Tensor Vectors { get; private set; }

        /// <summary>
        /// Computes a score measuring association between the specified <paramref name="x" /> tensor and each class.
        /// </summary>
        /// <param name="session">The graph that stores all operations performed on the tensors.</param>
        /// <param name="x">The input tensor.</param>
        /// <returns>
        /// The output tensor that contains the calculated scores.
        /// </returns>
        public Tensor Execute(Session session, Tensor x)
        {
            // run linear classifier
            return session.MxM(
                MatrixLayout.ColumnMajor,
                this.Weights,
                false,
                this.ExecuteKernel(session, x, false),
                false,
                this.Bias);

            /*for (int i = 0, ii = input.Length; i < ii; i++)
            {
                float sum = this.threshold;
                for (int j = 0; j < this.supportVectors.Length; j++)
                {
                    sum += this.weights[j] * this.kernel.Execute(this.supportVectors[j], input[i]);
                }

                result[i] = sum;
            }

            return result;*/
        }

        private Tensor ExecuteKernel(Session session, Tensor x, bool calculateGradient)
        {
            const string ActionName = "SVM kernel";

            int numberOfInputs = this.Vectors.Axes[0];
            int numberOfSupportVectors = this.Vectors.Axes[1];

            int mb = x.Axes[0];
            Tensor y = session.AllocateTensor(ActionName, new[] { mb, numberOfSupportVectors }, calculateGradient);

            int y0 = y.Axes[0];
            int y1 = y.Axes[1];

            float[] xw = x.Weights;
            float[] yw = y.Weights;
            float[] vw = this.Vectors.Weights;

            // here we are basically doing matrix to matrix multiplication
            // with a custom dot product
            for (int ix = 0, xpos = 0, ypos = 0; ix < y0; ix++, xpos += numberOfInputs)
            {
                for (int iy = 0, vpos = 0; iy < y1; iy++, vpos += numberOfInputs)
                {
                    yw[ypos++] = this.kernel.Execute(numberOfInputs, xw, xpos, vw, vpos);
                }
            }

            return y;
        }
    }
}
