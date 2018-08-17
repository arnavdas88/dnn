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
        /// <para>The kernel size is not an odd number ot it is less than three.</para>
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

        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
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
