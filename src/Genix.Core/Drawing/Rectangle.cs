﻿// -----------------------------------------------------------------------
// <copyright file="Rectangle.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the width, height, and location of a rectangle in a two-dimensional plane.
    /// </summary>
    [TypeConverter(typeof(RectangleConverter))]
    [JsonConverter(typeof(RectangleJsonConverter))]
    public struct Rectangle
        : IEquatable<Rectangle>
    {
        /// <summary>
        /// Represents a a rectangle with no position or area.
        /// </summary>
        /// <value>
        /// The empty rectangle, which has <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> property values of 0.
        /// </value>
        public static readonly Rectangle Empty;

        /// <summary>
        /// Epsilon used in rounding operations.
        /// </summary>
        private const float Eps = 1e-8f;

        /// <summary>
        /// The x-coordinate of the top-left corner of the rectangle.
        /// </summary>
        private int x;

        /// <summary>
        /// The y-coordinate of the top-left corner of the rectangle.
        /// </summary>
        private int y;

        /// <summary>
        /// The rectangle width.
        /// </summary>
        private int w;

        /// <summary>
        /// The rectangle height.
        /// </summary>
        private int h;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct
        /// that has the specified x-coordinate, y-coordinate, width, and height.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.w = width >= 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Core.Properties.Resources.E_InvalidRectangleWidth);
            this.h = height >= 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Core.Properties.Resources.E_InvalidRectangleHeight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct
        /// with the specified location and size.
        /// </summary>
        /// <param name="location">The x- and y-coordinates of the top-left corner of the rectangle.</param>
        /// <param name="size">The dimensions of the rectangle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(Point location, Size size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.w = size.Width;
            this.h = size.Height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct
        /// using two corner points.
        /// </summary>
        /// <param name="pt1">The first point.</param>
        /// <param name="pt2">The second point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(Point pt1, Point pt2)
        {
            this.x = MinMax.Min(pt1.X, pt2.X);
            this.y = MinMax.Min(pt1.Y, pt2.Y);
            this.w = MinMax.Max(pt1.X, pt2.X) - this.x;
            this.h = MinMax.Max(pt1.Y, pt2.Y) - this.y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct from the <see cref="System.Drawing.Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="System.Drawing.Rectangle"/> that contains the position of the point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><see cref="System.Drawing.Rectangle.Width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><see cref="System.Drawing.Rectangle.Height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(System.Drawing.Rectangle rect)
        {
            this.x = rect.X;
            this.y = rect.Y;
            this.w = rect.Width >= 0 ? rect.Width : throw new ArgumentOutOfRangeException(nameof(rect), Core.Properties.Resources.E_InvalidRectangleWidth);
            this.h = rect.Height >= 0 ? rect.Height : throw new ArgumentOutOfRangeException(nameof(rect), Core.Properties.Resources.E_InvalidRectangleHeight);
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the left side of the rectangle.
        /// </summary>
        /// <value>
        /// The x-coordinate of the left side of the rectangle.
        /// </value>
        /// <remarks>
        /// <para>Getting this property is equivalent to getting the <see cref="Left"/> property.</para>
        /// <para>Changing the <see cref="X"/> property will also cause a change in the <see cref="Right"/> property of the <see cref="Rectangle"/>.</para>
        /// </remarks>
        public int X
        {
            get => this.x;
            set => this.x = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the top side of the rectangle.
        /// </summary>
        /// <value>
        /// The y-coordinate of the top side of the rectangle.
        /// </value>
        /// <remarks>
        /// <para>Getting this property is equivalent to getting the <see cref="Top"/> property.</para>
        /// <para>Changing the <see cref="Y"/> property will also cause a change in the <see cref="Bottom"/> property of the <see cref="Rectangle"/>.</para>
        /// </remarks>
        public int Y
        {
            get => this.y;
            set => this.y = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangle.
        /// </summary>
        /// <value>
        /// A positive number that represents the width of the rectangle. The default is 0.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="Width"/> is set to a negative value.
        /// </exception>
        /// <remarks>
        /// Changing the <see cref="Width"/> property will also cause a change in the <see cref="Right"/> property of the <see cref="Rectangle"/>.
        /// </remarks>
        public int Width
        {
            get => this.w;
            set => this.w = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), Core.Properties.Resources.E_InvalidRectangleWidth);
        }

        /// <summary>
        /// Gets or sets the height of the rectangle.
        /// </summary>
        /// <value>
        /// A positive number that represents the height of the rectangle. The default is 0.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="Height"/> is set to a negative value.
        /// </exception>
        /// <remarks>
        /// Changing the <see cref="Height"/> property will also cause a change in the <see cref="Bottom"/> property of the <see cref="Rectangle"/>.
        /// </remarks>
        public int Height
        {
            get => this.h;
            set => this.h = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), Core.Properties.Resources.E_InvalidRectangleHeight);
        }

        /// <summary>
        /// Gets the x-coordinate of the left side of the rectangle.
        /// </summary>
        /// <value>
        /// The x-coordinate of the left side of the rectangle.
        /// </value>
        /// <remarks>
        /// Getting this property is equivalent to getting the <see cref="X"/> property.
        /// </remarks>
        public int Left => this.x;

        /// <summary>
        /// Gets the x-coordinate of the right side of the rectangle.
        /// </summary>
        /// <value>
        /// The x-coordinate of the right side of the rectangle.
        /// </value>
        /// <remarks>
        /// <para>The value of the <see cref="Right"/> property represents the x-coordinate of the first point at the right edge of the rectangle that is not contained in the <see cref="Rectangle"/>.</para>
        /// <para>The value of the property is equal to the sum of the <see cref="X"/> and <see cref="Width"/> properties.</para>
        /// </remarks>
        public int Right => this.x + this.w;

        /// <summary>
        /// Gets the y-coordinate of the top side of the rectangle.
        /// </summary>
        /// <value>
        /// The y-coordinate of the top side of the rectangle.
        /// </value>
        /// <remarks>
        /// Getting this property is equivalent to getting the <see cref="Y"/> property.
        /// </remarks>
        public int Top => this.y;

        /// <summary>
        /// Gets the y-coordinate of the bottom side of the rectangle.
        /// </summary>
        /// <value>
        /// The y-coordinate of the bottom side of the rectangle.
        /// </value>
        /// <remarks>
        /// <para>The value of the <see cref="Bottom"/> property represents the y-coordinate of the first point at the bottom edge of the rectangle that is not contained in the <see cref="Rectangle"/>.</para>
        /// <para>The value of the property is equal to the sum of the <see cref="Y"/> and <see cref="Height"/> properties.</para>
        /// </remarks>
        public int Bottom => this.y + this.h;

        /// <summary>
        /// Gets a value indicating whether all numeric properties of this <see cref="Rectangle"/> have value of zero.
        /// </summary>
        /// <value>
        /// <b>true</b> if <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/> and <see cref="Height"/> properties are zero; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.x == 0 && this.y == 0 && this.w == 0 && this.h == 0;

        /// <summary>
        /// Gets the location of this <see cref="Rectangle"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Point"/> struct that contains the x- and y-coordinates of the top-left corner of the rectangle.
        /// </value>
        public Point Location => new Point(this.x, this.y);

        /// <summary>
        /// Gets the size of this <see cref="Rectangle"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Size"/> struct that contains rectangle dimensions.
        /// </value>
        public Size Size => new Size(this.w, this.h);

        /// <summary>
        /// Gets the area of this <see cref="Rectangle"/>.
        /// </summary>
        /// <value>
        /// <see cref="Width"/> * <see cref="Height"/>.
        /// </value>
        public int Area => this.w * this.h;

        /// <summary>
        /// Gets the center <see cref="Point"/> of this <see cref="Rectangle"/>.
        /// </summary>
        /// <value>
        /// The center <see cref="Point"/> of this <see cref="Rectangle"/>.
        /// </value>
        public Point CenterPoint => new Point(this.x + (this.w / 2), this.y + (this.h / 2));

        /// <summary>
        /// Compares two <see cref="Rectangle"/> objects.
        /// The result specifies whether the values of the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties of the two <see cref="Rectangle"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="Rectangle"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Rectangle"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rectangle"/> objects.
        /// The result specifies whether the values of the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties of the two <see cref="Rectangle"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="Rectangle"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Rectangle"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, or <see cref="Height"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);

        /// <summary>
        /// Creates a <see cref="Rectangle"/> structure with the specified edge locations.
        /// </summary>
        /// <param name="left">The x-coordinate of the upper-left corner of this <see cref="Rectangle"/> structure.</param>
        /// <param name="top">The y-coordinate of the upper-left corner of this <see cref="Rectangle"/> structure.</param>
        /// <param name="right">The x-coordinate of the lower-right corner of this <see cref="Rectangle"/> structure.</param>
        /// <param name="bottom">The y-coordinate of the lower-right corner of this <see cref="Rectangle"/> structure.</param>
        /// <returns>The new <see cref="Rectangle"/> that this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle FromLTRB(int left, int top, int right, int bottom) => new Rectangle(left, top, right - left, bottom - top);

        /// <summary>
        /// Creates a rectangle that results from expanding or shrinking the specified rectangle by the specified width and height amounts, in all directions.
        /// </summary>
        /// <param name="rect">The rectangle to shrink or expand.</param>
        /// <param name="dx">The amount by which to expand or shrink the left and right sides of the rectangle.</param>
        /// <param name="dy">The amount by which to expand or shrink the top and bottom sides of the rectangle.</param>
        /// <returns>The resulting rectangle.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Width"/> of the resulting rectangle is increased or decreased by twice the specified width offset,
        /// because it is applied to both the left and right sides of the rectangle.
        /// Likewise, the <see cref="Height"/> of the resulting rectangle is increased or decreased by twice the specified height.
        /// </para>
        /// <para>
        /// If either <paramref name="dx"/> or <paramref name="dy"/> is negative, the <see cref="Rectangle"/> structure is deflated in the corresponding direction.
        /// </para>
        /// <para>
        /// If the specified width or height shrink the rectangle by more than its current <see cref="Width"/> or <see cref="Height"/>
        /// giving the rectangle a negative area, the rectangle becomes the <see cref="Rectangle.Empty"/> rectangle.</para>
        /// </remarks>
        public static Rectangle Inflate(Rectangle rect, int dx, int dy)
        {
            return new Rectangle(
                rect.X - dx,
                rect.Y - dy,
                MinMax.Max(rect.w + (2 * dx), 0),
                MinMax.Max(rect.h + (2 * dy), 0));
        }

        /// <summary>
        /// Creates a rectangle that results from expanding or shrinking the specified rectangle by the specified dimensions, in all directions.
        /// </summary>
        /// <param name="rect">The rectangle to shrink or expand.</param>
        /// <param name="size">The amount by which to expand or shrink the left, right and top, bottom sides of the rectangle.</param>
        /// <returns>The resulting rectangle.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Width"/> of the resulting rectangle is increased by twice the specified horizontal dimension,
        /// because it is applied to both the left and right sides of the rectangle.
        /// Likewise, the <see cref="Height"/> of the resulting rectangle is increased by twice the vertical dimension.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Inflate(Rectangle rect, Size size) => Rectangle.Inflate(rect, size.Width, size.Height);

        /// <summary>
        /// Creates a rectangle that results from expanding or shrinking the specified rectangle by the specified amounts, in all directions.
        /// </summary>
        /// <param name="rect">The rectangle to shrink or expand.</param>
        /// <param name="left">The amount by which to expand or shrink the left side of the rectangle.</param>
        /// <param name="top">The amount by which to expand or shrink the top side of the rectangle.</param>
        /// <param name="right">The amount by which to expand or shrink the right side of the rectangle.</param>
        /// <param name="bottom">The amount by which to expand or shrink the bottom side of the rectangle.</param>
        /// <returns>The resulting rectangle.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Width"/> of the resulting rectangle is increased or decreased by the sum of <paramref name="left"/> and <paramref name="right"/>,
        /// because it is applied to both the left and right sides of the rectangle.
        /// Likewise, the <see cref="Height"/> of the resulting rectangle is increased or decreased by the sum of <paramref name="top"/> and <paramref name="bottom"/>.
        /// </para>
        /// <para>
        /// If either sum is negative, the <see cref="Rectangle"/> structure is deflated in the corresponding direction.
        /// </para>
        /// <para>
        /// If the specified parameters shrink the rectangle by more than its current <see cref="Width"/> or <see cref="Height"/>
        /// giving the rectangle a negative area, the rectangle becomes the <see cref="Rectangle.Empty"/> rectangle.</para>
        /// </remarks>
        public static Rectangle Inflate(Rectangle rect, int left, int top, int right, int bottom)
        {
            return new Rectangle(
                rect.X - left,
                rect.Y - top,
                MinMax.Max(rect.w + left + right, 0),
                MinMax.Max(rect.h + top + bottom, 0));
        }

        /// <summary>
        /// Returns a <see cref="Rectangle"/> structure that represents the intersection of two other <see cref="Rectangle"/> structures.
        /// If there is no intersection, an empty <see cref="Rectangle"/> is returned.
        /// </summary>
        /// <param name="rect1">The first rectangle to intersect.</param>
        /// <param name="rect2">The second rectangle to intersect.</param>
        /// <returns>
        /// The intersection of the two rectangles,
        /// or <see cref="Rectangle.Empty"/> if no intersection exists.
        /// </returns>
        public static Rectangle Intersect(Rectangle rect1, Rectangle rect2)
        {
            int x1 = MinMax.Max(rect1.x, rect2.x);
            int x2 = MinMax.Min(rect1.x + rect1.w, rect2.x + rect2.w);
            int y1 = MinMax.Max(rect1.y, rect2.y);
            int y2 = MinMax.Min(rect1.y + rect1.h, rect2.y + rect2.h);

            return x2 >= x1 && y2 >= y1 ? Rectangle.FromLTRB(x1, y1, x2, y2) : Rectangle.Empty;
        }

        /// <summary>
        /// Translates the specified <see cref="Rectangle"/> by the specified amount.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to translate.</param>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        /// <returns>The translated <see cref="Rectangle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Offset(Rectangle rect, int dx, int dy) => new Rectangle(rect.x + dx, rect.y + dy, rect.w, rect.h);

        /// <summary>
        /// Translates the specified <see cref="Rectangle"/> by the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to translate.</param>
        /// <param name="offset">The <see cref="Point"/> that contains the offset for the <paramref name="rect"/>.</param>
        /// <returns>The translated <see cref="Rectangle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Offset(Rectangle rect, Point offset) => Rectangle.Offset(rect, offset.X, offset.Y);

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="Rectangle"/> in the following format:X Y Width Height.</param>
        /// <returns>The <see cref="Rectangle"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of four values represented by an optional sign followed by a sequence of digits (0 through 9).
        /// </exception>
        public static Rectangle Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] split = value.Split(' ');
            if (split?.Length == 4 &&
                int.TryParse(split[0], out int x) &&
                int.TryParse(split[1], out int y) &&
                int.TryParse(split[2], out int w) &&
                int.TryParse(split[3], out int h))
            {
                return new Rectangle(x, y, w, h);
            }
            else
            {
                throw new ArgumentException(Genix.Core.Properties.Resources.E_InvalidRectangleFormat, nameof(value));
            }
        }

        /// <summary>
        /// Creates a <see cref="Rectangle"/> that results from scaling the location and dimensions of specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to scale.</param>
        /// <param name="dx">The amount by which to scale the left position and the width of the rectangle.</param>
        /// <param name="dy">The amount by which to scale the top position and the height of the rectangle.</param>
        /// <returns>The resulting <see cref="Rectangle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Scale(Rectangle rect, int dx, int dy) =>
            new Rectangle(rect.x * dx, rect.y * dy, rect.w * dx, rect.h * dy);

