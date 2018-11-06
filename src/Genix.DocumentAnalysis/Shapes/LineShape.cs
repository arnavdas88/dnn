// -----------------------------------------------------------------------
// <copyright file="LineShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Globalization;
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
        /// <param name="bounds">The line bounding box.</param>
        /// <param name="begin">The starting point of the line.</param>
        /// <param name="end">The ending point of the line.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="width"/> is zero or less.
        /// </exception>
        public LineShape(Rectangle bounds, Point begin, Point end, int width, LineTypes types)
            : base(bounds)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "The line width is invalid.");
            }

            this.Begin = begin;
            this.End = end;
            this.Width = width;
            this.Types = types;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineShape"/> class.
        /// </summary>
        /// <param name="bounds">The line boundaries.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="width"/> is zero or less.
        /// </exception>
        public LineShape(Rectangle bounds, int width, LineTypes types)
            : base(bounds)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "The line width is invalid.");
            }

            Point center = bounds.CenterPoint;

            this.Begin = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Top) : new Point(bounds.Left, center.Y);
            this.End = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Bottom) : new Point(bounds.Right, center.Y);
            this.Width = width;
            this.Types = types;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineShape"/> class.
        /// </summary>
        /// <param name="other">The source <see cref="LineShape"/>.</param>
        public LineShape(LineShape other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Bounds = other.Bounds;

            this.Begin = other.Begin;
            this.End = other.End;
            this.Width = other.Width;
            this.Types = other.Types;
        }

        /// <summary>
        /// Gets the starting point of the line.
        /// </summary>
        /// <value>
        /// The starting point of the line.
        /// </value>
        [JsonProperty("begin")]
        public Point Begin { get; private set; }

        /// <summary>
        /// Gets the ending point of the line.
        /// </summary>
        /// <value>
        /// The ending point of the line.
        /// </value>
        [JsonProperty("end")]
        public Point End { get; private set; }

        /// <summary>
        /// Gets the line width, in pixels.
        /// </summary>
        /// <value>
        /// The line width, in pixels.
        /// </value>
        [JsonProperty("width")]
        public int Width { get; private set; }

        /// <summary>
        /// Gets the line type.
        /// </summary>
        /// <value>
        /// The line width, in pixels.
        /// </value>
        [JsonProperty("types")]
        public LineTypes Types { get; private set; }

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