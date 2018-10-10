// -----------------------------------------------------------------------
// <copyright file="CopyCrop.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <content>
    /// Provides copy extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Creates of blank template with the dimensions of the this <see cref="Image"/> and specified bit depth.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="width">The destination <see cref="Image"/> width, in pixels.</param>
        /// <param name="height">The destination <see cref="Image"/> height, in pixels.</param>
        /// <param name="bitsPerPixel">The destination <see cref="Image"/> color depth, in number of bits per pixel.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with specified dimensions.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the specified dimensions.</para>
        /// </remarks>
        public Image CreateTemplate(Image dst, int width, int height, int bitsPerPixel)
        {
            if (dst == null || dst == this)
            {
                return new Image(width, height, bitsPerPixel, this);
            }
            else
            {
                // reallocate destination
                dst.AllocateBits(width, height, bitsPerPixel);
                dst.SetResolution(this.HorizontalResolution, this.VerticalResolution);
                dst.Transform = this.Transform;
                return dst;
            }
        }

        /// <summary>
        /// Creates of blank template with the dimensions of the this <see cref="Image"/> and specified bit depth.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="bitsPerPixel">The destination <see cref="Image"/> color depth, in number of bits per pixel.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image CreateTemplate(Image dst, int bitsPerPixel) => this.CreateTemplate(dst, this.Width, this.Height, bitsPerPixel);

        /// <summary>
        /// Copies the data from this <see cref="Image"/> to destination <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="copyBits">The value indicating whether the <see cref="Image{T}.Bits"/> should be copied to the new <see cref="Image"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the method returns this <see cref="Image"/>.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Copy(Image dst, bool copyBits)
        {
            if (dst == this)
            {
                return this;
            }
            else
            {
                // reallocate destination image
                dst = this.CreateTemplate(dst, this.BitsPerPixel);

                // copy bits
                if (copyBits)
                {
                    Vectors.Copy(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
                }

                return dst;
            }
        }

        /// <summary>
        /// Copies a rectangular area specified by
        /// a pair of coordinates, a width, and a height from the specified <see cref="Image"/>
        /// to the current <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width, in pixels, of the destination rectangle.</param>
        /// <param name="height">The height, in pixels, of the destination rectangle.</param>
        /// <param name="src">The <see cref="Image"/> to copy from.</param>
        /// <param name="xsrc">The x-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void CopyFrom(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            this.ValidateArea(x, y, width, height);
            src.ValidateArea(xsrc, ysrc, width, height);

            if (this.BitsPerPixel != src.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            Image.CopyArea(this, x, y, width, height, src, xsrc, ysrc);
        }

        /// <summary>
        /// Copies a rectangular area specified by
        /// by a <see cref="Rectangle"/> struct from the specified <see cref="Image"/>
        /// to the current <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="area">The coordinates, in pixels, of the destination rectangle.</param>
        /// <param name="src">The <see cref="Image"/> to copy from.</param>
        /// <param name="origin">The coordinates, in pixels, of the upper-left corner of the source rectangle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(Rectangle area, Image src, Point origin) =>
            this.CopyFrom(area.X, area.Y, area.Width, area.Height, src, origin.X, origin.Y);

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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        public Image Crop(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Image dst = new Image(width, height, this);
            Image.CopyArea(dst, 0, 0, width, height, this, x, y);

            dst.AppendTransform(new MatrixTransform(-x, -y));
            return dst;
        }

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.</para>
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

            Image dst = new Image(bounds.Size, this);

            if (!blackArea.IsEmpty)
            {
                Rectangle srcarea = Rectangle.Intersect(bounds, blackArea);
                Rectangle dstarea = Rectangle.Offset(srcarea, -bounds.X, -bounds.Y);
                Image.CopyArea(dst, dstarea.X, dstarea.Y, srcarea.Width, srcarea.Height, this, srcarea.X, srcarea.Y);

                if (this.BitsPerPixel > 1)
                {
                    // set frame to white
                    dst.SetWhiteBorder(dstarea);
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

            dst.AppendTransform(new MatrixTransform(-bounds.X, -bounds.Y));
            return dst;
        }

        private static void CopyArea(Image dst, int xdst, int ydst, int width, int height, Image src, int xsrc, int ysrc)
        {
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = dst.Bits;

            int stride1src = src.Stride1;
            int stride1dst = dst.Stride1;

            int offsrc = (ysrc * stride1src) + (xsrc * src.BitsPerPixel);
            int offdst = (ydst * stride1dst) + (xdst * src.BitsPerPixel);

            int count = width * src.BitsPerPixel;

            if (stride1src == stride1dst && xsrc == 0 && xdst == 0 && width == src.Width)
            {
                Vectors.Copy(height * src.Stride, bitssrc, ysrc * src.Stride, bitsdst, ydst * dst.Stride);
            }
            else
            {
                for (int i = 0; i < height; i++, offsrc += stride1src, offdst += stride1dst)
                {
                    BitUtils.CopyBits(count, bitssrc, offsrc, bitsdst, offdst);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyArea(Image dst, Rectangle dstBounds, Image src, Point srcOrigin) =>
            Image.CopyArea(dst, dstBounds.X, dstBounds.Y, dstBounds.Width, dstBounds.Height, src, srcOrigin.X, srcOrigin.Y);
    }
}
