// -----------------------------------------------------------------------
// <copyright file="FeatureDetectors.cs" company="Noname, Inc.">
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
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        public void HOG()
        {
            if (this.BitsPerPixel != 1 && this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            // IPP does not support 1bpp images - convert to 8bpp
            Image src = this.BitsPerPixel == 1 ? this.Convert1To8(255, 0) : this;

            if (NativeMethods.hog(
                src.BitsPerPixel,
                src.Width,
                src.Height,
                src.Stride,
                src.Bits) != 0)
            {
                throw new OutOfMemoryException();
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
        }
    }
}
