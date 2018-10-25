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
    using System.Runtime.CompilerServices;
    using Genix.Drawing;
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
        /// <param name="bounds">The shape boundaries.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextBlockShape(Rectangle bounds)
            : base(bounds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        /// <param name="shapes">The shapes to add to this container.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextBlockShape(Rectangle bounds, IEnumerable<Shape> shapes)
            : base(bounds, shapes)
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