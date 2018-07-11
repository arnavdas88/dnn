// -----------------------------------------------------------------------
// <copyright file="TextShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.DocumentAnalysis
{
    using System.Drawing;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextShape : Shape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        protected TextShape(Rectangle bounds) : base(bounds)
        {
        }

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text: {0}",
            this.Bounds);
    }
}