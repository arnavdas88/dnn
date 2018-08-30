// -----------------------------------------------------------------------
// <copyright file="Statistic.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Genix.Drawing;

    /// <content>
    /// Provides statistical methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Calculates the <see cref="Image"/> intensity.
        /// </summary>
        /// <returns>
        /// The image intensity.
        /// For black-and-white images it is the number of black pixels on the bitmap.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Power() => this.Power(0, 0, this.Width, this.Height);

        /// <summary>
        /// Calculates the <see cref="Image"/> intensity withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The image intensity within the specified area.
        /// For black-and-white images it is the number of black pixels.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        public long Power(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 1:
                    return NativeMethods.power_1bpp(x, y, width, height, this.Bits, this.Stride);

                case 8:
                    return NativeMethods.power_8bpp(x, y, width, height, this.Bits, this.Stride);

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Calculates the <see cref="Image"/> intensity withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The image intensity within the specified area.
        /// For black-and-white images it is the number of black pixels.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Power(Rectangle area) => this.Power(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Returns the area on this <see cref="Image"/> that contains black pixels.
        /// </summary>
        /// <returns>
        /// The <see cref="Rectangle"/> that describes the area of the image that contains black pixels;
        /// <b>Rectangle.Empty</b> if the image does not have black pixels.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The image is not black-and-white.
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        public Rectangle BlackArea()
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            int width = this.Width;
            int height = this.Height;
            int stride1 = this.Stride1;
            int stride = this.Stride;
            ulong[] bits = this.Bits;

            // calculate top
            int top = FindTop();
            if (top < 0)
            {
                return Rectangle.Empty;
            }

            // calculate bottom
            int bottom = FindBottom();
            if (bottom < top)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate left
            ulong endMask = this.EndMask;
            int left = FindLeft(out int leftColumn, out ulong leftMask);
            if (left == -1)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate right
            int right = FindRight();
            if (right < left)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);

            int FindTop()
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

            int FindBottom()
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

            int FindLeft(out int resultColumn, out ulong resultMask)
            {
                resultColumn = 0;
                resultMask = 0;

                for (int i = 0; i < stride; i++)
                {
                    ulong mask = ColumnBlackMask(i);
                    if (mask != 0ul)
                    {
                        resultColumn = i;
                        resultMask = mask;
                        return (i * 64) + BitUtils64.BitScanOneForward(mask);
                    }
                }

                return -1;
            }

            int FindRight()
            {
                for (int i = stride - 1; i >= 0; i--)
                {
                    ulong mask = i == leftColumn ? leftMask : ColumnBlackMask(i);
                    if (mask != 0ul)
                    {
                        return (i * 64) + BitUtils64.BitScanOneReverse(mask);
                    }
                }

                return -1;
            }

            ulong ColumnBlackMask(int column)
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
        /// Calculates a color intensity histogram of the <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image{T}.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Histogram GrayHistogram() => this.GrayHistogram(0, 0, this.Width, this.Height);

        /// <summary>
        /// Calculates a color intensity histogram of the <see cref="Image"/>
        /// withing a rectangular area specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image{T}.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        public Histogram GrayHistogram(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Histogram histogram = new Histogram(1 << this.BitsPerPixel);

            switch (this.BitsPerPixel)
            {
                case 1:
                    histogram[1] = (int)this.Power(x, y, width, height);
                    histogram[0] = (this.Width * this.Height) - histogram[1];
                    break;

                case 8:
                    NativeMethods.grayhist_8bpp(x, y, width, height, this.Bits, this.Stride, histogram.Bins);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }

            return histogram;
        }

        /// <summary>
        /// Calculates a color intensity histogram of the <see cref="Image"/>
        /// withing a rectangular area specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image{T}.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Histogram GrayHistogram(Rectangle area) => this.GrayHistogram(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis.
        /// </summary>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <see cref="Image{T}.Height"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Histogram HistogramY() => this.HistogramY(0, 0, this.Width, this.Height);

        /// <summary>
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis
        /// withing a rectangular area specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <paramref name="height"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        public Histogram HistogramY(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Histogram histogram = new Histogram(height);

            switch (this.BitsPerPixel)
            {
                case 1:
                    NativeMethods.vhist_1bpp(x, y, width, height, this.Bits, this.Stride, histogram.Bins);
                    break;

                case 8:
                    NativeMethods.vhist_8bpp(x, y, width, height, this.Bits, this.Stride, histogram.Bins);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }

            return histogram;
        }

        /// <summary>
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis
        /// withing a rectangular area specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <paramref name="area"/>.<see cref="Rectangle.Height"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Histogram HistogramY(Rectangle area) => this.HistogramY(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Computes the minimum and maximum of <see cref="Image"/> values.
        /// </summary>
        /// <param name="min">The minimum pixel value.</param>
        /// <param name="max">The maximum pixel value.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <remarks>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinMax(out byte min, out byte max) =>
            this.MinMax(0, 0, this.Width, this.Height, out min, out max);

        /// <summary>
        /// Computes the minimum and maximum of <see cref="Image"/> values withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <param name="min">The minimum pixel value.</param>
        /// <param name="max">The maximum pixel value.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </remarks>
        public void MinMax(int x, int y, int width, int height, out byte min, out byte max)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 8:
                    NativeMethods.minmax_8bpp(
                        x,
                        y,
                        width,
                        height,
                        this.Bits,
                        this.Stride,
                        out min,
                        out max);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Computes the minimum and maximum of <see cref="Image"/> values withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="min">The minimum pixel value.</param>
        /// <param name="max">The maximum pixel value.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinMax(Rectangle area, out byte min, out byte max) =>
            this.MinMax(area.X, area.Y, area.Width, area.Height, out min, out max);

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern long power_1bpp(
               int x,
               int y,
               int width,
               int height,
               [In] ulong[] bits,
               int stride);

            [DllImport(NativeMethods.DllName)]
            public static extern long power_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride);

            [DllImport(NativeMethods.DllName)]
            public static extern void grayhist_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);

            [DllImport(NativeMethods.DllName)]
            public static extern void vhist_1bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);

            [DllImport(NativeMethods.DllName)]
            public static extern void vhist_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);

            [DllImport(NativeMethods.DllName)]
            public static extern void minmax_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] out byte min,
                [Out] out byte max);
        }
    }
}
