// -----------------------------------------------------------------------
// <copyright file="WordShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Drawing;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class WordShape : Shape
    {
        [JsonProperty("text")]
        private readonly string text;

        [JsonProperty("confidence")]
        private readonly float confidence;

        /// <summary>
        /// Initializes a new instance of the <see cref="WordShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        /// <param name="text">The word text.</param>
        /// <param name="confidence">The word confidence.</param>
        public WordShape(Rectangle bounds, string text, float confidence)
            : base(bounds)
        {
            this.text = text;
            this.confidence = confidence;
        }

        /// <inheritdoc />
        public override string Text => this.text;

        /// <summary>
        /// Gets the word confidence.
        /// </summary>
        /// <returns>
        /// The word confidence expressed as a probability (0.0 - 1.0).
        /// </returns>
        public float Confidence => this.confidence;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text: {0}",
            this.Bounds);
    }
}