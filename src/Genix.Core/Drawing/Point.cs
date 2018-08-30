// -----------------------------------------------------------------------
// <copyright file="Point.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point other)
        {
            return other.x == this.x && other.y == this.y;
        }

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
        /// <param name="point">The <see cref="Point"/> used offset this <see cref="Point"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(Point point)
        {
            this.Offset(point.X, point.Y);
        }
    }
}
