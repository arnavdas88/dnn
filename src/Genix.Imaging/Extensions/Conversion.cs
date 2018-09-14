// -----------------------------------------------------------------------
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="bitsPerPixel">The requested image depth.</param>
        /// <returns>
        /// A new <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// The method converts image to the specified depth using default conversion method.
        /// If the conversion is not required, the method returns this <see cref="Image"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// The conversion is not supported.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image ConvertTo(Image image, int bitsPerPixel)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            switch (bitsPerPixel)
            {
                case 1: return ConvertTo1bpp();
                case 8: return ConvertTo8bpp();
                case 16: return ConvertTo16bpp();
                case 24: return ConvertTo24bpp();
                case 32: return ConvertTo32bpp();

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, bitsPerPixel));
            }

            Image ConvertTo1bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return image;
                    case 8: return Image.Binarize(image);
                    case 24: return Image.Binarize(Image.Convert24To8(image));
                    case 32: return Image.Binarize(Image.Convert32To8(image));

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo8bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return Image.Convert1To8(image);
                    case 2: return Image.Convert2To8(image);
                    case 4: return Image.Convert4To8(image);
                    case 8: return image;
                    case 24: return Image.Convert24To8(image);
                    case 32: return Image.Convert32To8(image);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo16bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return Image.Convert1To16(image);
                    case 16: return image;

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo24bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return Image.Convert8To24(Image.Convert1To8(image));
                    case 2: return Image.Convert8To24(Image.Convert2To8(image));
                    case 4: return Image.Convert8To24(Image.Convert4To8(image));
                    case 8: return Image.Convert8To24(image);
                    case 24: return image;
                    case 32: return Image.Convert32To24(image);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo32bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return Image.Convert8To32(Image.Convert1To8(image), 255);
                    case 2: return Image.Convert8To32(Image.Convert2To8(image), 255);
                    case 4: return Image.Convert8To32(Image.Convert4To8(image), 255);
                    case 8: return Image.Convert8To32(image, 255);
                    case 24: return Image.Convert24To32(image);
                    case 32: return image;

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
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
        /// <param name="image">The <see cref="Image"/> to normalize.</param>
        /// <returns>
        /// A new normalized <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public static Image NormalizeBackground(Image image/*, byte threshold, int sx, int sy*/)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            int sx = 64;
            int sy = 128;
            byte threshold = 128;

            Histogram ghist = image.GrayHistogram();
            Histogram vhist = image.HistogramY();

            // generate foreground mask
            Image mask = Image.Convert8To1(image, threshold);
            mask = Image.Dilate(mask, StructuringElement.Square(7), 1);
            mask = Image.Convert1To8(mask);

            // use mask to remove foreground pixels from original image
            Image values = image & mask;

            // generate map
            ////int wd = (this.Width + sx - 1) / sx;
            ////int hd = (this.Height + sy - 1) / sy;

            int nx = image.Width / sx;
            int ny = image.Height / sy;
            long[] map = new long[ny * nx];

            for (int iy = 0, ty = 0, imap = 0; iy < ny; iy++, ty += sy)
            {
                int th = iy + 1 == ny ? image.Height - ty : sy;

                for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                {
                    int tw = ix + 1 == nx ? image.Width - tx : sx;

                    map[imap++] = values.Power(tx, ty, tw, th);
                }
            }

            return image;
        }

        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public static Image Binarize(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                1,
                image.HorizontalResolution,
                image.VerticalResolution);

            NativeMethods.otsu(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// Converts a binary <see cref="Image"/> to 8-bit gray scale image.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses 255 for zero (white) pixels and 0 for one (black) pixels.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert1To8(Image image) => Image.Convert1To8(image, 255, 0);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 8-bit gray scale image.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static Image Convert1To8(Image image, byte value0, byte value1)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert1to8(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// Converts a binary <see cref="Image"/> to 16-bit gray scale image.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses 65535 for zero (white) pixels and 0 for one (black) pixels.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert1To16(Image image) => Image.Convert1To16(image, ushort.MaxValue, 0);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 16-bit gray scale image.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="value0">16-bit value to be used for 0s pixels.</param>
        /// <param name="value1">16-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [CLSCompliant(false)]
        public static Image Convert1To16(Image image, ushort value0, ushort value1)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert1to16(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// Converts a binary <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// The destination <see cref="ImageF"/> this method creates.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses 0.0f for zero (white) pixels and 1.0f for one (black) pixels.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImageF Convert1To32f(Image image) => Image.Convert1To32f(image, 0, 1);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="ImageF"/> this method creates.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static ImageF Convert1To32f(Image image, float value0, float value1)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ImageF dst = new ImageF(
                image.Width,
                image.Height,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert1to32f(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses values 0x00, 0x55, 0xaa, and 0xff.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 2.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert2To8(Image image) => Image.Convert2To8(image, 0x00, 0x55, 0xaa, 0xff);

        /// <summary>
        /// Converts a 2-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 2.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static Image Convert2To8(Image image, byte value0, byte value1, byte value2, byte value3)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 2)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert2to8(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// The unpacking uses shift replication, i.e. each pixel is converted using this formula: <code>(val shl 4) | val</code>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 4.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static Image Convert4To8(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 4)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert4to8(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static Image Convert8To1(Image image, byte threshold)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                1,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert8to1(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new 24-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static Image Convert8To24(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                24,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert8to24(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="alpha">Constant value to create the alpha channel.</param>
        /// <returns>
        /// A new 32-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static Image Convert8To32(Image image, byte alpha)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                32,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert8to32(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static ImageF Convert8To32f(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                image.Width,
                image.Height,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert8to32f(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <param name="width">The width of created image.</param>
        /// <param name="height">The height of created image.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static ImageF Convert8To32f(
            Image image,
            int width,
            int height,
            BorderType borderType,
            float borderValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                width,
                height,
                image.HorizontalResolution,
                image.VerticalResolution);

            // convert pixels
            int areaWidth = Core.MinMax.Min(width, image.Width);
            int areaHeight = Core.MinMax.Min(height, image.Height);
            if (NativeMethods._convert8to32f(
                0,
                0,
                areaWidth,
                areaHeight,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static Image Convert24To8(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert24to8(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new 32-bit <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 24.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static Image Convert24To32(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                32,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert24to32(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
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
        public static Image Convert32To8(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert32to8(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public static Image Convert32To24(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                24,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert32to24(
                0,
                0,
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
            public static extern int _convert1to16(
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               ushort value0,
               ushort value1);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert1to32f(
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] float[] dst,
               int stridedst,
               float value0,
               float value1);

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
