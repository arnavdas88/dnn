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
    using Genix.Geometry;
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
        /// <param name="bounds">The shape boundaries.</param>
        /// <param name="horizontalResolution">The page horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The page vertical resolution, in pixels per inch.</param>
        public PageShape(Rectangle bounds, int horizontalResolution, int verticalResolution)
            : base(bounds)
        {
            this.HorizontalResolution = horizontalResolution;
            this.VerticalResolution = verticalResolution;
        }

        /// <summary>
        /// Gets the horizontal resolution, in pixels per inch, of this <see cref="PageShape"/>.
        /// </summary>
        /// <value>
        /// The horizontal resolution, in pixels per inch, of this <see cref="PageShape"/>.
        /// </value>
        [JsonProperty("hres")]
        public int HorizontalResolution { get; private set; }

        /// <summary>
        /// Gets the vertical resolution, in pixels per inch, of this <see cref="PageShape"/>.
        /// </summary>
        /// <value>
        /// The vertical resolution, in pixels per inch, of this <see cref="PageShape"/>.
        /// </value>
        [JsonProperty("vres")]
        public int VerticalResolution { get; private set; }

        /// <inheritdoc />
        public override string Text =>
            this.Shapes.Any() ? string.Join(Environment.NewLine, this.Shapes.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x))) : string.Empty;
    }
}