#if false
        /// <summary>
        /// Scales the specified <see cref="Rectangle"/> location.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to scale.</param>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        /// <returns>The scaled <see cref="Rectangle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Scale(Rectangle rect, float dx, float dy)
        {
            return new Rectangle(
                (int)Math.Round(dx * rect.x, MidpointRounding.AwayFromZero),
                (int)Math.Round(dy * rect.y, MidpointRounding.AwayFromZero));
        }
#endif

        /// <summary>
        /// Returns a <see cref="Rectangle"/> structure that contains the union of two other <see cref="Rectangle"/> structures.
        /// </summary>
        /// <param name="rect1">The first rectangle to union.</param>
        /// <param name="rect2">The second rectangle to union.</param>
        /// <returns>
        /// The <see cref="Rectangle"/> structure that bounds the union of the two <see cref="Rectangle"/> structures.
        /// </returns>
        public static Rectangle Union(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.IsEmpty)
            {
                return rect2;
            }

            if (rect2.IsEmpty)
            {
                return rect1;
            }

            int x1 = MinMax.Min(rect1.x, rect2.x);
            int x2 = MinMax.Max(rect1.x + rect1.w, rect2.x + rect2.w);
            int y1 = MinMax.Min(rect1.y, rect2.y);
            int y2 = MinMax.Max(rect1.y + rect1.h, rect2.y + rect2.h);

            return Rectangle.FromLTRB(x1, y1, x2, y2);
        }

        /// <summary>
        /// Returns a <see cref="Rectangle"/> structure that contains the union of a <see cref="Rectangle"/> structure
        /// and a rectangular area represented by its x-coordinate, y-coordinate, width, and height.
        /// </summary>
        /// <param name="rect">The first rectangle to union.</param>
        /// <param name="x">The x-coordinate of the top-left corner of the second rectangle to union.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the second rectangle to union.</param>
        /// <param name="width">The width of the second rectangle to union.</param>
        /// <param name="height">The height of the second rectangle to union.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        /// <returns>
        /// The <see cref="Rectangle"/> structure that bounds the union of the <see cref="Rectangle"/> structure and a rectangular area.
        /// </returns>
        public static Rectangle Union(Rectangle rect, int x, int y, int width, int height)
        {
            if (rect.IsEmpty)
            {
                return new Rectangle(x, y, width, height);
            }

            if (x == 0 && y == 0 && width == 0 && height == 0)
            {
                return rect;
            }

            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), Core.Properties.Resources.E_InvalidRectangleWidth);
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), Core.Properties.Resources.E_InvalidRectangleHeight);
            }

            int x1 = MinMax.Min(rect.x, x);
            int x2 = MinMax.Max(rect.x + rect.w, x + width);
            int y1 = MinMax.Min(rect.y, y);
            int y2 = MinMax.Max(rect.y + rect.h, y + height);

            return Rectangle.FromLTRB(x1, y1, x2, y2);
        }

        /// <summary>
        /// Returns a <see cref="Rectangle"/> structure that contains the union of the sequence of <see cref="Rectangle"/> structures.
        /// </summary>
        /// <param name="values">The rectangles to union.</param>
        /// <returns>
        /// A <see cref="Rectangle"/> structure that bounds the union of the sequence of <see cref="Rectangle"/> structures.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Union(IEnumerable<Rectangle> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Rectangle result = Rectangle.Empty;
            foreach (Rectangle value in values)
            {
                result.Union(value);
            }

            return result;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rectangle other) => other.x == this.x && other.y == this.y && other.w == this.w && other.h == this.h;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
            {
                return false;
            }

            return this.Equals((Rectangle)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked(this.x ^ this.y ^ this.w ^ this.h);

        /// <inheritdoc />
        public override string ToString() =>
            this.x.ToString(CultureInfo.CurrentCulture) + " " +
            this.y.ToString(CultureInfo.CurrentCulture) + " " +
            this.w.ToString(CultureInfo.CurrentCulture) + " " +
            this.h.ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Set the rectangle x-coordinate, y-coordinate, width, and height to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.x = this.y = this.w = this.h = 0;

        /// <summary>
        /// Determines if the specified point is contained within this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>
        /// <b>true</b> if the point defined by <paramref name="x"/> and <paramref name="y"/> is contained within this <see cref="Rectangle"/> structure; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int x, int y) => this.ContainsX(x) && this.ContainsY(y);

        /// <summary>
        /// Determines if the specified <see cref="Point"/> is contained within this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to test.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Point"/> represented by <paramref name="point"/> is contained within this <see cref="Rectangle"/> structure; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Point point) => this.Contains(point.X, point.Y);

        /// <summary>
        /// Determines if the rectangular region represented by <paramref name="rect"/> is contained within this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
        /// <returns>
        /// <b>true</b> if the rectangular region represented by <paramref name="rect"/> is contained within this <see cref="Rectangle"/> structure; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(Rectangle rect)
        {
            return
                this.x <= rect.x && rect.x + rect.w <= this.x + this.w &&
                this.y <= rect.y && rect.y + rect.h <= this.y + this.h;
        }

        /// <summary>
        /// Determines if the specified x-coordinate is contained within this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <param name="x">The x-coordinate to check.</param>
        /// <returns><b>true</b> if <paramref name="x"/> is contained within this <see cref="Rectangle"/> along its x-axis; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsX(int x) => x.Between(this.x, this.x + this.w - 1);

        /// <summary>
        /// Determines if the specified y-coordinate is contained within this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <param name="y">The y-coordinate to check.</param>
        /// <returns><b>true</b> if <paramref name="y"/> is contained within this <see cref="Rectangle"/> along its y-axis; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsY(int y) => y.Between(this.y, this.y + this.h - 1);

        /// <summary>
        /// Computes the Euclidean distance between this <see cref="Rectangle"/> and the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the Euclidean distance between this <see cref="Rectangle"/> and <paramref name="point"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(Point point) => (float)Math.Sqrt(this.DistanceToSquared(point));

        /// <summary>
        /// Computes the Euclidean distance between this <see cref="Rectangle"/> and the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the Euclidean distance between this <see cref="Rectangle"/> and <paramref name="rect"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(Rectangle rect) => (float)Math.Sqrt(this.DistanceToSquared(rect));

        /// <summary>
        /// Computes the squared Euclidean distance between this <see cref="Rectangle"/> and the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the squared Euclidean distance between this <see cref="Rectangle"/> and <paramref name="point"/>.
        /// </returns>
        public int DistanceToSquared(Point point)
        {
            int dx = this.DistanceToX(point);
            int dy = this.DistanceToY(point);
            return (dx * dx) + (dy * dy);
        }

        /// <summary>
        /// Computes the squared Euclidean distance between this <see cref="Rectangle"/> and the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the squared Euclidean distance between this <see cref="Rectangle"/> and <paramref name="rect"/>.
        /// </returns>
        public int DistanceToSquared(Rectangle rect)
        {
            int dx = this.DistanceToX(rect);
            int dy = this.DistanceToY(rect);
            return (dx * dx) + (dy * dy);
        }

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified point along x-axis.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="x"/> along x-axis.
        /// </returns>
        public int DistanceToX(int x)
        {
            int distance = this.x - x;
            if (distance < 0)
            {
                distance = x - (this.x + this.w);
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified point along y-axis.
        /// </summary>
        /// <param name="y">The y-coordinate of the point to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="y"/> along y-axis.
        /// </returns>
        public int DistanceToY(int y)
        {
            int distance = this.y - y;
            if (distance < 0)
            {
                distance = y - (this.y + this.h);
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified <see cref="Point"/> along x-axis.
        /// </summary>
        /// <param name="point">The x-coordinate of the <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="point"/> along x-axis.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistanceToX(Point point) => this.DistanceToX(point.X);

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified <see cref="Point"/> along y-axis.
        /// </summary>
        /// <param name="point">The y-coordinate of the <see cref="Point"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="point"/> along y-axis.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistanceToY(Point point) => this.DistanceToX(point.Y);

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified <see cref="Rectangle"/> along x-axis.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="rect"/> along x-axis.
        /// </returns>
        public int DistanceToX(Rectangle rect)
        {
            int distance = this.x - (rect.x + rect.w);
            if (distance < 0)
            {
                distance = rect.x - (this.x + this.w);
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Computes the distance between this <see cref="Rectangle"/> and the specified <see cref="Rectangle"/> along y-axis.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to compute the distance to.</param>
        /// <returns>
        /// A value that represents the distance between this <see cref="Rectangle"/> and <paramref name="rect"/> along y-axis.
        /// </returns>
        public int DistanceToY(Rectangle rect)
        {
            int distance = this.y - (rect.y + rect.h);
            if (distance < 0)
            {
                distance = rect.y - (this.y + this.h);
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Expands or shrinks the rectangle by using the specified width and height amounts, in all directions.
        /// </summary>
        /// <param name="dx">The amount by which to expand or shrink the left and right sides of the rectangle.</param>
        /// <param name="dy">The amount by which to expand or shrink the top and bottom sides of the rectangle.</param>
        /// <remarks>
        /// <para>
        /// The <see cref="Width"/> of the resulting rectangle is increased or decreased by twice the specified width offset,
        /// because it is applied to both the left and right sides of the rectangle.
        /// Likewise, the <see cref="Height"/> of the resulting rectangle is increased or decreased by twice the specified height.
        /// </para>
        /// <para>
        /// If either <paramref name="dx"/> or <paramref name="dy"/> is negative, the <see cref="Rectangle"/> structure is deflated in the corresponding direction.
        /// </para>
        /// <para>
        /// If the specified width or height shrink the rectangle by more than its current <see cref="Width"/> or <see cref="Height"/>
        /// giving the rectangle a negative area, the rectangle becomes the <see cref="Rectangle.Empty"/> rectangle.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inflate(int dx, int dy)
        {
            this.x -= dx;
            this.w = MinMax.Max(this.w + (2 * dx), 0);

            this.y -= dy;
            this.h = MinMax.Max(this.h + (2 * dy), 0);
        }

        /// <summary>
        /// Expands or shrinks the rectangle by the specified dimensions, in all directions.
        /// </summary>
        /// <param name="size">The amount by which to expand or shrink the left, right and top, bottom sides of the rectangle.</param>
        /// <remarks>
        /// <para>
        /// The <see cref="Width"/> of the rectangle is increased by twice the specified horizontal dimension,
        /// because it is applied to both the left and right sides of the rectangle.
        /// Likewise, the <see cref="Height"/> of the resulting rectangle is increased by twice the vertical dimension.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inflate(Size size) => this.Inflate(size.Width, size.Height);

        /// <summary>
        /// Replaces this <see cref="Rectangle"/> with the intersection of itself and the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The rectangle with which to intersect.</param>
        public void Intersect(Rectangle rect)
        {
            Rectangle result = Rectangle.Intersect(rect, this);

            this.x = result.x;
            this.y = result.y;
            this.w = result.w;
            this.h = result.h;
        }

        /// <summary>
        /// Determines if this rectangle <see cref="Rectangle"/> intersects with the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
        /// <returns>
        /// <b>true</b> if two rectangles intersect, otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(Rectangle rect) =>
            rect.x < this.x + this.w &&
            this.x < rect.x + rect.w &&
            rect.y < this.y + this.h &&
            this.y < rect.y + rect.h;

        /// <summary>
        /// Determines if this rectangle <see cref="Rectangle"/> intersects with the specified <see cref="Rectangle"/> along its x-axis.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
        /// <returns>
        /// <b>true</b> if two rectangles intersect along x-axis, otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWithX(Rectangle rect) =>
            rect.x < this.x + this.w &&
            this.x < rect.x + rect.w;

        /// <summary>
        /// Determines if this rectangle <see cref="Rectangle"/> intersects with the specified <see cref="Rectangle"/> along its y-axis.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
        /// <returns>
        /// <b>true</b> if two rectangles intersect along y-axis, otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWithY(Rectangle rect) =>
            rect.y < this.y + this.h &&
            this.y < rect.y + rect.h;

        /// <summary>
        /// Translates this <see cref="Rectangle"/> by the specified amount.
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
        /// Translates this <see cref="Rectangle"/> by the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> that contains the offset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(Point point) => this.Offset(point.X, point.Y);

        /// <summary>
        /// Set the rectangle x-coordinate, y-coordinate, width, and height to the specified values.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.w = width >= 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Core.Properties.Resources.E_InvalidRectangleWidth);
            this.h = height >= 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Core.Properties.Resources.E_InvalidRectangleHeight);
        }

        /// <summary>
        /// Replaces this <see cref="Rectangle"/> with the union of itself and the specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The rectangle with which to union.</param>
        public void Union(Rectangle rect)
        {
            Rectangle result = Rectangle.Union(rect, this);

            this.x = result.x;
            this.y = result.y;
            this.w = result.w;
            this.h = result.h;
        }

        /// <summary>
        /// Replaces this <see cref="Rectangle"/> with the union of itself and
        /// a specified rectangular area represented by its x-coordinate, y-coordinate, width, and height.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner of the rectangular area with which to union.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the rectangular area with which to union.</param>
        /// <param name="width">The width of the rectangular area with which to union.</param>
        /// <param name="height">The height of the rectangular area with which to union.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        public void Union(int x, int y, int width, int height)
        {
            Rectangle result = Rectangle.Union(this, x, y, width, height);

            this.x = result.x;
            this.y = result.y;
            this.w = result.w;
            this.h = result.h;
        }

        /// <summary>
        /// Scales the location and the dimensions of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="dx">The amount by which to scale the left position and the width of the rectangle.</param>
        /// <param name="dy">The amount by which to scale the top position and the height of the rectangle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(int dx, int dy)
        {
            this.x *= dx;
            this.y *= dy;
            this.w *= dx;
            this.h *= dy;
        }

        /// <summary>
        /// Scales the location and the dimensions of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="dx">The amount by which to scale the left position and the width of the rectangle.</param>
        /// <param name="dy">The amount by which to scale the top position and the height of the rectangle.</param>
        public void Scale(float dx, float dy)
        {
            float x1 = dx * this.x;
            float y1 = dy * this.y;
            float x2 = dx * (this.x + this.w);
            float y2 = dy * (this.y + this.h);

            // note: add epsilon to avoid rounding problems
            this.x = (int)Math.Round(x1 + Rectangle.Eps, MidpointRounding.AwayFromZero);
            this.y = (int)Math.Round(y1 + Rectangle.Eps, MidpointRounding.AwayFromZero);
            this.w = (int)Math.Floor(x2 - x1 + Rectangle.Eps);
            this.h = (int)Math.Floor(y2 - y1 + Rectangle.Eps);
        }

        /// <summary>
        /// Applies affine transformation described by the specified matrix to the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        public void Transform(System.Windows.Media.Matrix matrix)
        {
            // convert three corner points out of four
            (double x, double y) tr = TransformPoint(this.x + this.w, this.y);
            (double x, double y) br = TransformPoint(this.x + this.w, this.y + this.h);
            (double x, double y) bl = TransformPoint(this.x, this.y + this.h);

            // find boundaries of new rectangle
            double x1 = MinMax.Min(bl.x, tr.x, br.x);
            double x2 = MinMax.Max(bl.x, tr.x, br.x);
            double y1 = MinMax.Min(bl.y, tr.y, br.y);
            double y2 = MinMax.Max(bl.y, tr.y, br.y);

            // note: add epsilon to avoid rounding problems
            this.x = (int)Math.Round(x1 + Rectangle.Eps, MidpointRounding.AwayFromZero);
            this.y = (int)Math.Round(y1 + Rectangle.Eps, MidpointRounding.AwayFromZero);
            this.w = (int)Math.Floor(x2 - x1 + Rectangle.Eps);
            this.h = (int)Math.Floor(y2 - y1 + Rectangle.Eps);

            (double x, double y) TransformPoint(int x, int y)
            {
                return ((matrix.M11 * x) + (matrix.M12 * y) + matrix.OffsetX, (matrix.M21 * x) + (matrix.M22 * y) + matrix.OffsetY);
            }
        }
    }
}
