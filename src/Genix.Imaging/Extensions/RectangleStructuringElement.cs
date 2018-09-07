// -----------------------------------------------------------------------
// <copyright file="RectangleStructuringElement.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Collections.Generic;
    using Genix.Drawing;

    /// <summary>
    /// Represents a rectangular structuring element.
    /// </summary>
    internal class RectangleStructuringElement : StructuringElement
    {
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        public RectangleStructuringElement(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        public RectangleStructuringElement(int width, int height, Point anchor)
            : base(anchor)
        {
            this.width = width;
            this.height = height;
        }

        /// <inheritdoc />
        public override IEnumerable<Point> GetElements(Point anchor)
        {
            anchor = this.GetAnchor(anchor);

            for (int ix = 0; ix < this.width; ix++)
            {
                for (int iy = 0; iy < this.height; iy++)
                {
                    if (ix != anchor.X || iy != anchor.Y)
                    {
                        yield return new Point(ix - anchor.X, iy - anchor.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates vertical elements of the structuring element.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The collection of vertical elements.
        /// </returns>
        public IEnumerable<Point> GetVerticalElements(Point anchor)
        {
            anchor = this.GetAnchor(anchor);

            for (int iy = 0; iy < this.height; iy++)
            {
                if (iy != anchor.Y)
                {
                    yield return new Point(0, iy - anchor.Y);
                }
            }
        }

        /// <summary>
        /// Enumerates horizontal elements of the structuring element.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The collection of horizontal elements.
        /// </returns>
        public IEnumerable<Point> GetHorizontalElements(Point anchor)
        {
            anchor = this.GetAnchor(anchor);

            for (int ix = 0; ix < this.width; ix++)
            {
                if (ix != anchor.X)
                {
                    yield return new Point(ix - anchor.X, 0);
                }
            }
        }

        private Point GetAnchor(Point anchor)
        {
            if (anchor.X == -1 && anchor.Y == -1)
            {
                if (this.Anchor.X == -1 && this.Anchor.Y == -1)
                {
                    anchor.X = this.width / 2;
                    anchor.Y = this.height / 2;
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
