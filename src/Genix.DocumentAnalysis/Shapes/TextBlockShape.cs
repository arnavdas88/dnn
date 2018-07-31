// -----------------------------------------------------------------------
// <copyright file="TextBlockShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a block of text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextBlockShape : Container<TextLineShape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockShape"/> class.
        /// </summary>
        /// <param name="lines">The collection of <see cref="TextLineShape"/> contained in this container.</param>
        protected TextBlockShape(IList<TextLineShape> lines)
            : base(lines)
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