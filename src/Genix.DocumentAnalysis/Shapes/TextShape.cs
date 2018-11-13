// -----------------------------------------------------------------------
// <copyright file="TextShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextShape : Shape
    {
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private readonly string text;

        [JsonProperty("conf", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private readonly float confidence;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextShape(Rectangle bounds)
            : base(bounds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        /// <param name="text">The word text.</param>
        /// <param name="confidence">The word confidence.</param>
        public TextShape(Rectangle bounds, string text, float confidence)
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

        /// <summary>
        /// Gets or sets the text orientation.
        /// </summary>
        /// <value>
        /// The <see cref="Drawing.Orientation"/> enumeration value.
        /// </value>
        [JsonProperty("orientation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text: {0}",
            this.Bounds);
    }
}