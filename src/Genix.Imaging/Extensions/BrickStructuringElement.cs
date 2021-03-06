﻿// -----------------------------------------------------------------------
// <copyright file="BrickStructuringElement.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using Genix.Geometry;

    /// <summary>
    /// Represents a rectangular structuring element.
    /// </summary>
    internal class BrickStructuringElement : StructuringElement
    {
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is not a positive value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is not a positive value.</para>
        /// </exception>
        public BrickStructuringElement(int width, int height)
        {
            this.width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Properties.Resources.E_InvalidSEWidth);
            this.height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Properties.Resources.E_InvalidSEHeight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickStructuringElement"/> class.
        /// </summary>
        /// <param name="width">The width of the structuring element.</param>
        /// <param name="height">The height of the structuring element.</param>
        /// <param name="anchor">The anchor position within the element. The default value  (-1, -1) means that the anchor is at the center.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is not a positive value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is not a positive value.</para>
        /// </exception>
        public BrickStructuringElement(int width, int height, Point anchor)
            : base(anchor)
        {
            this.width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Properties.Resources.E_InvalidSEWidth);
            this.height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Properties.Resources.E_InvalidSEHeight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickStructuringElement"/> class.
        /// </summary>
        private BrickStructuringElement()
        {
        }

        /// <summary>
        /// Gets the structuring element width.
        /// </summary>
        /// <value>
        /// The structuring element width, in pixels.
        /// </value>
        public int Width => this.width;

        /// <summary>
        /// Gets the structuring element height.
        /// </summary>
        /// <value>
        /// The structuring element height, in pixels.
        /// </value>
        public int Height => this.height;

        /// <inheritdoc />
        public override Size Size => new Size(this.width, this.height);

        /// <inheritdoc />
        public override IEnumerable<Point> GetElements(Point anchor)
        {
            anchor = this.GetAnchor(anchor);

            for (int ix = 0; ix < this.width; ix++)
            {
                for (int iy = 0; iy < this.height; iy++)
                {
                    yield return new Point(ix - anchor.X, iy - anchor.Y);
                }
            }
        }

        /// <inheritdoc />
        public override StructuringElement Mirror()
        {
            Point anchor = this.GetAnchor(StructuringElement.DefaultAnchor);
            if ((this.width & 1) != 0 && anchor.X == this.width / 2 && (this.height & 1) != 0 && anchor.Y == this.height / 2)
            {
                return new BrickStructuringElement(this.width, this.height, this.Anchor);
            }
            else
            {
                return new BrickStructuringElement(
                    this.width,
                    this.height,
                    new Point(this.width - anchor.X - 1, this.height - anchor.Y - 1));
            }
        }
    }
}
