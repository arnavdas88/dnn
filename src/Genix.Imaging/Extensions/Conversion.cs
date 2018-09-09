﻿// -----------------------------------------------------------------------
// <copyright file="Conversion.cs" company="Noname, Inc.">
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
    using Genix.Core;
    ////using Leptonica;

    /// <content>
    /// Provides conversion methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Converts this <see cref="Image"/> to a specified depth.
        /// </summary>
        /// <param name="bitsPerPixel">The requested image depth.</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// The method converts image to the specified depth using default conversion method.
        /// If the conversion is not required, the method returns this <see cref="Image"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException">
        /// The conversion is not supported.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image ConvertTo(int bitsPerPixel)
        {
            switch (bitsPerPixel)
            {
                case 1: return ConvertTo1bpp();
                case 8: return ConvertTo8bpp();
                case 24: return ConvertTo24bpp();
                case 32: return ConvertTo32bpp();

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, bitsPerPixel));
            }

            Image ConvertTo1bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this;
                    case 8: return this.Binarize();
                    case 24: return this.Convert24To8().Binarize();
                    case 32: return this.Convert32To8().Binarize();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo8bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8();
                    case 2: return this.Convert2To8();
                    case 4: return this.Convert4To8();
                    case 8: return this;
                    case 24: return this.Convert24To8();
                    case 32: return this.Convert32To8();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo24bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8().Convert8To24();
                    case 2: return this.Convert2To8().Convert8To24();
                    case 4: return this.Convert4To8().Convert8To24();
                    case 8: return this.Convert8To24();
                    case 24: return this;
                    case 32: return this.Convert32To24();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo32bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8().Convert8To32(255);
                    case 2: return this.Convert2To8().Convert8To32(255);
                    case 4: return this.Convert4To8().Convert8To32(255);
                    case 8: return this.Convert8To32(255);
                    case 24: return this.Convert24To32();
                    case 32: return this;

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }
        }

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
                              .Convert1To8();

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
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses 255 for zero pixels and 0 for one pixels.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Convert1To8() => this.Convert1To8(255, 0);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to gray scale.
        /// </summary>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert1To8(byte value0, byte value1)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
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
        /// Converts a 2-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses values 0x00, 0x55, 0xaa, and 0xff.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 2.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Convert2To8() => this.Convert2To8(0x00, 0x55, 0xaa, 0xff);

        /// <summary>
        /// Converts a 2-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <param name="value2">8-bit value to be used for 2s pixels.</param>
        /// <param name="value3">8-bit value to be used for 3s pixels.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 0 (0x00), <paramref name="value1"/> = 85 (0x55),
        /// <paramref name="value2"/> = 170 (0xaa), and <paramref name="value3"/> = 255 (0xff).
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 2.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert2To8(byte value0, byte value1, byte value2, byte value3)
        {
            if (this.BitsPerPixel != 2)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                8,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert2to8(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                value0,
                value1,
                value2,
                value3) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        /// <summary>
        /// Converts a 4-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// The unpacking uses shift replication, i.e. each pixel is converted using this formula: <code>(val shl 4) | val</code>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 4.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert4To8()
        {
            if (this.BitsPerPixel != 4)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                8,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert4to8(
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

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to binary.
        /// </summary>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
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
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
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
        /// Converts a gray scale <see cref="Image"/> to a color 24-bit <see cref="Image"/> by copying luminance component to color components.
        /// </summary>
        /// <returns>
        /// A new 24-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts a gray scale image to an RGB/BGR image by copying luminance component to color components.
        /// </para>
        /// </remarks>
        public Image Convert8To24()
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                24,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert8to24(
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

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a color 32-bit <see cref="Image"/> by copying luminance component to color components.
        /// </summary>
        /// <param name="alpha">Constant value to create the alpha channel.</param>
        /// <returns>
        /// A new 32-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts a gray scale image to an RGB/BGR image by copying luminance component to color components.
        /// </para>
        /// <para>
        /// The alpha channel is filled with the provided value.
        /// </para>
        /// </remarks>
        public Image Convert8To32(byte alpha)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                32,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert8to32(
                0,
                0,
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                alpha) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert8To32f()
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                this.Width,
                this.Height,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert8to32f(
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

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/> of specified width and height.
        /// </summary>
        /// <param name="width">The width of created image.</param>
        /// <param name="height">The height of created image.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert8To32f(int width, int height, BorderType borderType, float borderValue)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                width,
                height,
                this.HorizontalResolution,
                this.VerticalResolution);

            // convert pixels
            int areaWidth = Core.MinMax.Min(width, this.Width);
            int areaHeight = Core.MinMax.Min(height, this.Height);
            if (NativeMethods._convert8to32f(
                0,
                0,
                areaWidth,
                areaHeight,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride) != 0)
            {
                throw new OutOfMemoryException();
            }

            // set border
            dst.SetBorder(0, 0, areaWidth, areaHeight, borderType, borderValue);

            return dst;
        }

        /// <summary>
        /// Converts a color 24-bit <see cref="Image"/> to gray scale using fixed transform coefficients.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 24.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Conversion from color image to gray scale uses the following basic equation to compute luma from nonlinear gamma-corrected red, green, and blue values:
        /// Y' = 0.299 * R' + 0.587 * G' + 0.114 * B'.
        /// Note that the transform coefficients conform to the standard for the NTSC red, green, and blue CRT phosphors.
        /// </para>
        /// </remarks>
        public Image Convert24To8()
        {
            if (this.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                8,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert24to8(
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

        /// <summary>
        /// Converts a color 24-bit <see cref="Image"/> to a color 32-bit <see cref="Image"/> by adding alpha channel.
        /// </summary>
        /// <returns>
        /// A new 32-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 24.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert24To32()
        {
            if (this.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                32,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert24to32(
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

        /// <summary>
        /// Converts a color 32-bit <see cref="Image"/> to gray scale using fixed transform coefficients.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
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
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                8,
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

        /// <summary>
        /// Converts a color 32-bit <see cref="Image"/> to a color 24-bit <see cref="Image"/> by discarding alpha channel.
        /// </summary>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert32To24()
        {
            if (this.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                this.Width,
                this.Height,
                24,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert32to24(
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

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
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
            public static extern int _convert2to8(
              int width,
              int height,
              [In] ulong[] src,
              int stridesrc,
              [Out] ulong[] dst,
              int stridedst,
              byte value0,
              byte value1,
              byte value2,
              byte value3);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert4to8(
              int width,
              int height,
              [In] ulong[] src,
              int stridesrc,
              [Out] ulong[] dst,
              int stridedst);

            [DllImport(NativeMethods.DllName)]
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
            public static extern int _convert8to24(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert8to32(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                byte alpha);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert8to32f(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] float[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert24to8(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert24to32(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
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
            public static extern int _convert32to24(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
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
