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
        private readonly List<(string className, float score)> candidates = new List<(string className, float score)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class,
        /// using the features used to create this <see cref="Answer"/>.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <param name="className">The name of the class.</param>
        /// <param name="confidence">A value from 0 to 100 that represents the confidence that the answer is valid.</param>
        /// <param name="candidates">The collection of candidate answers.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="id"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="candidates"/> is <b>null</b>.</para>
        /// </exception>
        public Answer(
            DataSourceId id,
            string className,
            float confidence,
            IEnumerable<(string, float)> candidates)
            : base(id)
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
        /// <param name="id">The source of data.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="id"/> is <b>null</b>.</para>
        /// </exception>
        /// <remarks>
        /// This constructor create a blank <see cref="Answer"/> that does not contain any result.
        /// </remarks>
        public Answer(DataSourceId id)
            : base(id)
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
        /// A <see cref="float"/> value from 0 to 100 that represents the confidence that the answer is valid.
        /// </value>
        [JsonProperty("confidence")]
        public float Confidence { get; private set; }

        /// <summary>
        /// Gets a collection of candidate answers.
        /// </summary>
        /// <value>
        /// A <see cref="IReadOnlyCollection{T}"/> object that contains the list of candidate answers.
        /// </value>
        public IReadOnlyCollection<(string className, float score)> Candidates =>
            new ReadOnlyCollection<(string, float)>(this.candidates);
    }
}
