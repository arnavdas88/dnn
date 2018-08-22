// -----------------------------------------------------------------------
// <copyright file="RectangleExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.Drawing
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides extension methods for the <see cref="Rectangle"/> class.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Indicates whether the rectangle contains the specified coordinate along its x-axis.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <param name="x">The x-coordinate to check.</param>
        /// <returns><b>true</b> if <c>x</c> is contained by the rectangle along its x-axis; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsX(this Rectangle rect, int x)
        {
            return rect.X <= x && x < rect.Right;
        }

        /// <summary>
        /// Indicates whether the rectangle contains the specified coordinate along its y-axis.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <param name="y">The x-coordinate to check.</param>
        /// <returns><b>true</b> if <c>y</c> is contained by the rectangle along its y-axis; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsY(this Rectangle rect, int y)
        {
            return rect.Y <= y && y < rect.Bottom;
        }

        /// <summary>
        /// Computes the distance between the rectangle and the specified rectangle.
        /// </summary>
        /// <param name="a">A first rectangle to compute the distance from.</param>
        /// <param name="b">A second rectangle to compute the distance to.</param>
        /// <returns>
        /// The distance between rectangles, zero if they intersect.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(this Rectangle a, Rectangle b)
        {
            int distanceX = a.DistanceX(b);
            int distanceY = a.DistanceY(b);
            return Math.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
        }

        /// <summary>
        /// Computes the distance between the rectangle and the specified rectangle along their x-axis.
        /// </summary>
        /// <param name="a">A first rectangle to compute the distance from.</param>
        /// <param name="b">A second rectangle to compute the distance to.</param>
        /// <returns>
        /// The distance between rectangles along their x-axis, zero if they intersect.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DistanceX(this Rectangle a, Rectangle b)
        {
            int distance = a.Left - b.Right;
            if (distance < 0)
            {
                distance = b.Left - a.Right;
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Computes the distance between the rectangle and the specified rectangle along their y-axis.
        /// </summary>
        /// <param name="a">A first rectangle to compute the distance from.</param>
        /// <param name="b">A second rectangle to compute the distance to.</param>
        /// <returns>
        /// The distance between rectangles along their y-axis, zero if they intersect.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DistanceY(this Rectangle a, Rectangle b)
        {
            int distance = a.Top - b.Bottom;
            if (distance < 0)
            {
                distance = b.Top - a.Bottom;
                if (distance < 0)
                {
                    distance = 0;
                }
            }

            return distance;
        }

        /// <summary>
        /// Set the rectangle x-coordinate, y-coordinate, width, and height to the specified values.
        /// </summary>
        /// <param name="rect">The rectangle to set.</param>
        /// <param name="x">The x-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <exception cref="ArgumentException">
        /// <para><c>width</c> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><c>height</c> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref Rectangle rect, int x, int y, int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(Genix.Core.Properties.Resources.E_InvalidRectangleWidth, nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(Genix.Core.Properties.Resources.E_InvalidRectangleHeight, nameof(height));
            }

            rect.X = x;
            rect.Y = y;
            rect.Width = width;
            rect.Height = height;
        }

        /// <summary>
        /// Set the rectangle to the same x-coordinate, y-coordinate, width, and height as the specified rectangle.
        /// </summary>
        /// <param name="a">The rectangle to set.</param>
        /// <param name="b">The rectangle to copy coordinates from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref Rectangle a, Rectangle b)
        {
            a.X = b.X;
            a.Y = b.Y;
            a.Width = b.Width;
            a.Height = b.Height;
        }

        /// <summary>
        /// Expands the current rectangle exactly enough to contain the specified rectangle.
        /// </summary>
        /// <param name="a">The first rectangle to union.</param>
        /// <param name="b">The second rectangle to union.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Union(this ref Rectangle a, Rectangle b)
        {
            a.Union(b.X, b.Y, b.Width, b.Height);
        }

        /// <summary>
        /// Expands the current rectangle exactly enough to contain the rectangle specified by x-coordinate, y-coordinate, width, and height.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> with which to union.</param>
        /// <param name="x">The x-coordinate of the top-left corner of the rectangle to include.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the rectangle to include.</param>
        /// <param name="width">The width of the rectangle to include.</param>
        /// <param name="height">The height of the rectangle to include.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>width</c> has a negative value.
        /// -or-
        /// <c>height</c> has a negative value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Union(this ref Rectangle rect, int x, int y, int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(Genix.Core.Properties.Resources.E_InvalidRectangleWidth, nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(Genix.Core.Properties.Resources.E_InvalidRectangleHeight, nameof(height));
            }

            if (width > 0 && height > 0)
            {
                if (rect.Width == 0 && rect.Height == 0)
                {
                    rect.Set(x, y, width, height);
                }
                else
                {
                    int x1 = Maximum.Min(rect.X, x);
                    int y1 = Maximum.Min(rect.Y, y);
                    int x2 = Maximum.Max(rect.Right, x + width);
                    int y2 = Maximum.Max(rect.Bottom, y + height);

                    rect.Set(x1, y1, x2 - x1, y2 - y1);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> structure that contains the union of the sequence of <see cref="Rectangle"/> structures.
        /// </summary>
        /// <param name="values">The rectangles to union.</param>
        /// <returns>
        /// A <see cref="Rectangle"/> structure that bounds the union of the sequence of <see cref="Rectangle"/> structures.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Union(this IEnumerable<Rectangle> values)
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
    }
}
