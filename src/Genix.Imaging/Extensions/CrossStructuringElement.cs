// -----------------------------------------------------------------------
// <copyright file="CrossStructuringElement.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Collections.Generic;
    using Genix.Drawing;

    /// <summary>
    /// Represents a cross structuring element.
    /// </summary>
    internal class CrossStructuringElement : StructuringElement
    {
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        public CrossStructuringElement(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        public CrossStructuringElement(int width, int height, Point anchor)
            : base(anchor)
        {
            this.width = width;
            this.height = height;
        }

        /// <inheritdoc />
        public override IEnumerable<Point> GetElements(Point anchor)
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

            for (int ix = 0; ix < this.width; ix++)
            {
                if (ix != anchor.X)
                {
                    yield return new Point(ix - anchor.X, 0);
                }
            }

            for (int iy = 0; iy < this.height; iy++)
            {
                if (iy != anchor.Y)
                {
                    yield return new Point(0, iy - anchor.Y);
                }
            }
        }
    }
}
