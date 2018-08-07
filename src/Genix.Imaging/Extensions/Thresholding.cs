// -----------------------------------------------------------------------
// <copyright file="Thresholding.cs" company="Noname, Inc.">
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

    /// <content>
    /// Provides thresholding methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is smaller than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “less than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdLT(byte threshold, byte value) =>
            this.ThresholdLT(0, 0, this.Width, this.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is smaller than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “less than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        public Image ThresholdLT(int x, int y, int width, int height, byte threshold, byte value)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 8:
                    Image dst = this.Clone(false);
                    NativeMethods.threshold_lt_8bpp(x, y, width, height, this.Bits, this.Stride, dst.Bits, dst.Stride, threshold, value);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is smaller than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “less than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdLT(Rectangle area, byte threshold, byte value) =>
            this.ThresholdLT(area.X, area.Y, area.Width, area.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is greater than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “greater than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdGT(byte threshold, byte value) =>
            this.ThresholdGT(0, 0, this.Width, this.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is greater than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “greater than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        public Image ThresholdGT(int x, int y, int width, int height, byte threshold, byte value)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 8:
                    Image dst = this.Clone(false);
                    NativeMethods.threshold_gt_8bpp(x, y, width, height, this.Bits, this.Stride, dst.Bits, dst.Stride, threshold, value);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is greater than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/> using the specified level <paramref name="threshold"/>.
        /// Pixel values in this <see cref="Image"/> are compared to the <paramref name="threshold"/> value for “greater than”.
        /// </para>
        /// <para>
        /// If the result of the compare is true, the corresponding destination pixel is set to the specified <paramref name="value"/>.
        /// Otherwise, it is set to this <see cref="Image"/> value.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdGT(Rectangle area, byte threshold, byte value) =>
            this.ThresholdGT(area.X, area.Y, area.Width, area.Height, threshold, value);

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified value.
        /// </summary>
        /// <param name="thresholdLT">The lower threshold value to use for each pixel.</param>
        /// <param name="valueLT">The value to set for each pixel that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each pixel.</param>
        /// <param name="valueGT">The value to set for each pixel that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/>
        /// using the specified levels <paramref name="thresholdLT"/> and <paramref name="thresholdGT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are smaller than <paramref name="thresholdLT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueLT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are greater than <paramref name="thresholdGT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueGT"/>.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdLTGT(byte thresholdLT, byte valueLT, byte thresholdGT, byte valueGT) =>
            this.ThresholdLTGT(0, 0, this.Width, this.Height, thresholdLT, valueLT, thresholdGT, valueGT);

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified value.
        /// </summary>
        /// <param name="x">The x-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate, in pixels, of the upper-left corner of the area.</param>
        /// <param name="width">The width, in pixels, of the area.</param>
        /// <param name="height">The height, in pixels, of the area.</param>
        /// <param name="thresholdLT">The lower threshold value to use for each pixel.</param>
        /// <param name="valueLT">The value to set for each pixel that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each pixel.</param>
        /// <param name="valueGT">The value to set for each pixel that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/>
        /// using the specified levels <paramref name="thresholdLT"/> and <paramref name="thresholdGT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are smaller than <paramref name="thresholdLT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueLT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are greater than <paramref name="thresholdGT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueGT"/>.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        public Image ThresholdLTGT(int x, int y, int width, int height, byte thresholdLT, byte valueLT, byte thresholdGT, byte valueGT)
        {
            this.ValidateArea(x, y, width, height);

            switch (this.BitsPerPixel)
            {
                case 8:
                    Image dst = this.Clone(false);
                    NativeMethods.threshold_ltgt_8bpp(
                        x,
                        y,
                        width,
                        height,
                        this.Bits,
                        this.Stride,
                        dst.Bits,
                        dst.Stride,
                        thresholdLT,
                        valueLT,
                        thresholdGT,
                        valueGT);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified value.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="thresholdLT">The lower threshold value to use for each pixel.</param>
        /// <param name="valueLT">The value to set for each pixel that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each pixel.</param>
        /// <param name="valueGT">The value to set for each pixel that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method thresholds pixels in this <see cref="Image"/>
        /// using the specified levels <paramref name="thresholdLT"/> and <paramref name="thresholdGT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are smaller than <paramref name="thresholdLT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueLT"/>.
        /// </para>
        /// <para>
        /// If pixel values in this <see cref="Image"/> are greater than <paramref name="thresholdGT"/>,
        /// the corresponding destination pixels are set to <paramref name="valueGT"/>.
        /// </para>
        /// <para>
        /// This method supports gray images only and will throw an exception otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ThresholdLTGT(Rectangle area, byte thresholdLT, byte valueLT, byte thresholdGT, byte valueGT) =>
            this.ThresholdLTGT(area.X, area.Y, area.Width, area.Height, thresholdLT, valueLT, thresholdGT, valueGT);

        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long threshold_lt_8bpp(
               int x,
               int y,
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               byte threshold,
               byte value);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long threshold_gt_8bpp(
               int x,
               int y,
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               byte threshold,
               byte value);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern long threshold_ltgt_8bpp(
               int x,
               int y,
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               byte thresholdLT,
               byte valueLT,
               byte thresholdGT,
               byte valueGT);
        }
    }
}
