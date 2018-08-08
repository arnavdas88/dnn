// -----------------------------------------------------------------------
// <copyright file="FeatureDetectors.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <content>
    /// Provides feature detection methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Suppresses the lines on the <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> with lines suppressed.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        public Image SuppressLines()
        {
            if (this.BitsPerPixel != 1 && this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            // IPP does not support 1bpp images - convert to 8bpp
            Image src;
            bool convert1bpp = false;
            if (this.BitsPerPixel == 1)
            {
                src = this.Convert1To8(255, 0);
                convert1bpp = true;
            }
            else
            {
                src = this;
            }

            Image dst = src.Clone(false);

            if (NativeMethods.supresslines(
                src.Width,
                src.Height,
                src.Stride,
                src.Bits,
                dst.Bits) != 0)
            {
                throw new OutOfMemoryException();
            }

            // convert back to 1bpp
            if (convert1bpp)
            {
                dst = dst.Convert8To1(1);
            }

            return dst;
        }

        /// <summary>
        /// Calculates the histogram of oriented gradients (HOG) on the <see cref="Image"/>.
        /// </summary>
        /// <param name="cellSize">The cell size, in pixels.</param>
        /// <param name="blockSize">The block size, in number of <paramref name="cellSize"/>.</param>
        /// <param name="numberOfBins">The number of bins (orientations) in the histogram.</param>
        /// <returns>
        /// The tuple that contains the feature vectors and the feature vector length.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1, 8, 24, or 32.
        /// </exception>
        public (float[] vectors, int vectorLength) HOG(int cellSize, int blockSize, int numberOfBins)
        {
            // convert image to float
            ImageF srcf = PrepareImage();

            // calculate gradient vectors magnitude and direction using Prewitt operator (-1 0 1)
            float[] magnitude = new float[srcf.Bits.Length];
            float[] angles = new float[srcf.Bits.Length];
            NativeMethods.gradientVectorPrewitt_f32(
                srcf.Width,
                srcf.Height,
                srcf.Bits,
                srcf.Stride,
                magnitude,
                srcf.Stride,
                angles,
                srcf.Stride);

            // convert angles to bins
            Math32f.DivC(angles.Length, (float)(Math.PI / numberOfBins), angles, 0);
            Math32f.Abs(angles.Length, angles, 0);

            return (null, 0);

            ImageF PrepareImage()
            {
                // convert image to 8bpp
                Image src;
                switch (this.BitsPerPixel)
                {
                    case 1:
                        src = this.Convert1To8(255, 0);
                        break;

                    case 8:
                        src = this;
                        break;

                    case 24:
                        src = this.Convert24To8();
                        break;

                    case 32:
                        src = this.Convert32To8();
                        break;

                    default:
                        throw new NotSupportedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }

                /*if (NativeMethods.hog(
                    src.BitsPerPixel,
                    src.Width,
                    src.Height,
                    src.Stride,
                    src.Bits) != 0)
                {
                    throw new OutOfMemoryException();
                }*/

                // convert image to float
                return src.Convert8To32f(
                    Mathematics.RoundUp(src.Width, cellSize),
                    Mathematics.RoundUp(src.Height, cellSize),
                    BorderType.BorderRepl,
                    0);
            }
        }

        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int supresslines(
               int width,
               int height,
               int stride,
               [In] ulong[] src,
               [Out] ulong[] dst);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int hog(
                int bitsPerPixel,
                int width,
                int height,
                int stride,
                [In] ulong[] src);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int cart2polar(
                int n,
                [In] float[] re,
                [In] float[] im,
                [Out] float[] magnitude,
                [Out] float[] phase);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int gradientVectorPrewitt_f32(
                int width,
                int height,
                [In] float[] src,
                int stride,
                [Out] float[] magnitude,
                int magnitudeStride,
                [Out] float[] angle,
                int angleStride);
        }
    }
}
