// -----------------------------------------------------------------------
// <copyright file="StructuringElement.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Represents a structuring element of the specified size and shape for morphological operations.
    /// </summary>
    public abstract class StructuringElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructuringElement"/> class.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        protected StructuringElement(Point anchor)
        {
            this.Anchor = anchor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuringElement"/> class.
        /// </summary>
        protected StructuringElement()
        {
            this.Anchor = new Point(-1, -1);
        }

        /// <summary>
        /// Gets the anchor position within the element.
        /// </summary>
        /// <value>
        /// The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.
        /// </value>
        public Point Anchor { get; }

        /// <summary>
        /// Creates a square structuring element.
        /// </summary>
        /// <param name="size">The size of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
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
        public static StructuringElement Rectangle(int width, int height, Point anchor)
        {
            return new RectangleStructuringElement(width, height, anchor);
        }

        /// <summary>
        /// Creates a cross structuring element.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <returns>
        /// The <see cref="StructuringElement"/> object this method creates.
        /// </returns>
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
        public IEnumerable<Point> GetElements()
        {
            return this.GetElements(new Point(-1, -1));
        }

        /// <summary>
        /// Enumerates elements of the structuring element.
        /// </summary>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <returns>
        /// The collection of elements.
        /// </returns>
        public abstract IEnumerable<Point> GetElements(Point anchor);

        /// <summary>
        /// Represents a rectangular structuring element.
        /// </summary>
        private class RectangleStructuringElement : StructuringElement
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
            public RectangleStructuringElement(int width, int height, Point anchor) : base(anchor)
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
                    for (int iy = 0; iy < this.height; iy++)
                    {
                        if (ix != anchor.X || iy != anchor.Y)
                        {
                            yield return new Point(ix - anchor.X, iy - anchor.Y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Represents a cross structuring element.
        /// </summary>
        private class CrossStructuringElement : StructuringElement
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
            public CrossStructuringElement(int width, int height, Point anchor) : base(anchor)
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
}
