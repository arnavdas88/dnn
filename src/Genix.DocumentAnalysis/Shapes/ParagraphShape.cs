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
    using System.Runtime.CompilerServices;
    using Genix.Geometry;
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
        /// <param name="bounds">The shape boundaries.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParagraphShape(Rectangle bounds)
            : base(bounds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        /// <param name="shapes">The shapes to add to this container.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParagraphShape(Rectangle bounds, IEnumerable<TextLineShape> shapes)
            : base(bounds, shapes)
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