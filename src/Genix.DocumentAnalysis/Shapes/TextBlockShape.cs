// -----------------------------------------------------------------------
// <copyright file="TextBlockShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a block of information.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextBlockShape : Container<Shape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockShape"/> class.
        /// </summary>
        /// <param name="textLines">The collection of <see cref="Shape"/> objects to include in this container.</param>
        public TextBlockShape(IList<Shape> textLines)
            : base(textLines)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockShape"/> class.
        /// </summary>
        /// <param name="textLines">The collection of <see cref="Shape"/> objects to include in this container.</param>
        /// <param name="bounds">The shape boundaries.</param>
        public TextBlockShape(IList<Shape> textLines, Rectangle bounds)
            : base(textLines, bounds)
        {
        }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(Environment.NewLine, this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text block: {0}",
            this.Bounds);
    }
}