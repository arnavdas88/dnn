// -----------------------------------------------------------------------
// <copyright file="Filters.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <content>
    /// Provides filtering methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Filters this <see cref="Image"/> using a rectangular filter.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="kernelWidth">The kernel width.</param>
        /// <param name="kernelHeight">The kernel height.</param>
        /// <param name="kernel">The kernel. The array of size <paramref name="kernelWidth"/> * <paramref name="kernelHeight"/>.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="kernelWidth"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="kernelHeight"/> is less than or equal to zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method uses a general rectangular kernel to filter an image.
        /// The kernel is a matrix of single-precision real values.
        /// For each input pixel, the kernel is placed on the image in such a way that the fixed anchor cell within the kernel coincides with the input pixel.
        /// The anchor cell is a geometric center of the kernel.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Filter(Image dst, int kernelWidth, int kernelHeight, float[] kernel, BorderType borderType, uint borderValue)
        {
            if (kernelWidth <= 0)
            {
                throw new ArgumentException("The kernel width must be positive.", nameof(kernelWidth));
            }

            if (kernelHeight <= 0)
            {
                throw new ArgumentException("The kernel height must be positive.", nameof(kernelHeight));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterRectangular(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        kernelWidth,
                        kernelHeight,
                        kernel,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Blurs this <see cref="Image"/> using a simple box filter.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskWidth">The kernel width.</param>
        /// <param name="maskHeight">The kernel height.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="maskWidth"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="maskHeight"/> is less than or equal to zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method sets each pixel in <paramref name="dst"/> as the average of all pixels in this <see cref="Image"/>
        /// in the rectangular neighborhood of size <paramref name="maskWidth"/>*<paramref name="maskHeight"/> with the anchor cell at that pixel.
        /// This has the effect of smoothing or blurring the input image.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterBox(Image dst, int maskWidth, int maskHeight, BorderType borderType, uint borderValue)
        {
            if (maskWidth <= 0)
            {
                throw new ArgumentException("The kernel width must be positive.", nameof(maskWidth));
            }

            if (maskHeight <= 0)
            {
                throw new ArgumentException("The kernel height must be positive.", nameof(maskHeight));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterBox(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        maskWidth,
                        maskHeight,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Performs Gaussian filtering of the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="kernelSize">The size of the Gaussian kernel (odd, greater or equal to 3).</param>
        /// <param name="sigma">The standard deviation of the Gaussian kernel.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The kernel size is not an odd number or it is less than three.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method applies the Gaussian filter to the <see cref="Image"/>.
        /// The kernel of the Gaussian filter is the matrix of size <paramref name="kernelSize"/>x<paramref name="kernelSize"/> with the standard deviation <paramref name="sigma"/>.
        /// The anchor cell is the center of the kernel.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterGaussian(Image dst, int kernelSize, float sigma, BorderType borderType, uint borderValue)
        {
            if (kernelSize < 3 || (kernelSize % 2) == 0)
            {
                throw new ArgumentException("The kernel size must be an odd number greater or equal to three.", nameof(kernelSize));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterGaussian(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        kernelSize,
                        sigma,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

 #pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies Laplace filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The kernel size is not 3 or 5.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method applies the Laplace filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are replicated from the edge pixels.
        /// </para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    -1 -1 -1</para>
        /// <para>    -1  8 -1</para>
        /// <para>    -1 -1 -1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    -1 -3 -4 -3 -1</para>
        /// <para>    -3  0  6  0 -3</para>
        /// <para>    -4  6 20  6 -4</para>
        /// <para>    -3  0  6  0 -3</para>
        /// <para>    -1 -3 -4 -3 -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterLaplace(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterLaplace(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        maskSize,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies high-pass filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The kernel size is not 3 or 5.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method applies the high-pass filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are replicated from the edge pixels.
        /// </para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    -1 -1 -1</para>
        /// <para>    -1  8 -1</para>
        /// <para>    -1 -1 -1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    -1 -1 -1 -1 -1</para>
        /// <para>    -1 -1 -1 -1 -1</para>
        /// <para>    -1 -1 24 -1 -1</para>
        /// <para>    -1 -1 -1 -1 -1</para>
        /// <para>    -1 -1 -1 -1 -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterHipass(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterHipass(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        maskSize,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies lowpass filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The kernel size is not 3 or 5.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method applies the lowpass filter (blur operation) to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are replicated from the edge pixels.
        /// </para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    1/9 1/9 1/9</para>
        /// <para>    1/9 1/9 1/9</para>
        /// <para>    1/9 1/9 1/9</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    1/25 1/25 1/25 1/25 1/25</para>
        /// <para>    1/25 1/25 1/25 1/25 1/25</para>
        /// <para>    1/25 1/25 1/25 1/25 1/25</para>
        /// <para>    1/25 1/25 1/25 1/25 1/25</para>
        /// <para>    1/25 1/25 1/25 1/25 1/25</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterLowpass(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods.filterLowpass(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        maskSize,
                        borderType,
                        borderValue) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterRectangular(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int kernelWidth,
                int kernelHeight,
                float[] kernel,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterBox(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int maskWidth,
                int maskeight,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterGaussian(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int kernelSize,
                float sigmaconst,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterLaplace(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterHipass(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterLowpass(
                int bitsPerPixel,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);
        }
    }
}
