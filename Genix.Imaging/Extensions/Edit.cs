﻿// -----------------------------------------------------------------------
// <copyright file="Edit.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides editing extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Edit
    {
        /// <summary>
        /// Gets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/>.</param>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>
        /// The color of the specified pixel.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><c>x</c> is less than 0, or greater than or equal to <see cref="Image.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><c>y</c> is less than 0, or greater than or equal to <see cref="Image.Height"/>.</para>
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetPixel(this Image image, int x, int y)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidatePosition(x, y);

            int pos = y * image.Stride;
            switch (image.BitsPerPixel)
            {
                case 1:
                    pos += x >> 6;
                    return (uint)(image.Bits[pos] >> (1 * (63 - (x & 63)))) & 1;

                case 2:
                    pos += x >> 5;
                    return (uint)(image.Bits[pos] >> (2 * (31 - (x & 31)))) & 3;

                case 4:
                    pos += x >> 4;
                    return (uint)(image.Bits[pos] >> (4 * (15 - (x & 15)))) & 0xf;

                case 8:
                    pos += x >> 3;
                    return (uint)(image.Bits[pos] >> (8 * (7 - (x & 7)))) & 0xff;

                case 16:
                    pos += x >> 2;
                    return (uint)(image.Bits[pos] >> (16 * (3 - (x & 3)))) & 0xffff;

                case 32:
                    pos += x >> 1;
                    return (uint)(image.Bits[pos] >> (32 * (1 - (x & 1)))) & 0xffff_ffff;

                default:
                    throw new InvalidOperationException(Properties.Resources.E_InvalidDepth);
            }
        }

        /// <summary>
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/>.</param>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <param name="color">The color to assign to the specified pixel.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><c>x</c> is less than 0, or greater than or equal to <see cref="Image.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><c>y</c> is less than 0, or greater than or equal to <see cref="Image.Height"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1,
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 2, 4, 8, and 16,
        /// <c>color</c> > 0 is masked to the maximum allowable pixel value, and any(invalid) higher order bits are discarded.
        /// </para>
        /// </remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPixel(this Image image, int x, int y, uint color)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidatePosition(x, y);

            int pos = y * image.Stride;
            int shift;
            switch (image.BitsPerPixel)
            {
                case 1:
                    pos += x >> 6;
                    if (color > 0)
                    {
                        image.Bits[pos] = BitUtils64.SetBit(image.Bits[pos], x & 63);
                    }
                    else
                    {
                        image.Bits[pos] = BitUtils64.ResetBit(image.Bits[pos], x & 63);
                    }

                    break;

                case 2:
                    pos += x >> 5;
                    shift = 2 * (x & 31);
                    image.Bits[pos] &= ~(0xc000_0000_0000_0000ul >> shift);
                    image.Bits[pos] |= (ulong)(color & 3) << ((2 * 31) - shift);
                    break;

                case 4:
                    pos += x >> 4;
                    shift = 4 * (x & 15);
                    image.Bits[pos] &= ~(0xf000_0000_0000_0000ul >> shift);
                    image.Bits[pos] |= (ulong)(color & 0xf) << ((4 * 15) - shift);
                    break;

                case 8:
                    pos += x >> 3;
                    shift = 8 * (x & 7);
                    image.Bits[pos] &= ~(0xff00_0000_0000_0000ul >> shift);
                    image.Bits[pos] |= (ulong)(color & 0xff) << ((8 * 7) - shift);
                    break;

                case 16:
                    pos += x >> 2;
                    shift = 16 * (x & 3);
                    image.Bits[pos] &= ~(0xffff_0000_0000_0000ul >> shift);
                    image.Bits[pos] |= (ulong)(color & 0xffff) << ((16 * 3) - shift);
                    break;

                case 32:
                    pos += x >> 1;
                    shift = 32 * (x & 1);
                    image.Bits[pos] &= ~(0xffff_ffff_0000_0000ul >> shift);
                    image.Bits[pos] |= (ulong)color << ((32 * 1) - shift);
                    break;

                default:
                    throw new InvalidOperationException(Properties.Resources.E_InvalidDepth);
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to clear.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1;
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetWhite(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Image dst = new Image(image);

            SetCopy.Set(
                dst.Bits.Length,
                dst.BitsPerPixel == 1 ? 0ul : ulong.MaxValue,
                dst.Bits,
                0);

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to clear.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1;
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetWhite(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            Image dst = image.Copy();

            ulong[] bits = dst.Bits;
            int bitCount = width * dst.BitsPerPixel;
            int stride1 = dst.Stride1;

            for (int i = 0, off = (y * stride1) + (x * dst.BitsPerPixel); i < height; i++, off += stride1)
            {
                if (dst.BitsPerPixel == 1)
                {
                    BitUtils64.ResetBits(bitCount, bits, off);
                }
                else
                {
                    BitUtils64.SetBits(bitCount, bits, off);
                }
            }

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to clear.</param>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetWhite(this Image image, System.Drawing.Rectangle rect)
        {
            return image.SetWhite(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels outside the specified area to white color.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to modify.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1;
        /// </para>
        /// </remarks>
        public static Image SetWhiteBorder(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            Image dst = new Image(image);
            CopyCrop.CopyArea(image, x, y, width, height, dst, x, y);

            if (image.BitsPerPixel > 1)
            {
                dst.SetWhiteBorderIP(x, y, width, height);
            }

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels outside the specified area to white color.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to modify.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1;
        /// </para>
        /// </remarks>
        public static void SetWhiteBorderIP(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            ulong[] bits = image.Bits;
            ulong white = image.BitsPerPixel == 1 ? 0ul : ulong.MaxValue;

            // clear top
            if (y > 0)
            {
                SetCopy.Set(y * image.Stride, white, bits, 0);
            }

            // clear left
            if (x > 0)
            {
                int count = x * image.BitsPerPixel;
                int pos = y * image.Stride1;
                for (int i = 0; i < height; i++, pos += image.Stride1)
                {
                    BitUtils64.SetBits(count, bits, pos);
                }
            }

            // clear right
            if (x + width < image.Width)
            {
                int count = (image.Width - (x + width)) * image.BitsPerPixel;
                int pos = (y * image.Stride1) + ((x + width) * image.BitsPerPixel);
                for (int i = 0; i < height; i++, pos += image.Stride1)
                {
                    BitUtils64.SetBits(count, bits, pos);
                }
            }

            // clear bottom
            if (y + height < image.Height)
            {
                SetCopy.Set((image.Height - (y + height)) * image.Stride, white, bits, (y + height) * image.Stride);
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetBlack(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Image dst = new Image(image);

            if (dst.BitsPerPixel == 1)
            {
                SetCopy.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

            return new Image(image);
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetBlack(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == image.Width && height == image.Height)
            {
                return image.SetBlack();
            }
            else
            {
                Image dst = image.Copy();

                ulong[] bits = dst.Bits;
                int count = width * dst.BitsPerPixel;
                int stride1 = dst.Stride1;

                for (int i = 0, off = (y * stride1) + (x * dst.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (dst.BitsPerPixel == 1)
                    {
                        BitUtils64.SetBits(count, bits, off);
                    }
                    else
                    {
                        BitUtils64.ResetBits(count, bits, off);
                    }
                }

                return dst;
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image SetBlack(this Image image, System.Drawing.Rectangle rect)
        {
            return image.SetBlack(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackIP(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            SetCopy.Set(
                image.Bits.Length,
                image.BitsPerPixel == 1 ? ulong.MaxValue : 0ul,
                image.Bits,
                0);
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackIP(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == image.Width && height == image.Height)
            {
                image.SetBlackIP();
            }
            else
            {
                ulong[] bits = image.Bits;
                int count = width * image.BitsPerPixel;
                int stride1 = image.Stride1;

                for (int i = 0, off = (y * stride1) + (x * image.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (image.BitsPerPixel == 1)
                    {
                        BitUtils64.SetBits(count, bits, off);
                    }
                    else
                    {
                        BitUtils64.ResetBits(count, bits, off);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackIP(this Image image, System.Drawing.Rectangle rect)
        {
            image.SetBlackIP(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Inverts all pixels in the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to invert.</param>
        /// <returns>
        /// A new inverted <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Invert(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Image dst = new Image(image);
            BitUtils64.WordsNOT(image.Bits.Length, image.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Inverts all pixels in the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to invert.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvertIP(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            BitUtils64.WordsNOT(image.Bits.Length, image.Bits, 0);
        }
    }
}