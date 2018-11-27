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
    using Genix.Geometry;

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterRectangular(
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
                            borderValue);
                    }
                }
            });

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterBox(
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
                            borderValue);
                    }
                }
            });

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterGaussian(
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
                            borderValue);
                    }
                }
            });

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterLaplace(
                            this.BitsPerPixel,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies Sobel filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <param name="normType">The normalization type.</param>
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
        /// The method applies the Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses kernels with the following values:</para>
        /// <para>      1  0  -1          1  2  1</para>
        /// <para> Gx=  2  0  -2     Gy=  0  0  0</para>
        /// <para>      1  0  -1         -1 -2 -1</para>
        /// <para>The 5x5 filter uses kernels with the following values:</para>
        /// <para>      1   2   0   -2  -1           1  4   6  4  1</para>
        /// <para>      4   8   0   -8  -4           2  8  12  8  2</para>
        /// <para> Gx=  6  12   0  -12  -6      Gy=  0  0   0  0  0</para>
        /// <para>      4   8   0   -8  -4          -2 -8 -12 -8 -2</para>
        /// <para>      1   2   0   -2  -1          -1 -4  -6 -4 -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobel(Image dst, int maskSize, NormalizationType normType, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobel(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            normType,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

 #pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies horizontal Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the horizontal Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances horizontal edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>     1  2  1</para>
        /// <para>     0  0  0</para>
        /// <para>    -1 -2 -1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>     1  4   6  4  1</para>
        /// <para>     2  8  12  8  2</para>
        /// <para>     0  0   0  0  0</para>
        /// <para>    -2 -8 -12 -8 -2</para>
        /// <para>    -1 -4  -6 -4 -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelHoriz(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelHoriz(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies horizontal (second derivative) Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the horizontal (second derivative) Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances horizontal edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>     1   2   1</para>
        /// <para>    -2  -4  -2</para>
        /// <para>     1   2   1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>     1   4    6   4   1</para>
        /// <para>     0   0    0   0   0</para>
        /// <para>    -2  -8  -12  -8  -2</para>
        /// <para>     0   0    0   0   0</para>
        /// <para>     1   4    6   4   1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelHorizSecond(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelHorizSecond(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies vertical Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the vertical Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances vertical edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    -1  0  1</para>
        /// <para>    -2  0  2</para>
        /// <para>    -1  0  1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    -1   -2   0   2   1</para>
        /// <para>    -4   -8   0   8   4</para>
        /// <para>    -6  -12   0  12   6</para>
        /// <para>    -4   -8   0   8   4</para>
        /// <para>    -1   -2   0   2   1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelVert(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelVert(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies vertical Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the vertical Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances vertical edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>This method uses the kernels which coefficients that are the same in magnitude as in method <see cref="FilterSobelVert"/> but opposite in sign.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>     1  0  -1</para>
        /// <para>     2  0  -2</para>
        /// <para>     1  0  -1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>     1   2   0   -2  -1</para>
        /// <para>     4   8   0   -8  -4</para>
        /// <para>     6  12   0  -12  -6</para>
        /// <para>     4   8   0   -8  -4</para>
        /// <para>     1   2   0   -2  -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelNegVert(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelNegVert(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies vertical (second derivative) Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the vertical (second derivative) Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>This filter enhances vertical edges of an image.</para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    1  -2  1</para>
        /// <para>    2  -4  2</para>
        /// <para>    1  -2  1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    1   0   -2   0   1</para>
        /// <para>    4   0   -8   0   4</para>
        /// <para>    6   0  -12   0   6</para>
        /// <para>    4   0   -8   0   4</para>
        /// <para>    1   0   -2   0   1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelVertSecond(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelVertSecond(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies second derivative cross Sobel filter to the <see cref="Image"/>.
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
        /// The method applies the second derivative cross Sobel filter to the <see cref="Image"/>.
        /// The kernel of the filter is a matrix of 3x3 or 5x5 size depending on the <paramref name="maskSize"/> value.
        /// The values of border pixels are assigned in accordance with the <paramref name="borderType"/> and <see cref="BorderType.BorderConst"/> parameters.
        /// </para>
        /// <para>The resulting image has depth of 16bpp and contains signed 16-bit integers.</para>
        /// <para>The 3x3 filter uses the kernel with the following values:</para>
        /// <para>    -1   0   1</para>
        /// <para>     0   0   0</para>
        /// <para>     1   0  -1</para>
        /// <para>The 5x5 filter uses the kernel with the following values:</para>
        /// <para>    -1  -2   0   2   1</para>
        /// <para>    -2  -4   0   4   2</para>
        /// <para>     0   0   0   0   0</para>
        /// <para>     2   4   0  -4  -2</para>
        /// <para>     1   2   0  -2  -1</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterSobelCross(Image dst, int maskSize, BorderType borderType, uint borderValue)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterSobelCross(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (short*)bitsdst,
                            dst.Stride8 / 2,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Filters an image using the Wiener algorithm.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="maskSize">The size of the mask, in pixels.</param>
        /// <param name="anchor">The anchor cell specifying the mask alignment with respect to the position of the input pixel.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image FilterWiener(Image dst, Size maskSize, Point anchor)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterWiener(
                            this.BitsPerPixel,
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            maskSize.Width,
                            maskSize.Height,
                            anchor.X,
                            anchor.Y,
                            new float[3] { 0.5f, 0.5f, 0.5f });
                    }
                }
            });

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterHipass(
                            this.BitsPerPixel,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

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

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.filterLowpass(
                            this.BitsPerPixel,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            maskSize,
                            borderType,
                            borderValue);
                    }
                }
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Performs deconvolution of this <see cref="Image"/> using FFT (Fast Fourie Transform).
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="kernelSize">The width and height of the convolution kernel.</param>
        /// <param name="kernel">The convolution kernel. The length must be <paramref name="kernelSize"/> * <paramref name="kernelSize"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="kernelSize"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="kernelSize"/> is less than 3.</para>
        /// <para>-or-</para>
        /// <para><paramref name="kernel"/> length is not <paramref name="kernelSize"/> * <paramref name="kernelSize"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image DeconvolutionFFT(Image dst, int kernelSize, float[] kernel)
        {
            if (kernelSize < 3)
            {
                throw new ArgumentException("The convolution kernel size must be at least 3.");
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (kernel.Length != kernelSize * kernelSize)
            {
                throw new ArgumentException("The kernel length must be kernelSize * kernelSize.");
            }

            // convert to float image
            ImageF srcf = null;
            switch (this.BitsPerPixel)
            {
                case 8:
                    srcf = this.Convert8To32f();
                    break;

                case 24:
                    srcf = this.Convert24To32f();
                    break;

                case 32:
                    srcf = this.Convert32To32f(false);
                    break;

                default:
                    throw new ArgumentException(Properties.Resources.E_UnsupportedDepth);
            }

            // explanation of IPP deconvolution parameters is here:
            // https://software.intel.com/en-us/forums/intel-integrated-performance-primitives/topic/304247

            // perform deconvolution
            ImageF dstf = new ImageF(srcf);
            IPP.Execute(() =>
            {
                return NativeMethods.deconv_FFT(
                    0,
                    0,
                    srcf.Width,
                    srcf.Height,
                    srcf.Bits,
                    srcf.Stride,
                    dstf.Bits,
                    dstf.Stride,
                    this.BitsPerPixel / 8,
                    kernelSize,
                    kernel,
                    (int)Math.Ceiling(Math.Log(srcf.Width + kernelSize - 1, 2))); // 2^FFT order >= (roi.width + kernelSize - 1)
            });

            // convert back to original format
            switch (this.BitsPerPixel)
            {
                case 8:
                    return dstf.ConvertTo8(dst, MidpointRounding.AwayFromZero);

                case 24:
                    return dstf.ConvertTo24(dst, MidpointRounding.AwayFromZero);

                case 32:
                    return dstf.ConvertTo32(dst, false, MidpointRounding.AwayFromZero);

                default:
                    throw new ArgumentException(Properties.Resources.E_UnsupportedDepth);
            }
        }

        /// <summary>
        /// Performs deconvolution of this <see cref="Image"/> using Lucy-Richardson algorithm.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="kernelSize">The width and height of the convolution kernel.</param>
        /// <param name="kernel">The convolution kernel. The length must be <paramref name="kernelSize"/> * <paramref name="kernelSize"/>.</param>
        /// <param name="numberOfIterations">The number of iterations.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="kernelSize"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="kernelSize"/> is less than 3.</para>
        /// <para>-or-</para>
        /// <para><paramref name="kernel"/> length is not <paramref name="kernelSize"/> * <paramref name="kernelSize"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image DeconvolutionLR(Image dst, int kernelSize, float[] kernel, int numberOfIterations)
        {
            if (kernelSize < 3)
            {
                throw new ArgumentException("The convolution kernel size must be at least 3.");
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (kernel.Length != kernelSize * kernelSize)
            {
                throw new ArgumentException("The kernel length must be kernelSize * kernelSize.");
            }

            // convert to float image
            ImageF srcf = null;
            switch (this.BitsPerPixel)
            {
                case 8:
                    srcf = this.Convert8To32f();
                    break;

                case 24:
                    srcf = this.Convert24To32f();
                    break;

                case 32:
                    srcf = this.Convert32To32f(false);
                    break;

                default:
                    throw new ArgumentException(Properties.Resources.E_UnsupportedDepth);
            }

            // explanation of IPP deconvolution parameters is here:
            // https://software.intel.com/en-us/forums/intel-integrated-performance-primitives/topic/304247

            // perform deconvolution
            ImageF dstf = new ImageF(srcf);
            IPP.Execute(() =>
            {
                return NativeMethods.deconv_LR(
                    0,
                    0,
                    srcf.Width,
                    srcf.Height,
                    srcf.Bits,
                    srcf.Stride,
                    dstf.Bits,
                    dstf.Stride,
                    this.BitsPerPixel / 8,
                    kernelSize,
                    kernel,
                    numberOfIterations);
            });

            // convert back to original format
            switch (this.BitsPerPixel)
            {
                case 8:
                    return dstf.ConvertTo8(dst, MidpointRounding.AwayFromZero);

                case 24:
                    return dstf.ConvertTo24(dst, MidpointRounding.AwayFromZero);

                case 32:
                    return dstf.ConvertTo32(dst, false, MidpointRounding.AwayFromZero);

                default:
                    throw new ArgumentException(Properties.Resources.E_UnsupportedDepth);
            }
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
                int maskHeight,
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
            public static unsafe extern int filterSobel(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                NormalizationType normType,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelHoriz(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelHorizSecond(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelVert(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelNegVert(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelVertSecond(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterSobelCross(
                int width,
                int height,
                byte* src,
                int stridesrc,
                short* dst,
                int stridedst,
                int maskSize,
                BorderType borderType,
                uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int filterWiener(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int maskWidth,
                int maskHeight,
                int anchorx,
                int anchory,
                [In] float[] noise);

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

            [DllImport(NativeMethods.DllName)]
            public static extern int deconv_FFT(
                int x,
                int y,
                int width,
                int height,
                [In] float[] src,
                int stridesrc,
                [Out] float[] dst,
                int stridedst,
                int channels,
                int kernelSize,
                [In] float[] kernel,
                int FFTorder);

            [DllImport(NativeMethods.DllName)]
            public static extern int deconv_LR(
                int x,
                int y,
                int width,
                int height,
                [In] float[] src,
                int stridesrc,
                [Out] float[] dst,
                int stridedst,
                int channels,
                int kernelSize,
                [In] float[] kernel,
                int numIter);
        }
    }
}
