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

    /// <summary>
    /// Represents the result of classification.
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult" /> class.
        /// </summary>
        public ClassificationResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult" /> class, using property values.
        /// </summary>
        /// <param name="fileName">The image file name.</param>
        /// <param name="frameIndex">The zero-based frame index of the classified image in the multi-page image file.</param>
        /// <param name="className">The classification result.</param>
        /// <param name="truth">The ground truth data for the class.</param>
        /// <param name="confidence">The classification confidence level.</param>
        /// <param name="isAccepted">The value indicating whether the classification result was accepted.</param>
        public ClassificationResult(string fileName, int? frameIndex, string className, string truth, int confidence, bool isAccepted)
        {
            this.FileName = fileName;
            this.FrameIndex = frameIndex;
            this.Predicted = className;
            this.Expected = truth;
            this.Confidence = confidence;
            this.IsAccepted = isAccepted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationResult" /> class that has the same property values as the specified <see cref="ClassificationResult" />.
        /// </summary>
        /// <param name="other">The <see cref="ClassificationResult" /> to copy property values from.</param>
        public ClassificationResult(ClassificationResult other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.FileName = other.FileName;
            this.FrameIndex = other.FrameIndex;
            this.Predicted = other.Predicted;
            this.Expected = other.Expected;
            this.Confidence = other.Confidence;
            this.IsAccepted = other.IsAccepted;
        }

        /// <summary>
        /// Gets or sets the image file name.
        /// </summary>
        /// <value>
        /// A <see cref="String"/> that contains the image file name.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets zero-based frame index of the classified image in the multi-page image file. <b>null</b> if the classified image belongs to a single-page image file.
        /// </summary>
        /// <value>
        /// The zero-based index for the classified image in a multi-page image file. <b>null</b> if the classified image belongs to a single-page image file.
        /// </value>
        public int? FrameIndex { get; set; }

        /// <summary>
        /// Gets or sets the classification result.
        /// </summary>
        /// <value>
        /// A <see cref="String"/> that contains the classification result.
        /// </value>
        public string Predicted { get; set; }

        /// <summary>
        /// Gets or sets the ground truth data.
        /// </summary>
        /// <value>
        /// The <see cref="String"/> that contains the ground truth.
        /// </value>
        public string Expected { get; set; }

        /// <summary>
        /// Gets or sets the classification confidence level.
        /// </summary>
        /// <value>
        /// A <see cref="Single"/> that contains the classification confidence level.
        /// </value>
        public int Confidence { get; set; }

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
        /// <param name="answer">The <see cref="ClassificationResult"/> structure to write.</param>
        public static void Write(TextWriter writer, ClassificationResult answer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (answer == null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            writer.WriteLine(answer.ToString());
        }

        /// <summary>
        /// Writes a sequence of classification results into the stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="answers">The sequence of <see cref="ClassificationResult"/> structures to write.</param>
        public static void Write(TextWriter writer, IEnumerable<ClassificationResult> answers)
        {
            if (answers == null)
            {
                throw new ArgumentNullException(nameof(answers));
            }

            foreach (ClassificationResult answer in answers)
            {
                ClassificationResult.Write(writer, answer);
            }
        }

        /// <summary>
        /// Reads a sequence of classification results from the file.
        /// </summary>
        /// <param name="fileName">A path to the file to read from.</param>
        /// <returns>A collection of <see cref="ClassificationResult"/> structures created from the stream.</returns>
        public static IList<ClassificationResult> Read(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName, System.Text.Encoding.UTF8))
            {
                return ClassificationResult.Read(reader).ToList();
            }
        }

        /// <summary>
        /// Reads a sequence of classification results from the stream.
        /// </summary>
        /// <param name="reader">A stream to read from.</param>
        /// <returns>A sequence of <see cref="ClassificationResult"/> structures created from the stream.</returns>
        public static IEnumerable<ClassificationResult> Read(TextReader reader)
        {
            while (true)
            {
                string s = reader.ReadLine();
                if (s == null)
                {
                    break;
                }

                if (s == string.Empty)
                {
                    continue;
                }

                string[] split = s.SplitQualified(',');
                if (split.Length >= 5)
                {
                    ClassificationResult answer = new ClassificationResult()
                    {
                        FileName = split[0]
                    };

                    if (!string.IsNullOrEmpty(split[1]))
                    {
                        answer.FrameIndex = int.Parse(split[1]);
                    }

                    string className = split[2].Unqualify('\"');
                    if (!string.IsNullOrEmpty(className))
                    {
                        answer.Predicted = className;
                    }

                    answer.Confidence = int.Parse(split[3]);
                    answer.IsAccepted = split[4] == "1";

                    yield return answer;
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

            ClassificationResult other = obj as ClassificationResult;
            if (other == null)
            {
                return false;
            }

            return string.Compare(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase) == 0 &&
                this.FrameIndex == other.FrameIndex &&
                this.Predicted == other.Predicted &&
                this.Expected == other.Expected &&
                this.Confidence == other.Confidence &&
                this.IsAccepted == other.IsAccepted;
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.ToString().GetHashCode();

        /// <inheritdoc />
        public override string ToString() =>
            string.Join(
                ",",
                this.FileName,
                this.FrameIndex.HasValue ? this.FrameIndex.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                this.Predicted?.Qualify() ?? string.Empty,
                this.Expected?.Qualify() ?? string.Empty,
                this.Confidence,
                this.IsAccepted ? 1 : 0);
    }
}
