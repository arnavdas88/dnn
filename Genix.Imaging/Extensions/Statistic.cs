// -----------------------------------------------------------------------
// <copyright file="Statistic.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides statistical extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Statistic
    {
        /// <summary>
        /// Returns the number of black pixels on this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <returns>
        /// The number of black pixels on the bitmap.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Power(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.Power(0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Returns the number of black pixels withing specified area of this <see cref="Image"/> .
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The number of black pixels on the bitmap.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Power(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            image.ValidateArea(x, y, width, height);

            ulong[] bits = image.Bits;
            int stride = image.Stride1;

            int sum = 0;
            for (int iy = 0, pos = (y * stride) + x; iy < height; iy++, pos += stride)
            {
                sum += BitUtils64.CountOneBits(width, bits, pos);
            }

            return sum;
        }

        /// <summary>
        /// Returns the number of black pixels withing specified area of this <see cref="Image"/> .
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <param name="area">The area on <paramref name="image"/> to calculate histogram for.</param>
        /// <returns>
        /// The number of black pixels on the bitmap.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Power(this Image image, Rectangle area)
        {
            return Statistic.Power(image, area.X, area.Y, area.Width, area.Height);
        }

        /// <summary>
        /// Returns the area on this <see cref="Image"/> that contains black pixels.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <returns>
        /// The <see cref="Rectangle"/> that describes the area of the image that contains black pixels;
        /// <b>Rectangle.Empty</b> if the image does not have black pixels.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The image is not black-and-white.
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle BlackArea(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;
            ulong[] bits = image.Bits;

            // calculate top
            int top = findTop();
            if (top < 0)
            {
                return Rectangle.Empty;
            }

            // calculate bottom
            int bottom = findBottom();
            if (bottom < top)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate left
            ulong endMask = image.EndMask;
            int left = findLeft(out int leftColumn, out ulong leftMask);
            if (left == -1)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate right
            int right = findRight();
            if (right < left)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);

            int findTop()
            {
                for (int i = 0, off = 0; i < height; i++, off += stride1)
                {
                    if (BitUtils64.BitScanOneForward(width, bits, off) != -1)
                    {
                        return i;
                    }
                }

                return -1;
            }

            int findBottom()
            {
                for (int i = height - 1, off = i * stride1; i >= 0; i--, off -= stride1)
                {
                    if (BitUtils64.BitScanOneForward(width, bits, off) != -1)
                    {
                        return i;
                    }
                }

                return -1;
            }

            int findLeft(out int resultColumn, out ulong resultMask)
            {
                resultColumn = 0;
                resultMask = 0;

                for (int i = 0; i < stride; i++)
                {
                    ulong mask = columnBlackMask(i);
                    if (mask != 0ul)
                    {
                        resultColumn = i;
                        resultMask = mask;
                        return (i * 64) + BitUtils64.BitScanOneForward(mask);
                    }
                }

                return -1;
            }

            int findRight()
            {
                for (int i = stride - 1; i >= 0; i--)
                {
                    ulong mask = i == leftColumn ? leftMask : columnBlackMask(i);
                    if (mask != 0ul)
                    {
                        return (i * 64) + BitUtils64.BitScanOneReverse(mask);
                    }
                }

                return -1;
            }

            ulong columnBlackMask(int column)
            {
                ulong mask = 0;

                for (int i = top, off = (i * stride) + column; i <= bottom; i++, off += stride)
                {
                    mask |= bits[off];
                }

                if (column == stride - 1)
                {
                    mask &= endMask;
                }

                return mask;
            }
        }

        /// <summary>
        /// Calculates a horizontal histogram for the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram HistogramY(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.HistogramY(0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Calculates a horizontal histogram for the specified area of the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram HistogramY(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            image.ValidateArea(x, y, width, height);

            int stride1 = image.Stride1;
            ulong[] bits = image.Bits;

            Histogram histogram = new Histogram(height);

            for (int i = 0, pos = (y * stride1) + x; i < height; i++, pos += stride1)
            {
                histogram.Increment(i, BitUtils64.CountOneBits(width, bits, pos));
            }

            return histogram;
        }

        /// <summary>
        /// Calculates a horizontal histogram for the specified area of the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <param name="area">The area on <paramref name="image"/> to calculate histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram HistogramY(this Image image, Rectangle area)
        {
            return Statistic.HistogramY(image, area.X, area.Y, area.Width, area.Height);
        }
    }
}
