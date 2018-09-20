// -----------------------------------------------------------------------
// <copyright file="LineShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Globalization;
    using Genix.Core;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a straight line.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class LineShape : Shape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineShape"/> class.
        /// </summary>
        /// <param name="begin">The starting point of the line.</param>
        /// <param name="end">The ending point of the line.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        public LineShape(Point begin, Point end, int width, LineTypes types)
            : base(new Rectangle(begin, end))
        {
            this.Begin = begin;
            this.End = end;
            this.Width = width;
            this.Types = types;

            // if width is greater than corresponding calculated boundary - inflate the boundary
            if (types.HasFlag(LineTypes.Vertical) && width > this.Bounds.Width)
            {
                int dx = width - this.Bounds.Width;
                this.Bounds = Rectangle.Inflate(this.Bounds, dx / 2, 0, this.Bounds.Width - (dx / 2), 0);
            }

            if (types.HasFlag(LineTypes.Horizontal) && width > this.Bounds.Height)
            {
                int dy = width - this.Bounds.Height;
                this.Bounds = Rectangle.Inflate(this.Bounds, 0, dy / 2, 0, this.Bounds.Height - (dy / 2));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineShape"/> class.
        /// </summary>
        /// <param name="bounds">The line boundaries.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        public LineShape(Rectangle bounds, int width, LineTypes types)
            : base(bounds)
        {
            Point center = bounds.CenterPoint;

            this.Begin = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Top) : new Point(bounds.Left, center.Y);
            this.End = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Bottom) : new Point(bounds.Right, center.Y);
            this.Width = width;
            this.Types = types;
        }

        /// <summary>
        /// Gets the starting point of the line.
        /// </summary>
        /// <value>
        /// The starting point of the line.
        /// </value>
        [JsonProperty("begin")]
        public Point Begin { get; }

        /// <summary>
        /// Gets the ending point of the line.
        /// </summary>
        /// <value>
        /// The ending point of the line.
        /// </value>
        [JsonProperty("end")]
        public Point End { get; }

        /// <summary>
        /// Gets the line width, in pixels.
        /// </summary>
        /// <value>
        /// The line width, in pixels.
        /// </value>
        [JsonProperty("width")]
        public int Width { get; }

        /// <summary>
        /// Gets the line type.
        /// </summary>
        /// <value>
        /// The line width, in pixels.
        /// </value>
        [JsonProperty("types")]
        public LineTypes Types { get; }

        /// <inheritdoc />
        public override string Text => null;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Line: {{{0}}} -> {{{1}}}, width: {2}",
            this.Begin,
            this.End,
            this.Width);
    }
}