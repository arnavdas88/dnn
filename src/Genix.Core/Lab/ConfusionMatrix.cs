// -----------------------------------------------------------------------
// <copyright file="ConfusionMatrix.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Visualizes the performance of a prediction algorithm.
    /// Each row of the matrix represents the instances in a predicted class while each column represents the instances in an expected class.
    /// </summary>
    public class ConfusionMatrix : ICloneable
    {
        /// <summary>
        /// The labels supported by the matrix.
        /// </summary>
        private readonly Dictionary<string, int> labels = new Dictionary<string, int>();

        /// <summary>
        /// The confusion matrix.
        /// </summary>
        private int[][] matrix = new int[0][];

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfusionMatrix"/> class.
        /// </summary>
        public ConfusionMatrix()
        {
        }

        /// <summary>
        /// Gets the labels supported by the matrix.
        /// </summary>
        /// <value>
        /// The <see cref="IReadOnlyCollection{T}"/> that contains a list of labels.
        /// </value>
        public IReadOnlyCollection<string> Labels
        {
            get { return this.labels.Keys; }
        }

        /// <summary>
        /// Gets a number of confusions between <c>predicted</c> and <c>expected</c> labels.
        /// </summary>
        /// <param name="predicted">The detected label.</param>
        /// <param name="expected">The correct label.</param>
        /// <returns>
        /// The <see cref="int"/> that represents a number of confusions between <c>predicted</c> and <c>expected</c> labels.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional", Justification = "We need to use label names and access two-dimensional matrix.")]
        public int this[string predicted, string expected]
        {
            get
            {
                if (!this.labels.TryGetValue(predicted, out int predictedIndex))
                {
                    throw new ArgumentException("The predicted value is not one of the labels.", nameof(predicted));
                }

                if (!this.labels.TryGetValue(expected, out int expectedIndex))
                {
                    throw new ArgumentException("The expected value is not one of the labels.", nameof(expected));
                }

                return this.matrix[predictedIndex][expectedIndex];
            }
        }

        /// <summary>
        /// Adds a sample to the matrix.
        /// </summary>
        /// <param name="predicted">The detected label.</param>
        /// <param name="expected">The correct label.</param>
        public void Add(string predicted, string expected)
        {
            this.InsertLabel(predicted, out int predictedIndex);
            this.InsertLabel(expected, out int expectedIndex);

            this.matrix[predictedIndex][expectedIndex]++;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            ConfusionMatrix other = obj as ConfusionMatrix;
            if (other == null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(this.labels, other.labels);
        }

        /// <inheritdoc />
        public override int GetHashCode() => 0;

        /// <inheritdoc />
        public override string ToString()
        {
            int count = this.labels.Count;

            // calculate maximum label length
            string[] names = this.labels.Keys.OrderBy(x => x).ToArray();
            int[] lengths = names.Select(x => Math.Max((x?.Length ?? 0) + 1, 8)).ToArray();
            int maxLength = lengths.Max();
            string maxFormat = string.Format(CultureInfo.InvariantCulture, "{{0,-{0}}}", maxLength);
            string[] formats = lengths.Select(x => string.Format(CultureInfo.InvariantCulture, "{{0,-{0}}}", x)).ToArray();

            // allocate buffer
            int totalLength = lengths.Sum();
            StringBuilder sb = new StringBuilder((totalLength + maxLength + 2) * (count + 2));

            // write header - expected labels
            sb.AppendFormat(CultureInfo.InvariantCulture, maxFormat, "p\\e");
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, maxFormat, names[i]);
            }

            sb.Append(Environment.NewLine);
            sb.Append('-', totalLength + maxLength);
            sb.Append(Environment.NewLine);

            // write rows - detected labels
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, formats[i], names[i]);
                int ii = this.labels[names[i]];

                for (int j = 0; j < count; j++)
                {
                    int jj = this.labels[names[j]];
                    sb.AppendFormat(CultureInfo.InvariantCulture, formats[j], this.matrix[ii][jj]);
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            ConfusionMatrix other = new ConfusionMatrix();
            foreach (KeyValuePair<string, int> kvp in other.labels)
            {
                other.labels.Add(kvp.Key, kvp.Value);
            }

            other.matrix = new int[this.matrix.Length][];
            for (int i = 0, ii = this.matrix.Length; i < ii; i++)
            {
                other.matrix[i] = this.matrix[i].ToArray();
            }

            return other;
        }

        /// <summary>
        /// Finds a position of existing label, inserts a label if not found.
        /// </summary>
        /// <param name="label">The label to find.</param>
        /// <param name="index">The zero-based index of inserted label.</param>
        private void InsertLabel(string label, out int index)
        {
            if (!this.labels.TryGetValue(label, out index))
            {
                index = this.labels.Count;
                this.labels.Add(label, index);

                int count = this.labels.Count;
                Array.Resize(ref this.matrix, count);

                for (int i = 0; i < count; i++)
                {
                    Array.Resize(ref this.matrix[i], count);
                }
            }
        }
    }
}
