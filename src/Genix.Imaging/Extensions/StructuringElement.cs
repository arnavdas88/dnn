// -----------------------------------------------------------------------
// <copyright file="StructuringElement.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Drawing;

    /// <summary>
    /// Represents a structuring element of the specified size and shape for morphological operations.
    /// </summary>
    public abstract class StructuringElement
    {
        /// <summary>
        /// Represents a default anchor value (-1, -1) that means that the anchor is at the center.
        /// </summary>
        public static readonly Point DefaultAnchor = new Point(-1, -1);

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuringElement"/> class.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected StructuringElement(Point anchor)
        {
            this.Anchor = anchor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuringElement"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected StructuringElement()
        {
            this.Anchor = StructuringElement.DefaultAnchor;
        }

        /// <summary>
        /// Gets the anchor position within the element.
        /// </summary>
        /// <value>
        /// The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.
        /// </value>
        public Point Anchor { get; }

        /// <summary>
        /// Gets the size of this <see cref="StructuringElement"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Size"/> struct that contains structuring element dimensions.
        /// </value>
        public abstract Size Size { get; }

        /// <summary>
        /// Creates a square structuring element.
        /// </summary>
        /// <param name="size">The size of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Square(int size)
        {
            return new RectangleStructuringElement(size, size);
        }

        /// <summary>
        /// Creates a square structuring element.
        /// </summary>
        /// <param name="size">The size of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Square(int size, Point anchor)
        {
            return new RectangleStructuringElement(size, size, anchor);
        }

        /// <summary>
        /// Creates a rectangular structuring element.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Rectangle(int width, int height)
        {
            return new RectangleStructuringElement(width, height);
        }

        /// <summary>
        /// Creates a rectangular structuring element.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Rectangle(int width, int height, Point anchor)
        {
            return new RectangleStructuringElement(width, height, anchor);
        }

        /// <summary>
        /// Creates a cross structuring element.
        /// </summary>
        /// <param name="size">The size of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Cross(int size)
        {
            return new CrossStructuringElement(size, size);
        }

        /// <summary>
        /// Creates a cross structuring element.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Cross(int width, int height)
        {
            return new CrossStructuringElement(width, height);
        }

        /// <summary>
        /// Creates a cross structuring element.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructuringElement Cross(int width, int height, Point anchor)
        {
            return new CrossStructuringElement(width, height, anchor);
        }

        /// <summary>
        /// Enumerates elements of the structuring element anchored around its center.
        /// </summary>
        /// <returns>
        /// The collection of elements.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Point> GetElements() => this.GetElements(StructuringElement.DefaultAnchor);

        /// <summary>
        /// Enumerates elements of the structuring element.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The collection of elements.
        /// </returns>
        public abstract IEnumerable<Point> GetElements(Point anchor);

        /// <summary>
        /// Computes the anchor position within the element.
        /// </summary>
        /// <param name="anchor">The initial anchor position within the element.</param>
        /// <returns>
        /// The computed anchor position within the element.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="anchor"/> is not <see cref="StructuringElement.DefaultAnchor"/> the method returns <paramref name="anchor"/>.
        /// </para>
        /// <para>
        /// Then, if <see cref="Anchor"/> is not <see cref="StructuringElement.DefaultAnchor"/> the method returns <see cref="Anchor"/>.
        /// </para>
        /// <para>
        /// Conversely, the method returns the <see cref="Point"/> that corresponds the center point of the <see cref="Size"/>.
        /// </para>
        /// </remarks>
        public Point GetAnchor(Point anchor)
        {
            if (anchor == StructuringElement.DefaultAnchor)
            {
                if (this.Anchor == StructuringElement.DefaultAnchor)
                {
                    Size size = this.Size;
                    anchor.X = size.Width / 2;
                    anchor.Y = size.Height / 2;
                }
                else
                {
                    anchor = this.Anchor;
                }
            }

            return anchor;
        }
    }
}
