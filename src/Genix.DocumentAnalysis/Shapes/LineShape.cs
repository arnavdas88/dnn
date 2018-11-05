// -----------------------------------------------------------------------
// <copyright file="LineShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
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
        [JsonProperty("begin")]
        private Point begin;

        [JsonProperty("end")]
        private Point end;

        [JsonProperty("width")]
        private int width;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineShape"/> class.
        /// </summary>
        /// <param name="begin">The starting point of the line.</param>
        /// <param name="end">The ending point of the line.</param>
        /// <param name="width">The line width, in pixels.</param>
        /// <param name="types">The line types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="width"/> is zero or less.
        /// </exception>
        public LineShape(Point begin, Point end, int width, LineTypes types)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "The line width is invalid.");
            }

            this.begin = begin;
            this.end = end;
            this.width = width;
            this.Types = types;

            this.UpdateBounds();
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
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "The line width is invalid.");
            }

            Point center = bounds.CenterPoint;

            this.begin = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Top) : new Point(bounds.Left, center.Y);
            this.end = types.HasFlag(LineTypes.Vertical) ? new Point(center.X, bounds.Bottom - 1) : new Point(bounds.Right - 1, center.Y);
            this.width = width;
            this.Types = types;

            this.UpdateBounds();
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

            this.begin = other.begin;
            this.end = other.end;
            this.width = other.width;
            this.Types = other.Types;
        }

        /// <summary>
        /// Gets or sets the starting point of the line.
        /// </summary>
        /// <value>
        /// The starting point of the line.
        /// </value>
        public Point Begin
        {
            get => this.begin;
            set
            {
                this.begin = value;
                this.UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the ending point of the line.
        /// </summary>
        /// <value>
        /// The ending point of the line.
        /// </value>
        public Point End
        {
            get => this.end;
            set
            {
                this.end = value;
                this.UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the line width, in pixels.
        /// </summary>
        /// <value>
        /// The line width, in pixels.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value is zero or less.
        /// </exception>
        public int Width
        {
            get => this.width;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The line width is invalid.");
                }

                this.width = value;
                this.UpdateBounds();
            }
        }

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

        private void UpdateBounds()
        {
            Rectangle bounds = new Rectangle(this.Begin, this.End);

            int dw = (this.Width - 1) / 2;
            if (this.Types.HasFlag(LineTypes.Vertical))
            {
                bounds.Inflate(dw, 0, this.Width - dw, 0);
            }

            if (this.Types.HasFlag(LineTypes.Horizontal))
            {
                bounds.Inflate(0, dw, 0, this.Width - dw);
            }

            this.Bounds = bounds;
        }
    }
}