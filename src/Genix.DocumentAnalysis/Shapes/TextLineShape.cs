// -----------------------------------------------------------------------
// <copyright file="TextLineShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a horizontal text line.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextLineShape : Container<TextShape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLineShape"/> class.
        /// </summary>
        /// <param name="texts">The collection of <see cref="TextShape"/> contained in this container.</param>
        protected TextLineShape(IList<TextShape> texts)
            : base(texts)
        {
        }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(" ", this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text line: {0}",
            this.Bounds);
    }
}