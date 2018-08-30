// -----------------------------------------------------------------------
// <copyright file="PageShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a page.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PageShape : Container<TextBlockShape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageShape"/> class.
        /// </summary>
        /// <param name="textBlocks">The collection of <see cref="TextBlockShape"/> objects to include in this container.</param>
        public PageShape(IList<TextBlockShape> textBlocks)
            : base(textBlocks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageShape"/> class.
        /// </summary>
        /// <param name="textBlocks">The collection of <see cref="TextBlockShape"/> objects to include in this container.</param>
        /// <param name="bounds">The shape boundaries.</param>
        public PageShape(IList<TextBlockShape> textBlocks, Rectangle bounds)
            : base(textBlocks, bounds)
        {
        }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(Environment.NewLine, this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;
    }
}