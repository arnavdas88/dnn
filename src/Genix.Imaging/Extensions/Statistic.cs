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
    using Genix.Geometry;

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
        [CLSCompliant(false)]
        public ulong Power() => this.Power(0, 0, this.Width, this.Height);

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
        [CLSCompliant(false)]
        public ulong Power(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 1:
                case 8:
                    return NativeMethods.power(this.BitsPerPixel, x, y, width, height, this.Bits, this.Stride);

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
        [CLSCompliant(false)]
        public ulong Power(Rectangle area) => this.Power(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only white pixels.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only white pixels; otherwise, <b>false</b>.
        /// </returns>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllWhite() => this.IsAllWhite(0, 0, this.Width, this.Height);

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only white pixels withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only white pixels withing the specified area; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </remarks>
        public bool IsAllWhite(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);
            return NativeMethods.is_all_white(this.BitsPerPixel, x, y, width, height, this.Bits, this.Stride);
        }

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only white pixels withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only white pixels withing the specified area; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllWhite(Rectangle area) => this.IsAllWhite(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only black pixels.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only black pixels; otherwise, <b>false</b>.
        /// </returns>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllBlack() => this.IsAllBlack(0, 0, this.Width, this.Height);

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only black pixels withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only black pixels withing the specified area; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        public bool IsAllBlack(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);
            return NativeMethods.is_all_black(this.BitsPerPixel, x, y, width, height, this.Bits, this.Stride);
        }

        /// <summary>
        /// Determines whether the <see cref="Image"/> contains only black pixels withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="Image"/> contains only black pixels withing the specified area; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllBlack(Rectangle area) => this.IsAllBlack(area.X, area.Y, area.Width, area.Height);

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
                    if (BitUtils.BitScanOneForward(width, bits, off) != -1)
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
                    if (BitUtils.BitScanOneForward(width, bits, off) != -1)
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
                        return (i * 64) + BitUtils.BitScanOneForward(mask);
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
                        return (i * 64) + BitUtils.BitScanOneReverse(mask);
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
        /// Computes the minimum of <see cref="Image"/> values.
        /// </summary>
        /// <returns>
        /// The minimum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint Min() => this.Min(0, 0, this.Width, this.Height);

        /// <summary>
        /// Computes the minimum of <see cref="Image"/> values withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The minimum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [CLSCompliant(false)]
        public uint Min(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            uint result = uint.MaxValue;
            unsafe
            {
                fixed (ulong* bits = &this.Bits[y * this.Stride])
                {
                    int stride8 = this.Stride8;
                    byte* ptr = (byte*)bits + (x * this.BitsPerPixel / 8);

                    switch (this.BitsPerPixel)
                    {
                        case 1:
                            result = NativeMethods.is_all_black(1, x, y, width, height, this.Bits, this.Stride) ? 1u : 0u;
                            break;

                        case 8:
                            if (width == stride8)
                            {
                                result = (uint)Vectors.Min(width * height, ptr);
                            }
                            else
                            {
                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    result = Math.Min(result, (uint)Vectors.Min(width, ptr));
                                }
                            }

                            break;

                        case 16:
                            if (width == stride8 / sizeof(ushort))
                            {
                                result = (uint)Vectors.Min(width * height, (ushort*)bits);
                            }
                            else
                            {
                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    result = Math.Min(result, (uint)Vectors.Min(width, (ushort*)bits));
                                }
                            }

                            break;

                        case 24:
                            {
                                Color mincolor = Color.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    mincolor.B = Math.Min(mincolor.B, Vectors.Min(width, ptr + 0, 3));
                                    mincolor.G = Math.Min(mincolor.G, Vectors.Min(width, ptr + 1, 3));
                                    mincolor.R = Math.Min(mincolor.R, Vectors.Min(width, ptr + 2, 3));
                                }

                                result = mincolor.Argb;
                            }

                            break;

                        case 32:
                            {
                                Color mincolor = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

                                if (width == stride8)
                                {
                                    mincolor.B = Vectors.Min(width * height, ptr + 0, 4);
                                    mincolor.G = Vectors.Min(width * height, ptr + 1, 4);
                                    mincolor.R = Vectors.Min(width * height, ptr + 2, 4);
                                    mincolor.A = Vectors.Min(width * height, ptr + 3, 4);
                                }
                                else
                                {
                                    for (int i = 0; i < height; i++, ptr += stride8)
                                    {
                                        mincolor.B = Math.Min(mincolor.B, Vectors.Min(width, ptr + 0, 4));
                                        mincolor.G = Math.Min(mincolor.G, Vectors.Min(width, ptr + 1, 4));
                                        mincolor.R = Math.Min(mincolor.R, Vectors.Min(width, ptr + 2, 4));
                                        mincolor.A = Math.Min(mincolor.A, Vectors.Min(width, ptr + 3, 4));
                                    }
                                }

                                result = mincolor.Argb;
                            }

                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the minimum of <see cref="Image"/> values withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The minimum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint Min(Rectangle area) => this.Min(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Computes the maximum of <see cref="Image"/> values.
        /// </summary>
        /// <returns>
        /// The maximum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint Max() => this.Max(0, 0, this.Width, this.Height);

        /// <summary>
        /// Computes the maximum of <see cref="Image"/> values withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The maximum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [CLSCompliant(false)]
        public uint Max(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            uint result = uint.MinValue;
            unsafe
            {
                fixed (ulong* bits = &this.Bits[y * this.Stride])
                {
                    int stride8 = this.Stride8;
                    byte* ptr = (byte*)bits + (x * this.BitsPerPixel / 8);

                    switch (this.BitsPerPixel)
                    {
                        case 1:
                            result = NativeMethods.is_all_white(1, x, y, width, height, this.Bits, this.Stride) ? 0u : 1u;
                            break;

                        case 8:
                            if (width == stride8)
                            {
                                result = (uint)Vectors.Max(width * height, ptr);
                            }
                            else
                            {
                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    result = Math.Max(result, (uint)Vectors.Max(width, ptr));
                                }
                            }

                            break;

                        case 16:
                            if (width == stride8 / sizeof(ushort))
                            {
                                result = (uint)Vectors.Max(width * height, (ushort*)bits);
                            }
                            else
                            {
                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    result = Math.Max(result, (uint)Vectors.Max(width, (ushort*)bits));
                                }
                            }

                            break;

                        case 24:
                            {
                                Color maxcolor = Color.FromArgb(0, byte.MinValue, byte.MinValue, byte.MinValue);

                                for (int i = 0; i < height; i++, ptr += stride8)
                                {
                                    maxcolor.B = Math.Max(maxcolor.B, Vectors.Max(width, ptr + 0, 3));
                                    maxcolor.G = Math.Max(maxcolor.G, Vectors.Max(width, ptr + 1, 3));
                                    maxcolor.R = Math.Max(maxcolor.R, Vectors.Max(width, ptr + 2, 3));
                                }

                                result = maxcolor.Argb;
                            }

                            break;

                        case 32:
                            {
                                Color maxcolor = Color.FromArgb(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue);

                                if (width == stride8)
                                {
                                    maxcolor.B = Vectors.Max(width * height, ptr + 0, 4);
                                    maxcolor.G = Vectors.Max(width * height, ptr + 1, 4);
                                    maxcolor.R = Vectors.Max(width * height, ptr + 2, 4);
                                    maxcolor.A = Vectors.Max(width * height, ptr + 3, 4);
                                }
                                else
                                {
                                    for (int i = 0; i < height; i++, ptr += stride8)
                                    {
                                        maxcolor.B = Math.Max(maxcolor.B, Vectors.Max(width, ptr + 0, 4));
                                        maxcolor.G = Math.Max(maxcolor.G, Vectors.Max(width, ptr + 1, 4));
                                        maxcolor.R = Math.Max(maxcolor.R, Vectors.Max(width, ptr + 2, 4));
                                        maxcolor.A = Math.Max(maxcolor.A, Vectors.Max(width, ptr + 3, 4));
                                    }
                                }

                                result = maxcolor.Argb;
                            }

                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the maximum of <see cref="Image"/> values withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The maximum pixel value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint Max(Rectangle area) => this.Max(area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Computes the minimum and maximum of <see cref="Image"/> values.
        /// </summary>
        /// <param name="min">The minimum pixel value.</param>
        /// <param name="max">The maximum pixel value.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public void MinMax(out uint min, out uint max) =>
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
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [CLSCompliant(false)]
        public void MinMax(int x, int y, int width, int height, out uint min, out uint max)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 1:
                    if (NativeMethods.is_all_black(1, x, y, width, height, this.Bits, this.Stride))
                    {
                        min = max = 1u;
                    }
                    else if (NativeMethods.is_all_white(1, x, y, width, height, this.Bits, this.Stride))
                    {
                        min = max = 0u;
                    }
                    else
                    {
                        min = 0u;
                        max = 1u;
                    }

                    break;

                case 8:
                case 16:
                    uint localmin = 0;
                    uint localmax = 0;
                    IPP.Execute(() =>
                    {
                        unsafe
                        {
                            fixed (ulong* bits = this.Bits)
                            {
                                return NativeMethods.minmax(
                                    this.BitsPerPixel,
                                    x,
                                    y,
                                    width,
                                    height,
                                    (byte*)bits,
                                    this.Stride8,
                                    out localmin,
                                    out localmax);
                            }
                        }
                    });

                    min = localmin;
                    max = localmax;
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
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 16.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public void MinMax(Rectangle area, out uint min, out uint max) =>
            this.MinMax(area.X, area.Y, area.Width, area.Height, out min, out max);

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern ulong power(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride);

            [DllImport(NativeMethods.DllName)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool is_all_white(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride);

            [DllImport(NativeMethods.DllName)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool is_all_black(
                int bitsPerPixel,
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
            public static extern unsafe int minmax(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                byte* bits,
                int stride,
                [Out] out uint min,
                [Out] out uint max);
        }
    }
}
