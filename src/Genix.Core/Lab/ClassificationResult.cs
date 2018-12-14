// -----------------------------------------------------------------------
// <copyright file="ClassificationResult.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Genix.Core;

    /// <summary>
    /// Represents the result of classification.
    /// </summary>
    /// <typeparam name="T">The type of the classification answer.</typeparam>
    public class ClassificationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult{T}" /> class.
        /// </summary>
        public ClassificationResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult{T}" /> class,
        /// using property values.
        /// </summary>
        /// <param name="sourceId">The information about result source.</param>
        /// <param name="predicted">The classification result.</param>
        /// <param name="expected">The ground truth data for the class.</param>
        /// <param name="confidence">The classification confidence level.</param>
        /// <param name="isAccepted">The value indicating whether the classification result was accepted.</param>
        public ClassificationResult(DataSourceId sourceId, T predicted, T expected, float confidence, bool isAccepted)
        {
            this.SourceId = sourceId;
            this.Predicted = predicted;
            this.Expected = expected;
            this.Confidence = confidence;
            this.IsAccepted = isAccepted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult{T}" /> class that has the same property values as the specified <see cref="ClassificationResult{T}" />.
        /// </summary>
        /// <param name="other">The <see cref="ClassificationResult{T}" /> to copy property values from.</param>
        public ClassificationResult(ClassificationResult<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.SourceId = other.SourceId;
            this.Predicted = other.Predicted;
            this.Expected = other.Expected;
            this.Confidence = other.Confidence;
            this.IsAccepted = other.IsAccepted;
        }

        /// <summary>
        /// Gets or sets the data source id.
        /// </summary>
        /// <value>
        /// A <see cref="DataSourceId"/> that contains the information about result source.
        /// </value>
        public DataSourceId SourceId { get; set; }

        /// <summary>
        /// Gets or sets the classification result.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the classification result.
        /// </value>
        public T Predicted { get; set; }

        /// <summary>
        /// Gets or sets the ground truth data.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that contains the ground truth.
        /// </value>
        public T Expected { get; set; }

        /// <summary>
        /// Gets or sets the classification confidence level.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> that contains the classification confidence level.
        /// </value>
        public float Confidence { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the classification result was accepted.
        /// </summary>
        /// <value>
        /// <b>true</b> when classification result is accepted; otherwise, <b>false</b>.
        /// </value>
        public bool IsAccepted { get; set; }

        /// <summary>
        /// Writes a classification result into the stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="result">The <see cref="ClassificationResult{T}"/> structure to write.</param>
        public static void Write(TextWriter writer, ClassificationResult<T> result)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            writer.WriteLine(result.ToString());
        }

        /// <summary>
        /// Writes a sequence of classification results into the stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="results">The sequence of <see cref="ClassificationResult{T}"/> structures to write.</param>
        public static void Write(TextWriter writer, IEnumerable<ClassificationResult<T>> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (ClassificationResult<T> result in results)
            {
                ClassificationResult<T>.Write(writer, result);
            }
        }

        /// <summary>
        /// Reads a sequence of classification results from the file.
        /// </summary>
        /// <param name="fileName">A path to the file to read from.</param>
        /// <returns>A collection of <see cref="ClassificationResult{T}"/> structures created from the stream.</returns>
        public static IList<ClassificationResult<T>> Read(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName, System.Text.Encoding.UTF8))
            {
                return ClassificationResult<T>.Read(reader).ToList();
            }
        }

        /// <summary>
        /// Reads a sequence of classification results from the stream.
        /// </summary>
        /// <param name="reader">A stream to read from.</param>
        /// <returns>A sequence of <see cref="ClassificationResult{T}"/> structures created from the stream.</returns>
        public static IEnumerable<ClassificationResult<T>> Read(TextReader reader)
        {
            while (true)
            {
                string s = reader.ReadLine();
                if (s == null)
                {
                    break;
                }

                if (s != string.Empty)
                {
                    string[] split = s.SplitQualified(',');
                    if (split.Length >= 5)
                    {
                        string fileName = split[0];
                        int? frameIndex = !string.IsNullOrEmpty(split[1]) ? (int?)int.Parse(split[1]) : null;
                        string predicted = split[2].Unqualify('\"');
                        float confidence = float.Parse(split[3]);
                        bool isAccepted = split[4] == "1";

                        yield return new ClassificationResult<T>(
                            new DataSourceId(fileName, fileName, frameIndex),
                            !string.IsNullOrEmpty(predicted) ? (T)Convert.ChangeType(predicted, typeof(T)) : default,
                            default,
                            confidence,
                            isAccepted);
                    }
                }
            }

            yield break;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ClassificationResult<T> other = obj as ClassificationResult<T>;
            if (other == null)
            {
                return false;
            }

            return object.Equals(this.SourceId, other.SourceId) &&
                object.Equals(this.Predicted, other.Predicted) &&
                object.Equals(this.Expected, other.Expected) &&
                this.Confidence == other.Confidence &&
                this.IsAccepted == other.IsAccepted;
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.ToString().GetHashCode();

        /// <inheritdoc />
        public override string ToString() =>
            string.Join(
                ",",
                this.SourceId.ToFileName(true),
                this.Predicted?.ToString()?.Qualify() ?? string.Empty,
                this.Expected?.ToString()?.Qualify() ?? string.Empty,
                this.Confidence.ToString("F4", CultureInfo.InvariantCulture),
                this.IsAccepted ? 1 : 0);
    }
}
