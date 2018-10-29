// -----------------------------------------------------------------------
// <copyright file="Answer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Recognition
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a field recognition answer.
    /// </summary>
    public class Answer : DataSource
    {
        /// <summary>
        /// The collection of answer candidates.
        /// </summary>
        [JsonProperty("candidates")]
        private readonly List<(string text, float score)> candidates = new List<(string text, float score)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <param name="text">The recognized text.</param>
        /// <param name="confidence">The confidence for the answer. The value from 0 to 1.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Answer(DataSourceId id, string text, float confidence)
            : base(id)
        {
            this.Text = text;
            this.Confidence = confidence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <param name="text">The recognized text.</param>
        /// <param name="confidence">The confidence for the answer. The value from 0 to 1.</param>
        /// <param name="position">The position of the recognized text.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Answer(DataSourceId id, string text, float confidence, Rectangle position)
            : this(id, text, confidence)
        {
            this.Position = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <param name="text">The recognized text.</param>
        /// <param name="confidence">The confidence for the answer. The value from 0 to 1.</param>
        /// <param name="position">The position of the recognized text.</param>
        /// <param name="candidates">The collection of answer candidates.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Answer(DataSourceId id, string text, float confidence, Rectangle position, IEnumerable<(string text, float score)> candidates)
            : this(id, text, confidence, position)
        {
            if (candidates != null)
            {
                this.candidates.AddRange(candidates);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Answer(DataSourceId id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Answer()
        {
        }

        /// <summary>
        /// Gets or sets a recognized text.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the recognized text.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a confidence for the answer.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> value from 0 to 1 that represents the confidence that the answer is valid.
        /// </value>
        [JsonProperty("conf")]
        public float Confidence { get; set; }

        /// <summary>
        /// Gets or sets the position of the recognized text regarding to <see cref="DataSource"/> it was extracted from.
        /// </summary>
        /// <value>
        /// A <see cref="Rectangle"/> object that contains the text position.
        /// </value>
        [JsonProperty("pos")]
        public Rectangle Position { get; set; }

        /// <summary>
        /// Gets a collection of candidate answers.
        /// </summary>
        /// <value>
        /// A <see cref="IReadOnlyCollection{T}"/> object that contains the list of candidate answers.
        /// </value>
        public IReadOnlyCollection<(string text, float score)> Candidates => this.candidates;

        /// <inheritdoc />
        public override string ToString() =>
            string.Join(
                " : ",
                this.Id,
                this.Text,
                this.Confidence,
                this.Position);
    }
}
