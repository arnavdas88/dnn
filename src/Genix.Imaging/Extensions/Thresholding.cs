// -----------------------------------------------------------------------
// <copyright file="Thresholding.cs" company="Noname, Inc.">
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
    using Genix.Drawing;

    /// <content>
    /// Provides thresholding methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is smaller than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdLT(Image dst, uint threshold, uint value) =>
            this.ThresholdLT(dst, 0, 0, this.Width, this.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
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
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image ThresholdLT(Image dst, int x, int y, int width, int height, uint threshold, uint value)
        {
            this.ValidateArea(x, y, width, height);

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                     CultureInfo.InvariantCulture,
                     Properties.Resources.E_UnsupportedDepth,
                     this.BitsPerPixel));
            }

            dst = this.Copy(dst, false);
            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.threshold_lt_8bpp(
                            this.BitsPerPixel,
                            x,
                            y,
                            width,
                            height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            threshold,
                            value,
                            false);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are less than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is smaller than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdLT(Image dst, Rectangle area, uint threshold, uint value) =>
            this.ThresholdLT(dst, area.X, area.Y, area.Width, area.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is greater than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdGT(Image dst, uint threshold, uint value) =>
            this.ThresholdGT(dst, 0, 0, this.Width, this.Height, threshold, value);

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
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
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image ThresholdGT(Image dst, int x, int y, int width, int height, uint threshold, uint value)
        {
            this.ValidateArea(x, y, width, height);

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                     CultureInfo.InvariantCulture,
                     Properties.Resources.E_UnsupportedDepth,
                     this.BitsPerPixel));
            }

            dst = this.Copy(dst, false);
            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.threshold_gt_8bpp(
                            this.BitsPerPixel,
                            x,
                            y,
                            width,
                            height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            threshold,
                            value,
                            false);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Performs thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are greater than the threshold, are set to a specified value.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="threshold">The threshold value to use for each pixel.</param>
        /// <param name="value">The value to set for each pixel that is greater than the <paramref name="threshold"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdGT(Image dst, Rectangle area, uint threshold, uint value) =>
            this.ThresholdGT(dst, area.X, area.Y, area.Width, area.Height, threshold, value);

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/>.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified values.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="thresholdLT">The lower threshold value to use for each pixel.</param>
        /// <param name="valueLT">The value to set for each pixel that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each pixel.</param>
        /// <param name="valueGT">The value to set for each pixel that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdLTGT(Image dst, uint thresholdLT, uint valueLT, uint thresholdGT, uint valueGT) =>
            this.ThresholdLTGT(dst, 0, 0, this.Width, this.Height, thresholdLT, valueLT, thresholdGT, valueGT);

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a pair of coordinates, a width, and a height.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified values.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
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
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image ThresholdLTGT(Image dst, int x, int y, int width, int height, uint thresholdLT, uint valueLT, uint thresholdGT, uint valueGT)
        {
            this.ValidateArea(x, y, width, height);

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                     CultureInfo.InvariantCulture,
                     Properties.Resources.E_UnsupportedDepth,
                     this.BitsPerPixel));
            }

            dst = this.Copy(dst, false);
            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.threshold_ltgt_8bpp(
                            this.BitsPerPixel,
                            x,
                            y,
                            width,
                            height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            thresholdLT,
                            valueLT,
                            thresholdGT,
                            valueGT);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Performs double thresholding of pixel values on the <see cref="Image"/> withing a rectangular area
        /// specified by a <see cref="Rectangle"/> struct.
        /// Pixels that are smaller or greater than the thresholds, are set to a specified values.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="thresholdLT">The lower threshold value to use for each pixel.</param>
        /// <param name="valueLT">The value to set for each pixel that is smaller than the <paramref name="thresholdLT"/>.</param>
        /// <param name="thresholdGT">The upper threshold value to use for each pixel.</param>
        /// <param name="valueGT">The value to set for each pixel that is greater than the <paramref name="thresholdGT"/>.</param>
        /// <returns>
        /// The thresholded <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
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
        /// This method supports 8-bit gray, 24- and 32-bit color images only and will throw an exception otherwise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image ThresholdLTGT(Image dst, Rectangle area, uint thresholdLT, uint valueLT, uint thresholdGT, uint valueGT) =>
            this.ThresholdLTGT(dst, area.X, area.Y, area.Width, area.Height, thresholdLT, valueLT, thresholdGT, valueGT);

        /// <summary>
        /// Computes the value of the Otsu threshold.
        /// </summary>
        /// <returns>
        /// The computed Otsu threshold.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Otsu() => this.Otsu(0, 0, this.Width, this.Height);

        /// <summary>
        /// Computes the value of the Otsu threshold for the pixels in the specified rectangular area.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// The computed Otsu threshold.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        public byte Otsu(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            byte threshold = 0;
            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bits = this.Bits)
                    {
                        return NativeMethods.threshold_otsu(x, y, width, height, (byte*)bits, this.Stride8, out threshold);
                    }
                }
            });

            return threshold;
        }

        /// <summary>
        /// Computes the value of the Otsu threshold for the pixels in the specified rectangular area.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// The computed Otsu threshold.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Otsu(Rectangle area) => this.Otsu(area.X, area.Y, area.Width, area.Height);

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int threshold_lt_8bpp(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                uint threshold,
                uint value,
                [MarshalAs(UnmanagedType.Bool)] bool convertAlphaChannel);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int threshold_gt_8bpp(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                uint threshold,
                uint value,
                [MarshalAs(UnmanagedType.Bool)] bool convertAlphaChannel);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int threshold_ltgt_8bpp(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                uint thresholdLT,
                uint valueLT,
                uint thresholdGT,
                uint valueGT);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int threshold_otsu(
                 int x,
                 int y,
                 int width,
                 int height,
                 byte* src,
                 int stridesrc,
                 out byte threshold);
        }
    }
}
