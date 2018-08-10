// -----------------------------------------------------------------------
// <copyright file="Answer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a classification answer.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Answer : DataSource
    {
        /// <summary>
        /// The list of answer candidates.
        /// </summary>
        private readonly List<(string className, double score)> candidates = new List<(string className, double score)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data used to create this <see cref="Answer"/>.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this page if it belongs to a multi-page file.
        /// <b>null</b> if this page belongs to a single-page file.
        /// </param>
        /// <param name="className">The name of the class.</param>
        /// <param name="confidence">A value from 0 to 100 that represents the confidence that the answer is valid.</param>
        /// <param name="candidates">The collection of candidate answers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="candidates"/> is <b>null</b>.
        /// </exception>
        public Answer(
            string id,
            string name,
            int? frameIndex,
            string className,
            double confidence,
            IEnumerable<(string, double)> candidates)
            : base(id, name, frameIndex)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            this.ClassName = className;
            this.Confidence = confidence;
            this.candidates.AddRange(candidates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class,
        /// using the features used to create this <see cref="Answer"/>.
        /// </summary>
        /// <param name="features">The <see cref="Features"/> used to create this <see cref="Answer"/>.</param>
        /// <param name="className">The name of the class.</param>
        /// <param name="confidence">A value from 0 to 100 that represents the confidence that the answer is valid.</param>
        /// <param name="candidates">The collection of candidate answers.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="features"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="candidates"/> is <b>null</b>.</para>
        /// </exception>
        public Answer(
            Features features,
            string className,
            double confidence,
            IEnumerable<(string, double)> candidates)
            : base(features)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            this.ClassName = className;
            this.Confidence = confidence;
            this.candidates.AddRange(candidates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class,
        /// using the features used to create this <see cref="Answer"/>.
        /// </summary>
        /// <param name="features">The <see cref="Features"/> used to create this <see cref="Answer"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="features"/> is <b>null</b>.</para>
        /// </exception>
        /// <remarks>
        /// This constructor create a blank <see cref="Answer"/> that does not contain any result.
        /// </remarks>
        public Answer(Features features)
            : base(features)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class,
        /// using the existing <see cref="Answer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Answer"/> to copy the data from.</param>
        public Answer(Answer other)
            : base(other)
        {
            this.ClassName = other.ClassName;
            this.Confidence = other.Confidence;
            this.candidates.AddRange(other.Candidates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        private Answer()
        {
        }

        /// <summary>
        /// Gets a name of the class.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the name class.
        /// </value>
        [JsonProperty("className")]
        public string ClassName { get; private set; }

        /// <summary>
        /// Gets a confidence for the answer.
        /// </summary>
        /// <value>
        /// A <see cref="double"/> value from 0 to 100 that represents the confidence that the answer is valid.
        /// </value>
        [JsonProperty("confidence")]
        public double Confidence { get; private set; }

        /// <summary>
        /// Gets a collection of candidate answers.
        /// </summary>
        /// <value>
        /// A <see cref="IReadOnlyCollection{T}"/> object that contains the list of candidate answers.
        /// </value>
        public IReadOnlyCollection<(string className, double score)> Candidates =>
            new ReadOnlyCollection<(string, double)>(this.candidates);
    }
}
