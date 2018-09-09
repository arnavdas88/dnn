// -----------------------------------------------------------------------
// <copyright file="CopyCrop.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <content>
    /// Provides copy extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Copies the <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> that is a copy of this instance.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Copy() => this.Clone(true);

        /// <summary>
        /// Copies a rectangular area specified by
        /// a pair of coordinates, a width, and a height from the specified <see cref="Image"/>
        /// to the current <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width, in pixels, of the destination rectangle.</param>
        /// <param name="height">The height, in pixels, of the destination rectangle.</param>
        /// <param name="source">The <see cref="Image"/> to copy from.</param>
        /// <param name="srcx">The x-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <param name="srcy">The y-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="source"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void CopyIP(int x, int y, int width, int height, Image source, int srcx, int srcy)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.ValidateArea(x, y, width, height);
            source.ValidateArea(srcx, srcy, width, height);

            if (this.BitsPerPixel != source.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            Image.CopyArea(source, srcx, srcy, width, height, this, x, y);
        }

        /// <summary>
        /// Copies a rectangular area specified by
        /// by a <see cref="Rectangle"/> struct from the specified <see cref="Image"/>
        /// to the current <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="area">The coordinates, in pixels, of the destination rectangle.</param>
        /// <param name="source">The <see cref="Image"/> to copy from.</param>
        /// <param name="origin">The coordinates, in pixels, of the upper-left corner of the source rectangle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyIP(Rectangle area, Image source, Point origin) =>
            this.CopyIP(area.X, area.Y, area.Width, area.Height, source, origin.X, origin.Y);

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        public Image Crop(int x, int y, int width, int height)
        {
            Image dst = new Image(width, height, this);
            Image.CopyArea(this, x, y, width, height, dst, 0, 0);

            dst.Transform = this.Transform.Append(new MatrixTransform(-x, -y));
            return dst;
        }

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><see cref="Rectangle.Width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><see cref="Rectangle.Height"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Crop(Rectangle area) => this.Crop(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle calculated by <see cref="Image.BlackArea"/> method.
        /// </summary>
        /// <param name="dx">The amount by which to expand or shrink the left and right sides of the image black area.</param>
        /// <param name="dy">The amount by which to expand or shrink the top and bottom sides of the image black area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        public Image CropBlackArea(int dx, int dy)
        {
            // determine black area of the image
            Rectangle blackArea = this.BlackArea();

            if (dx == 0 && dy == 0)
            {
                // no frame - simply crop the black area
                return this.Crop(blackArea);
            }

            // expand target area
            Rectangle bounds = Rectangle.Inflate(blackArea, dx, dy);

            Image dst = new Image(bounds.Width, bounds.Height, this);

            if (!blackArea.IsEmpty)
            {
                Rectangle area = Rectangle.Intersect(bounds, blackArea);
                int dstx = area.X - bounds.X;
                int dsty = area.Y - bounds.Y;
                Image.CopyArea(this, area.X, area.Y, area.Width, area.Height, dst, dstx, dsty);

                if (this.BitsPerPixel > 1)
                {
                    // set frame to white
                    dst.SetWhiteBorderIP(dstx, dsty, area.Width, area.Height);
                }
            }
            else
            {
                if (this.BitsPerPixel > 1)
                {
                    // set all image to white
                    Vectors.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
                }
            }

            dst.Transform = this.Transform.Append(new MatrixTransform(-bounds.X, -bounds.Y));
            return dst;
        }

        private static void CopyArea(Image src, int x, int y, int width, int height, Image dst, int dstx, int dsty)
        {
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = dst.Bits;

            int stride1src = src.Stride1;
            int stride1dst = dst.Stride1;

            int offsrc = (y * stride1src) + (x * src.BitsPerPixel);
            int offdst = (dsty * stride1dst) + (dstx * src.BitsPerPixel);

            int count = width * src.BitsPerPixel;

            if (stride1src == stride1dst && x == 0 && dstx == 0 && width == src.Width)
            {
                Vectors.Copy(height * src.Stride, bitssrc, y * src.Stride, bitsdst, dsty * src.Stride);
            }
            else
            {
                for (int i = 0; i < height; i++, offsrc += stride1src, offdst += stride1dst)
                {
                    BitUtils.CopyBits(count, bitssrc, offsrc, bitsdst, offdst);
                }
            }
        }
    }
}
