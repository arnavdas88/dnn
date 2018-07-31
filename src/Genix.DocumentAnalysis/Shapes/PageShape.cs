// -----------------------------------------------------------------------
// <copyright file="PageShape.cs" company="Noname, Inc.">
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
    /// Represents a page.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PageShape : Container<Shape>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageShape"/> class.
        /// </summary>
        /// <param name="shapes">The collection of <see cref="Shape"/> objects contained in this container.</param>
        protected PageShape(IList<Shape> shapes)
            : base(shapes)
        {
        }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(Environment.NewLine, this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;
    }
}