// -----------------------------------------------------------------------
// <copyright file="OneVsAllSupportVectorMachine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Genix.Core;
    using Genix.MachineLearning.VectorMachines.Learning;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the one-against-all Kernel Support Vector Machine (SVM).
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class OneVsAllSupportVectorMachine
    {
        /// <summary>
        /// The one-vs-one machines.
        /// </summary>
        [JsonProperty("machines")]
        private readonly SupportVectorMachine[] machines;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneVsAllSupportVectorMachine"/> class.
        /// </summary>
        /// <param name="machines">The one-vs-one machines.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="machines"/> is <b>null</b>.</para>
        /// </exception>
        private OneVsAllSupportVectorMachine(SupportVectorMachine[] machines)
        {
            this.machines = machines ?? throw new ArgumentNullException(nameof(machines));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneVsAllSupportVectorMachine"/> class.
        /// </summary>
        [JsonConstructor]
        private OneVsAllSupportVectorMachine()
        {
        }

        /// <summary>
        /// Gets the number of classes.
        /// </summary>
        /// <value>
        /// The number of classes.
        /// </value>
        public int NumberOfClasses => this.machines.Length;

        /// <summary>
        /// Creates a <see cref="OneVsAllSupportVectorMachine"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="OneVsAllSupportVectorMachine"/>.</param>
        /// <returns>The <see cref="OneVsAllSupportVectorMachine"/> this method creates.</returns>
        public static OneVsAllSupportVectorMachine FromFile(string fileName) => OneVsAllSupportVectorMachine.FromString(File.ReadAllText(fileName, Encoding.UTF8));

        /// <summary>
        /// Creates a <see cref="OneVsAllSupportVectorMachine"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="OneVsAllSupportVectorMachine"/> from.</param>
        /// <returns>The <see cref="OneVsAllSupportVectorMachine"/> this method creates.</returns>
        public static OneVsAllSupportVectorMachine FromMemory(byte[] buffer) => OneVsAllSupportVectorMachine.FromString(UTF8Encoding.UTF8.GetString(buffer));

        /// <summary>
        /// Creates a <see cref="OneVsAllSupportVectorMachine"/> from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="OneVsAllSupportVectorMachine"/> from.</param>
        /// <returns>The <see cref="OneVsAllSupportVectorMachine"/> this method creates.</returns>
        public static OneVsAllSupportVectorMachine FromString(string value) => JsonConvert.DeserializeObject<OneVsAllSupportVectorMachine>(value);

        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="trainer">The learning algorithm.</param>
        /// <param name="numberOfClasses">The number of classes.</param>
        /// <param name="x">The input vectors <paramref name="x"/>.</param>
        /// <param name="y">The expected binary output <paramref name="y"/>.</param>
        /// <param name="weights">The <c>weight</c> of importance for each input vector (if supported by the learning algorithm).</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the machine that the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="trainer"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="x"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="numberOfClasses"/> is less than 2.</para>
        /// <para>-or-</para>
        /// <para>The number of elements in <paramref name="y"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="weights"/> is not <b>null</b> and the number of elements in <paramref name="weights"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// </exception>
        /// <returns>
        /// The <see cref="OneVsAllSupportVectorMachine"/> learned by this method.
        /// A model that has learned how to produce <paramref name="y"/> given <paramref name="x"/>.
        /// </returns>
        public static OneVsAllSupportVectorMachine Learn(
            ISupportVectorMachineLearning trainer,
            int numberOfClasses,
            IList<float[]> x,
            IList<int> y,
            IList<float> weights,
            CancellationToken cancellationToken)
        {
            if (trainer == null)
            {
                throw new ArgumentNullException(nameof(trainer));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (numberOfClasses < 2)
            {
                throw new ArgumentException("The machine must have at least two classes.", nameof(numberOfClasses));
            }

            if (y.Count != x.Count)
            {
                throw new ArgumentException("The number of output labels must match the number of input vectors.", nameof(y));
            }

            // create the machines
            SupportVectorMachine[] machines = new SupportVectorMachine[numberOfClasses];

            // train each machine
            int sampleCount = x.Count;
            CommonParallel.For(
                0,
                machines.Length,
                (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        bool[] expected = new bool[sampleCount];
                        for (int j = 0; j < sampleCount; j++)
                        {
                            expected[j] = y[j] == i;
                        }

                        machines[i] = SupportVectorMachine.Learn(trainer, x, expected, weights, cancellationToken);
                    }
                },
                new ParallelOptions()
                {
                    CancellationToken = cancellationToken,
                });

            return new OneVsAllSupportVectorMachine(machines);
        }

        /// <summary>
        /// Saves the current <see cref="OneVsAllSupportVectorMachine"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="OneVsAllSupportVectorMachine"/>.</param>
        public void SaveToFile(string fileName) => File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);

        /// <summary>
        /// Saves the current <see cref="OneVsAllSupportVectorMachine"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="OneVsAllSupportVectorMachine"/>.</returns>
        public byte[] SaveToMemory() => UTF8Encoding.UTF8.GetBytes(this.SaveToString());

        /// <summary>
        /// Saves the current <see cref="OneVsAllSupportVectorMachine"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="OneVsAllSupportVectorMachine"/>.</returns>
        public string SaveToString() => JsonConvert.SerializeObject(this);

        /// <summary>
        /// Computes a score measuring association between the specified <paramref name="x" /> vector and each class.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <param name="result">The array that receives the result. Can be <b>null</b>.</param>
        /// <returns>
        /// The array of length <see cref="NumberOfClasses"/> that contains calculated association between <paramref name="x"/> and each class.
        /// </returns>
        public float[] Classify(float[] x, float[] result)
        {
            if (result == null)
            {
                result = new float[this.machines.Length];
            }

            // compute score for each machine
            CommonParallel.For(
                0,
                this.machines.Length,
                (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        result[i] = this.machines[i].Classify(x);
                    }
                },
                new ParallelOptions());

            return result;
        }
    }
}
