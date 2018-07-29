// -----------------------------------------------------------------------
// <copyright file="Conversion.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    ////using Leptonica;

    /// <content>
    /// Provides conversion methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        //// <param name="threshold">The threshold to determine foreground.</param>
        //// <param name="sx">The tile width.</param>
        //// <param name="sy">The tile height.</param>

        /// <summary>
        /// Normalizes the <see cref="Image"/> intensity be mapping the image
        /// so that the background is near the specified value.
        /// </summary>
        /// <returns>
        /// A new normalized <see cref="Image"/>.
        /// </returns>
        public Image NormalizeBackground(/*, byte threshold, int sx, int sy*/)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            int sx = 64;
            int sy = 128;
            byte threshold = 128;

            Histogram ghist = this.GrayHistogram();
            Histogram vhist = this.HistogramY();

            // generate foreground mask
            Image mask = this.Convert8To1(threshold)
                              .Dilate(StructuringElement.Rectangle(7, 1), 1)
                              .Dilate(StructuringElement.Rectangle(1, 7), 1)
                              .Convert1To8(255, 0);

            // use mask to remove foreground pixels from original image
            Image values = this & mask;

            // generate map
            ////int wd = (this.Width + sx - 1) / sx;
            ////int hd = (this.Height + sy - 1) / sy;

            int nx = this.Width / sx;
            int ny = this.Height / sy;
            long[] map = new long[ny * nx];

            for (int iy = 0, ty = 0, imap = 0; iy < ny; iy++, ty += sy)
            {
                int th = iy + 1 == ny ? this.Height - ty : sy;

                for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                {
                    int tw = ix + 1 == nx ? this.Width - tx : sx;

                    map[imap++] = values.Power(tx, ty, tw, th);
                }
            }

            return this;
        }

        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        public Image Binarize()
        {
            if (this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                1,
                this.HorizontalResolution,
                this.VerticalResolution);

            NativeMethods.otsu(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                64,
                128,
                2,
                2);

            return dst;

            /*try
            {
                using (Pix pixs = this.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        return pixd.CreateImage(this.HorizontalResolution, this.VerticalResolution);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot binarize the this.", e);
            }*/
        }

        /// <summary>
        /// Converts a binary <see cref="Image"/> to gray scale.
        /// </summary>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        public Image Convert1To8(byte value0, byte value1)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                8,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert1to8(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                value0,
                value1) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to binary.
        /// </summary>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If the input pixel is more than, or equal to the <paramref name="threshold"/> value, the corresponding output bit is set to 0 (white).
        /// </para>
        /// <para>
        /// If the input pixel is less than the <paramref name="threshold"/> value, the corresponding output bit is set to 1 (black).
        /// </para>
        /// </remarks>
        public Image Convert8To1(byte threshold)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                1,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert8to1(
                0,
                0,
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                threshold) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        /// <summary>
        /// Converts a color <see cref="Image"/> to gray scale using fixed transform coefficients.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Conversion from color image to gray scale uses the following basic equation to compute luma from nonlinear gamma-corrected red, green, and blue values:
        /// Y' = 0.299 * R' + 0.587 * G' + 0.114 * B'.
        /// Note that the transform coefficients conform to the standard for the NTSC red, green, and blue CRT phosphors.
        /// </para>
        /// </remarks>
        public Image Convert32To8()
        {
            if (this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                1,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert32to8(
                0,
                0,
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int _convert1to8(
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               byte value0,
               byte value1);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int _convert8to1(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                byte threshold);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int _convert32to8(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int otsu(
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int sx,
                int sy,
                int smoothx,
                int smoothy);
        }
    }
}
