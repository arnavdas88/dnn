// -----------------------------------------------------------------------
// <copyright file="CheckboxShape.cs" company="Noname, Inc.">
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
    public class CheckboxShape : Shape
    {
        [JsonProperty("checked", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private readonly bool isChecked;

        [JsonProperty("conf", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private readonly float confidence;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CheckboxShape(Rectangle bounds)
            : base(bounds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        /// <param name="isChecked">Determines whether the check box is checked.</param>
        /// <param name="confidence">The word confidence.</param>
        public CheckboxShape(Rectangle bounds, bool isChecked, float confidence)
            : base(bounds)
        {
            this.isChecked = isChecked;
            this.confidence = confidence;
        }

        /// <summary>
        /// Gets a value indicating whether the check box is checked.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the check box is checked; otherwise, <b>false</b>.
        /// </returns>
        public bool IsChecked => this.isChecked;

        /// <inheritdoc />
        public override string Text => this.isChecked ? "X" : string.Empty;

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
            "Check box: {0}",
            this.Bounds);
    }
}