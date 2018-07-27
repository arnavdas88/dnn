// -----------------------------------------------------------------------
// <copyright file="Statistic.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <summary>
    /// Provides statistical extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Statistic
    {
        /// <summary>
        /// Calculates the <see cref="Image"/> intensity.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the intensity for.</param>
        /// <returns>
        /// The image intensity.
        /// For black-and-white images it is the number of black pixels on the bitmap.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Power(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.Power(0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Calculates the <see cref="Image"/> intensity withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the intensity for.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The image intensity within the specified area.
        /// For black-and-white images it is the number of black pixels.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Power(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            switch (image.BitsPerPixel)
            {
                case 1:
                    return NativeMethods.power_1bpp(x, y, width, height, image.Bits, image.Stride);

                case 8:
                    return NativeMethods.power_8bpp(x, y, width, height, image.Bits, image.Stride);

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        image.BitsPerPixel));
            }
        }

        /// <summary>
        /// Calculates the <see cref="Image"/> intensity withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the intensity for.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The image intensity within the specified area.
        /// For black-and-white images it is the number of black pixels.
        /// For gray-scale images it is the sum of all pixel values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Power(this Image image, Rectangle area)
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
        /// Calculates a color intensity histogram of the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram GrayHistogram(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return image.GrayHistogram(0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Calculates a color intensity histogram of the <see cref="Image"/>
        /// withing a rectangular area specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram GrayHistogram(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            Histogram histogram = new Histogram(1 << image.BitsPerPixel);

            switch (image.BitsPerPixel)
            {
                case 1:
                    histogram[1] = (int)image.Power(x, y, width, height);
                    histogram[0] = (image.Width * image.Height) - histogram[1];
                    break;

                case 8:
                    NativeMethods.grayhist_8bpp(x, y, width, height, image.Bits, image.Stride, histogram.Bins);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        image.BitsPerPixel));
            }

            return histogram;
        }

        /// <summary>
        /// Calculates a color intensity histogram of the <see cref="Image"/>
        /// withing a rectangular area specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is 2^<see cref="Image.BitsPerPixel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram GrayHistogram(this Image image, Rectangle area)
        {
            return Statistic.GrayHistogram(image, area.X, area.Y, area.Width, area.Height);
        }

        /// <summary>
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <paramref name="image"/>.<see cref="Image.Height"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
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
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis 
        /// withing a rectangular area specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <paramref name="height"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram HistogramY(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            Histogram histogram = new Histogram(height);

            switch (image.BitsPerPixel)
            {
                case 1:
                    NativeMethods.vhist_1bpp(x, y, width, height, image.Bits, image.Stride, histogram.Bins);
                    break;

                case 8:
                    NativeMethods.vhist_8bpp(x, y, width, height, image.Bits, image.Stride, histogram.Bins);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        image.BitsPerPixel));
            }

            return histogram;
        }

        /// <summary>
        /// Calculates an intensity histogram of the <see cref="Image"/> along its x-axis 
        /// withing a rectangular area specified by a <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to calculate the histogram for.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// The histogram size is <paramref name="area"/>.<see cref="Rectangle.Height"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <paramref name="image"/>.<see cref="Image.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// This method supports binary and gray images only and will throw an exception otherwise.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Histogram HistogramY(this Image image, Rectangle area)
        {
            return Statistic.HistogramY(image, area.X, area.Y, area.Width, area.Height);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long power_1bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long power_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void grayhist_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void vhist_1bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void vhist_8bpp(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] bits,
                int stride,
                [Out] int[] hist);
        }
    }
}
