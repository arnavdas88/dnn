// -----------------------------------------------------------------------
// <copyright file="Line.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a straight line.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Line : Shape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> class.
        /// </summary>
        /// <param name="begin">The starting point of the line.</param>
        /// <param name="end">The ending point of the line.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        public Line(Point begin, Point end, int width, LineTypes types)
        {
            this.Begin = begin;
            this.End = end;
            this.Width = width;
            this.Types = types;

            this.Bounds = Rectangle.FromLTRB(
                Math.Min(begin.X, end.X),
                Math.Min(begin.Y, end.Y),
                Math.Max(begin.X, end.X),
                Math.Max(begin.Y, end.Y));
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
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Line: {{{0}}} -> {{{1}}}, width: {2}",
            this.Begin,
            this.End,
            this.Width);
    }
}