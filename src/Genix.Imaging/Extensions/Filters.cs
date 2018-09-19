// -----------------------------------------------------------------------
// <copyright file="Filters.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <content>
    /// Provides filtering methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Performs Gaussian filtering of the <see cref="Image"/>.
        /// </summary>
        /// <param name="kernelSize">The size of the Gaussian kernel (odd, greater or equal to 3).</param>
        /// <param name="sigma">The standard deviation of the Gaussian kernel.</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8 or 24.
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
        /// </remarks>
        public Image FilterGaussian(int kernelSize, float sigma)
        {
            if (kernelSize < 3 || (kernelSize % 2) == 0)
            {
                throw new ArgumentException("The kernel size must be an odd number greater or equal to three.", nameof(kernelSize));
            }

            Image dst = this.Clone(false);

            switch (this.BitsPerPixel)
            {
                case 8:
                    if (NativeMethods.filterGaussian_8bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, kernelSize, sigma) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                case 24:
                    if (NativeMethods.filterGaussian_24bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, kernelSize, sigma) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies Laplace filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
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
        /// </remarks>
        public Image FilterLaplace(int maskSize)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            Image dst = this.Clone(false);

            switch (this.BitsPerPixel)
            {
                case 8:
                    if (NativeMethods.filterLaplace_8bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                case 24:
                    if (NativeMethods.filterLaplace_24bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                case 32:
                    if (NativeMethods.filterLaplace_32bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies high-pass filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
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
        /// </remarks>
        public Image FilterHipass(int maskSize)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            Image dst = this.Clone(false);

            switch (this.BitsPerPixel)
            {
                case 8:
                    if (NativeMethods.filterHipass_8bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                case 24:
                    if (NativeMethods.filterHipass_24bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                case 32:
                    if (NativeMethods.filterHipass_32bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            return dst;
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Applies lowpass filter to the <see cref="Image"/>.
        /// </summary>
        /// <param name="maskSize">The size of the kernel (3 or 5).</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
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
        /// </remarks>
        public Image FilterLowpass(int maskSize)
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if (maskSize != 3 && maskSize != 5)
            {
                throw new ArgumentException("The mask size must be either 3 or 5.", nameof(maskSize));
            }

            Image dst = this.Clone(false);

            switch (this.BitsPerPixel)
            {
                case 8:
                    if (NativeMethods.filterLowpass_8bpp(this.Width, this.Height, this.Bits, this.Stride, dst.Bits, dst.Stride, maskSize) != 0)
                    {
                        throw new OutOfMemoryException();
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern int filterGaussian_8bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int kernelSize,
                float sigma);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterGaussian_24bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int kernelSize,
                float sigma);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterLaplace_8bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterLaplace_24bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterLaplace_32bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterHipass_8bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterHipass_24bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterHipass_32bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);

            [DllImport(NativeMethods.DllName)]
            public static extern int filterLowpass_8bpp(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int maskSize);
        }
    }
}
