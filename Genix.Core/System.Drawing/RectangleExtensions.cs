// -----------------------------------------------------------------------
// <copyright file="RectangleExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.Drawing
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for the <see cref="Rectangle"/> class.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Computes the distance between the rectangle and the specified rectangle.
        /// </summary>
        /// <param name="a">A first rectangle to compute the distance from.</param>
        /// <param name="b">A second rectangle to compute the distance to.</param>
        /// <returns>
        /// The distance between rectangles, zero if they intersect.
        /// </returns>
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
        /// Gets a <see cref="Rectangle"/> structure that contains the union of the sequence of <see cref="Rectangle"/> structures.
        /// </summary>
        /// <param name="values">The rectangles to union.</param>
         /// <returns>
        /// A <see cref="Rectangle"/> structure that bounds the union of the sequence of <see cref="Rectangle"/> structures.
        /// </returns>
        public static Rectangle Union(this IEnumerable<Rectangle> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Rectangle result = Rectangle.Empty;
            foreach (Rectangle value in values)
            {
                result = Rectangle.Union(result, value);
            }

            return result;
        }
    }
}
