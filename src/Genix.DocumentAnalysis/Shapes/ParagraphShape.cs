// -----------------------------------------------------------------------
// <copyright file="ParagraphShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a paragraph of text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ParagraphShape : Container<TextLineShape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphShape"/> class.
        /// </summary>
        /// <param name="textLines">The collection of <see cref="TextLineShape"/> objects to include in this container.</param>
        public ParagraphShape(IList<TextLineShape> textLines)
            : base(textLines)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphShape"/> class.
        /// </summary>
        /// <param name="textLines">The collection of <see cref="TextLineShape"/> objects to include in this container.</param>
        /// <param name="bounds">The shape boundaries.</param>
        public ParagraphShape(IList<TextLineShape> textLines, Rectangle bounds)
            : base(textLines, bounds)
        {
        }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(Environment.NewLine, this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Paragraph: {0}",
            this.Bounds);
    }
}