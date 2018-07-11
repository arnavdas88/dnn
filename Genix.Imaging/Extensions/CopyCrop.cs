// -----------------------------------------------------------------------
// <copyright file="CopyCrop.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Accord.DNN;
    using Genix.Core;

    /// <summary>
    /// Provides copy extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class CopyCrop
    {
        /// <summary>
        /// Copies the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to copy.</param>
        /// <returns>
        /// A new <see cref="Image"/> that is a copy of this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Copy(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution);

            SetCopy.Copy(image.Bits.Length, image.Bits, 0, dst.Bits, 0);

            return dst;
        }

        /// <summary>
        /// Copies a rectangular area specified by a pair of coordinates, a width, and a height from the specified <see cref="Image"/> to the current <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to copy to.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width, in pixels, of the destination rectangle.</param>
        /// <param name="height">The height, in pixels, of the destination rectangle.</param>
        /// <param name="source">The <see cref="Image"/> to copy from.</param>
        /// <param name="srcx">The x-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <param name="srcy">The y-coordinate, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><c>image</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>source</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyIP(this Image image, int x, int y, int width, int height, Image source, int srcx, int srcy)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            image.ValidateArea(x, y, width, height);
            source.ValidateArea(srcx, srcy, width, height);

            if (image.BitsPerPixel != source.BitsPerPixel)
            {
                throw new ArgumentException("The image depth in source and destination images must match.");
            }

            CopyCrop.CopyArea(source, srcx, srcy, width, height, image, x, y);
        }

        /// <summary>
        /// Copies a rectangular area specified by a pair of coordinates, a width, and a height from the specified <see cref="Image"/> to the current <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to copy to.</param>
        /// <param name="area">The coordinates, in pixels, of the destination rectangle.</param>
        /// <param name="source">The <see cref="Image"/> to copy from.</param>
        /// <param name="origin">The coordinates, in pixels, of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><c>image</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>source</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyIP(this Image image, System.Drawing.Rectangle area, Image source, System.Drawing.Point origin)
        {
            image.CopyIP(area.X, area.Y, area.Width, area.Height, source, origin.X, origin.Y);
        }

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to crop.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Crop(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Image dst = new Image(
                width,
                height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution);

            CopyCrop.CopyArea(image, x, y, width, height, dst, 0, 0);

            return dst;
        }

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle specified by a <see cref="System.Drawing.Rectangle"/> struct.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to crop.</param>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><see cref="System.Drawing.Rectangle.Width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><see cref="System.Drawing.Rectangle.Height"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Crop(this Image image, System.Drawing.Rectangle rect)
        {
            return image.Crop(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Crops the <see cref="Image"/> using rectangle calculated by <see cref="ImageExtensions.BlackArea"/> method.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to modify.</param>
        /// <param name="dx">The amount by which to expand or shrink the left and right sides of the image black area.</param>
        /// <param name="dy">The amount by which to expand or shrink the top and bottom sides of the image black area.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image CropBlackArea(this Image image, int dx, int dy)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // determine black area of the image
            System.Drawing.Rectangle blackArea = image.BlackArea();

            if (dx == 0 && dy == 0)
            {
                // no frame - simply crop the black area
                return image.Crop(blackArea);
            }

            // expand target area
            System.Drawing.Rectangle bounds = System.Drawing.Rectangle.Inflate(blackArea, dx, dy);

            Image dst = new Image(
                bounds.Width,
                bounds.Height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (!blackArea.IsEmpty)
            {
                System.Drawing.Rectangle area = System.Drawing.Rectangle.Intersect(bounds, blackArea);
                int dstx = area.X - bounds.X;
                int dsty = area.Y - bounds.Y;
                CopyCrop.CopyArea(image, area.X, area.Y, area.Width, area.Height, dst, dstx, dsty);

                if (image.BitsPerPixel > 1)
                {
                    // set frame to white
                    dst.SetWhiteBorderIP(dstx, dsty, area.Width, area.Height);
                }
            }
            else
            {
                if (image.BitsPerPixel > 1)
                {
                    // set all image to white
                    SetCopy.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
                }
            }

            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CopyArea(Image src, int x, int y, int width, int height, Image dst, int dstx, int dsty)
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
                SetCopy.Copy(height * src.Stride, bitssrc, y * src.Stride, bitsdst, dsty * src.Stride);
            }
            else
            {
                for (int i = 0; i < height; i++, offsrc += stride1src, offdst += stride1dst)
                {
                    BitUtils64.CopyBits(count, bitssrc, offsrc, bitsdst, offdst);
                }
            }
        }
    }
}
