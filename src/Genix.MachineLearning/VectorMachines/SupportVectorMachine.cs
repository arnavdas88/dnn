// -----------------------------------------------------------------------
// <copyright file="SupportVectorMachine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines
{
    using System;
    using System.IO;
    using System.Text;
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
        [JsonProperty("kernel", TypeNameHandling = TypeNameHandling.Objects)]
        private readonly IKernel kernel;

        /// <summary>
        /// The support vectors used by this machine.
        /// </summary>
        [JsonProperty("vectors")]
        private readonly float[][] vectors;

        /// <summary>
        /// The weights used by this machine.
        /// </summary>
        [JsonProperty("weights")]
        private readonly float[] weights;

        /// <summary>
        /// The bias used by this machine.
        /// </summary>
        [JsonProperty("bias")]
        private readonly float bias;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVectorMachine"/> class.
        /// </summary>
        /// <param name="kernel">The kernel function to use.</param>
        /// <param name="vectors">The support vectors to use.</param>
        /// <param name="weights">The weights to use.</param>
        /// <param name="bias">The bias to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="kernel"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="vectors"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="weights"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The length of weights and support vectors are not the same.</para>
        /// </exception>
        public SupportVectorMachine(IKernel kernel, float[][] vectors, float[] weights, float bias)
        {
            this.kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            this.vectors = vectors ?? throw new ArgumentNullException(nameof(vectors));
            this.weights = weights ?? throw new ArgumentNullException(nameof(weights));
            this.bias = bias;

            if (vectors.Length != weights.Length)
            {
                throw new ArgumentException("The length of weights and support vectors are not the same.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVectorMachine"/> class.
        /// </summary>
        [JsonConstructor]
        private SupportVectorMachine()
        {
        }

        /// <summary>
        /// Creates a <see cref="SupportVectorMachine"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="SupportVectorMachine"/>.</param>
        /// <returns>The <see cref="SupportVectorMachine"/> this method creates.</returns>
        public static SupportVectorMachine FromFile(string fileName) => SupportVectorMachine.FromString(File.ReadAllText(fileName, Encoding.UTF8));

        /// <summary>
        /// Creates a <see cref="SupportVectorMachine"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="SupportVectorMachine"/> from.</param>
        /// <returns>The <see cref="SupportVectorMachine"/> this method creates.</returns>
        public static SupportVectorMachine FromMemory(byte[] buffer) => SupportVectorMachine.FromString(UTF8Encoding.UTF8.GetString(buffer));

        /// <summary>
        /// Creates a <see cref="SupportVectorMachine"/> from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="SupportVectorMachine"/> from.</param>
        /// <returns>The <see cref="SupportVectorMachine"/> this method creates.</returns>
        public static SupportVectorMachine FromString(string value) => JsonConvert.DeserializeObject<SupportVectorMachine>(value);

        /// <summary>
        /// Computes a score measuring association between the specified <paramref name="x" /> vector and each class.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>
        /// The calculated score.
        /// </returns>
        public float Execute(float[] x)
        {
            float result = this.bias;
            for (int i = 0, ii = this.weights.Length; i < ii; i++)
            {
                result += this.weights[i] * this.kernel.Execute(x.Length, this.vectors[i], 0, x, 0);
            }

            return result;
        }

        /// <summary>
        /// Computes a score measuring association between the specified <paramref name="x" /> vectors and each class.
        /// </summary>
        /// <param name="x">The input vectors.</param>
        /// <returns>
        /// The calculated scores.
        /// </returns>
        [CLSCompliant(false)]
        public float[] Execute(float[][] x)
        {
            float[] result = new float[x.Length];
            for (int i = 0, ii = x.Length; i < ii; i++)
            {
                result[i] = this.Execute(x[i]);
            }

            return result;
        }

        /// <summary>
        /// Saves the current <see cref="SupportVectorMachine"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="SupportVectorMachine"/>.</param>
        public void SaveToFile(string fileName) => File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);

        /// <summary>
        /// Saves the current <see cref="SupportVectorMachine"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="SupportVectorMachine"/>.</returns>
        public byte[] SaveToMemory() => UTF8Encoding.UTF8.GetBytes(this.SaveToString());

        /// <summary>
        /// Saves the current <see cref="SupportVectorMachine"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="SupportVectorMachine"/>.</returns>
        public string SaveToString() => JsonConvert.SerializeObject(this);
    }
}
