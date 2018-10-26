// -----------------------------------------------------------------------
// <copyright file="Line.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a line in a two-dimensional plane described by two <see cref="Point"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>The line is defined by the equation: <c>y = a * x + b</c>.</para>
    /// </remarks>
    public struct Line
        : IEquatable<Line>
    {
        /// <summary>
        /// Epsilon used in rounding operations.
        /// </summary>
        private const double Eps = 1e-8;

        /// <summary>
        /// The x-coordinate of the first point.
        /// </summary>
        private int x1;

        /// <summary>
        /// The y-coordinate of the first point.
        /// </summary>
        private int y1;

        /// <summary>
        /// The x-coordinate of the second point.
        /// </summary>
        private int x2;

        /// <summary>
        /// The y-coordinate of the second point.
        /// </summary>
        private int y2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> struct
        /// that has the specified x-coordinate and y-coordinate of two points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="x1"/> equals <paramref name="x2"/> and <paramref name="y1"/> equals <paramref name="y2"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2)
            {
                throw new ArgumentException("The line must be defined by two points.");
            }

            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> struct
        /// that has the specified coordinates of two points.
        /// </summary>
        /// <param name="point1">The coordinates of the first point.</param>
        /// <param name="point2">The coordinates of the second point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line(Point point1, Point point2)
            : this(point1.X, point1.Y, point2.X, point2.Y)
        {
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the first point.
        /// </summary>
        /// <value>
        /// The x-coordinate of the first point.
        /// </value>
        public int X1
        {
            get => this.x1;
            set => this.x1 = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the first point.
        /// </summary>
        /// <value>
        /// The y-coordinate of the first point.
        /// </value>
        public int Y1
        {
            get => this.y1;
            set => this.y1 = value;
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the second point.
        /// </summary>
        /// <value>
        /// The x-coordinate of the second point.
        /// </value>
        public int X2
        {
            get => this.x2;
            set => this.x2 = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the second point.
        /// </summary>
        /// <value>
        /// The y-coordinate of the second point.
        /// </value>
        public int Y2
        {
            get => this.y2;
            set => this.y2 = value;
        }

        /// <summary>
        /// Gets the slope of this <see cref="Line"/>.
        /// </summary>
        /// <value>
        /// (<see cref="Y2"/> - <see cref="Y1"/>) / (<see cref="X2"/> - <see cref="X1"/>); <see cref="double.PositiveInfinity"/> if <see cref="X2"/> equals <see cref="X1"/>.
        /// </value>
        public double A => this.x2 == this.x1 ? double.PositiveInfinity : (double)(this.y2 - this.y1) / (this.x2 - this.x1);

        /// <summary>
        /// Gets the slope of this <see cref="Line"/>.
        /// </summary>
        /// <value>
        /// (<see cref="Y2"/> - <see cref="Y1"/>) / (<see cref="X2"/> - <see cref="X1"/>); <see cref="double.PositiveInfinity"/> if <see cref="X2"/> equals <see cref="X1"/>.
        /// </value>
        public double B => this.x2 == this.x1 ? double.PositiveInfinity : (double)(this.y2 - this.y1) / (this.x2 - this.x1);

        /// <summary>
        /// Compares two <see cref="Line"/> objects.
        /// The result specifies whether the values of the <see cref="X1"/>, <see cref="Y1"/>, <see cref="X2"/>, and <see cref="Y2"/> properties of the two <see cref="Line"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="Line"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Line"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="X1"/>, <see cref="Y1"/>, <see cref="X2"/>, and <see cref="Y2"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Line left, Line right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Line"/> objects.
        /// The result specifies whether the values of the <see cref="X1"/>, <see cref="Y1"/>, <see cref="X2"/>, and <see cref="Y2"/> properties of the two <see cref="Line"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="Line"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Line"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="X1"/>, <see cref="Y1"/>, <see cref="X2"/>, or <see cref="Y2"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Line left, Line right) => !left.Equals(right);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Line other) => other.x1 == this.x1 && other.y1 == this.y1 && other.x2 == this.x2 && other.y2 == this.y2;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Line))
            {
                return false;
            }

            return this.Equals((Line)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked(this.x1 ^ this.y1 ^ this.x2 ^ this.y2);

        /// <inheritdoc />
        public override string ToString() =>
            this.x1.ToString(CultureInfo.CurrentCulture) + " " +
            this.y1.ToString(CultureInfo.CurrentCulture) + " " +
            this.x2.ToString(CultureInfo.CurrentCulture) + " " +
            this.y2.ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Computes the y-coordinate of the point using specified x-coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <returns>
        /// The computed y-coordinate.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para><see cref="X1"/> equals <see cref="X2"/>; cannot compute y-coordinate of the point for the vertical line.</para>
        /// </exception>
        public int Y(int x)
        {
            if (this.x1 == this.x2)
            {
                throw new InvalidOperationException("Cannot compute y-coordinate of the point for the vertical line.");
            }

            return (int)Math.Round((this.A * x) + this.B + Line.Eps, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Computes the x-coordinate of the point using specified y-coordinate.
        /// </summary>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>
        /// The computed x-coordinate.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para><see cref="Y1"/> equals <see cref="Y2"/>; cannot compute x-coordinate of the point for the horizontal line.</para>
        /// </exception>
        public int X(int y)
        {
            if (this.y1 == this.y2)
            {
                throw new InvalidOperationException("Cannot compute x-coordinate of the point for the horizontal line.");
            }

            return (int)Math.Round((((double)y - this.B) / this.A) + Line.Eps, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Determines if the specified point is on this <see cref="Line"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>
        /// <b>true</b> if the point defined by <paramref name="x"/> and <paramref name="y"/> is on this <see cref="Line"/>; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(int x, int y) => this.x1 == this.x2 ? this.x1 == x : this.Y(x) == y;

        /// <summary>
        /// Determines if the specified <see cref="Point"/> is on this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to test.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Point"/> represented by <paramref name="point"/> is on this <see cref="Line"/>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Point point) => this.Contains(point.X, point.Y);

        /// <summary>
        /// Determines if the specified point is located above this <see cref="Line"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>
        /// <b>true</b> if the specified point is located above this <see cref="Line"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para><see cref="X1"/> equals <see cref="X2"/>; cannot text the condition, the line is vertical.</para>
        /// </exception>
        public bool IsAbove(int x, int y)
        {
            if (this.x1 == this.x2)
            {
                throw new InvalidOperationException("Cannot text the condition, the line is vertical.");
            }

            return y - this.y1 < (x - this.x1).MulDiv(this.y2 - this.y1, this.x2 - this.x1);
        }

        /// <summary>
        /// Determines if the specified point is located below this <see cref="Line"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>
        /// <b>true</b> if the specified point is located below this <see cref="Line"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para><see cref="X1"/> equals <see cref="X2"/>; cannot text the condition, the line is vertical.</para>
        /// </exception>
        public bool IsBelow(int x, int y)
        {
            if (this.x1 == this.x2)
            {
                throw new InvalidOperationException("Cannot text the condition, the line is vertical.");
            }

            return y - this.y1 > (x - this.x1).MulDiv(this.y2 - this.y1, this.x2 - this.x1);
        }
    }
}
