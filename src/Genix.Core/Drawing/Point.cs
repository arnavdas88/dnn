// -----------------------------------------------------------------------
// <copyright file="Point.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    [TypeConverter(typeof(PointConverter))]
    [JsonConverter(typeof(PointJsonConverter))]
    public struct Point
        : IEquatable<Point>
    {
        /// <summary>
        /// Represents a <see cref="Point"/> that has <see cref="X"/> and <see cref="Y"/> values set to zero.
        /// </summary>
        public static readonly Point Empty;

        /// <summary>
        /// The x-coordinate of this <see cref="Point"/>.
        /// </summary>
        private int x;

        /// <summary>
        /// The y-coordinate of this <see cref="Point"/>.
        /// </summary>
        private int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal position of the point.</param>
        /// <param name="y">The vertical position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct from the <see cref="System.Drawing.Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="System.Drawing.Point"/> that contains the position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point(System.Drawing.Point point)
        {
            this.x = point.X;
            this.y = point.Y;
        }

        /// <summary>
        /// Gets or sets the x-coordinate of this <see cref="Point"/>.
        /// </summary>
        /// <value>
        /// The x-coordinate of this <see cref="Point"/>.
        /// </value>
        public int X
        {
            get => this.x;
            set => this.x = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of this <see cref="Point"/>.
        /// </summary>
        /// <value>
        /// The y-coordinate of this <see cref="Point"/>.
        /// </value>
        public int Y
        {
            get => this.y;
            set => this.y = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Point"/> is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if both <see cref="X"/> and <see cref="Y"/> are 0; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.x == 0 && this.y == 0;

        /// <summary>
        /// Compares two <see cref="Point"/> objects.
        /// The result specifies whether the values of the <see cref="X"/> and <see cref="Y"/> properties of the two <see cref="Point"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="Point"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Point"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="X"/> and <see cref="Y"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Point left, Point right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Point"/> objects.
        /// The result specifies whether the values of the <see cref="X"/> and <see cref="Y"/> properties of the two <see cref="Point"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="Point"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Point"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="X"/> and <see cref="Y"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="Point"/> in the following format:X Y.</param>
        /// <returns>The <see cref="Point"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of two values represented by an optional sign followed by a sequence of digits (0 through 9).
        /// </exception>
        public static Point Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] split = value.Split(' ');
            if (split?.Length == 2 &&
                int.TryParse(split[0], out int x) &&
                int.TryParse(split[1], out int y))
            {
                return new Point(x, y);
            }
            else
            {
                throw new ArgumentException(Genix.Core.Properties.Resources.E_InvalidPointFormat, nameof(value));
            }
        }

        /// <summary>
        /// Scales the specified <see cref="Point"/> location.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to scale.</param>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        /// <returns>The scaled <see cref="Point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Scale(Point point, int dx, int dy) => new Point(point.x * dx, point.y * dy);

        /// <summary>
        /// Scales the specified <see cref="Point"/> location.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to scale.</param>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        /// <returns>The scaled <see cref="Point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Scale(Point point, float dx, float dy)
        {
            return new Point(
                (int)Math.Round(dx * point.x, MidpointRounding.AwayFromZero),
                (int)Math.Round(dy * point.y, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Translates the specified <see cref="Point"/> by the specified amount.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to translate.</param>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        /// <returns>The translated <see cref="Point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Offset(Point point, int dx, int dy) => new Point(point.x + dx, point.y + dy);

        /// <summary>
        /// Translates the specified <see cref="Point"/> by the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to translate.</param>
        /// <param name="offset">The <see cref="Point"/> that contains the offset for the <paramref name="point"/>.</param>
        /// <returns>The translated <see cref="Point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Offset(Point point, Point offset) => Point.Offset(point, offset.x, offset.y);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point other) => other.x == this.x && other.y == this.y;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            return this.Equals((Point)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked(this.x ^ this.y);

        /// <inheritdoc />
        public override string ToString() =>
            this.x.ToString(CultureInfo.CurrentCulture) + " " + this.y.ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Sets <see cref="X"/> and <see cref="Y"/> values set to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.x = this.y = 0;

        /// <summary>
        /// Sets this <see cref="Point"/> position.
        /// </summary>
        /// <param name="x">The horizontal position of the point.</param>
        /// <param name="y">The vertical position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Translates this <see cref="Point"/> by the specified amount.
        /// </summary>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(int dx, int dy)
        {
            this.x += dx;
            this.y += dy;
        }

        /// <summary>
        /// Translates this <see cref="Point"/> by the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> that contains the offset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(Point point) => this.Offset(point.X, point.Y);

        /// <summary>
        /// Scales this <see cref="Point"/> location.
        /// </summary>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(int dx, int dy)
        {
            this.x *= dx;
            this.y *= dy;
        }

        /// <summary>
        /// Scales this <see cref="Point"/> location.
        /// </summary>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(float dx, float dy)
        {
            this.x = (int)Math.Round(dx * this.x, MidpointRounding.AwayFromZero);
            this.y = (int)Math.Round(dy * this.y, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Computes the Euclidean distance between this <see cref="Point"/> and the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the Euclidean distance between this <see cref="Point"/> and <paramref name="point"/>.
        /// </returns>
        public float DistanceTo(Point point) => (float)Math.Sqrt(this.DistanceToSquared(point));

        /// <summary>
        /// Computes the Euclidean distance between this <see cref="Point"/> and the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the squared Euclidean distance between this <see cref="Point"/> and <paramref name="point"/>.
        /// </returns>
        public int DistanceToSquared(Point point)
        {
            int dx = this.x - point.x;
            int dy = this.y - point.y;
            return (dx * dx) + (dy * dy);
        }
    }
}
