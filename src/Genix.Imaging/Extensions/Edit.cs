// -----------------------------------------------------------------------
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
        /// <c>image</c> is <b>null</b>.
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

            int xpos = x * image.BitsPerPixel;
            return (uint)BitUtils64.GetBits(image.Bits[(y * image.Stride) + (xpos >> 6)], xpos & 63, image.BitsPerPixel);
        }

        /// <summary>
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/>.</param>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <param name="color">The color to assign to the specified pixel.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
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

            int xpos = x * image.BitsPerPixel;
            int pos = (y * image.Stride) + (xpos >> 6);
            if (image.BitsPerPixel == 1)
            {
                if (color > 0)
                {
                    image.Bits[pos] = BitUtils64.SetBit(image.Bits[pos], xpos & 63);
                }
                else
                {
                    image.Bits[pos] = BitUtils64.ResetBit(image.Bits[pos], xpos & 63);
                }
            }
            else
            {
                image.Bits[pos] = BitUtils64.CopyBits(image.Bits[pos], xpos & 63, image.BitsPerPixel, color);
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
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1.
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

            if (dst.BitsPerPixel != 1)
            {
                Arrays.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

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
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1.
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

            if (x == 0 && y == 0 && width == image.Width && height == image.Height)
            {
                return image.SetWhite();
            }
            else
            {
                Image dst = image.Copy();
                dst.SetWhiteIP(x, y, width, height);
                return dst;
            }
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
        /// <c>image</c> is <b>null</b>.
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
        /// Sets all <see cref="Image"/> pixels to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteIP(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Arrays.Set(
                image.Bits.Length,
                image.BitsPerPixel == 1 ? 0ul : ulong.MaxValue,
                image.Bits,
                0);
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteIP(this Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == image.Width && height == image.Height)
            {
                image.SetWhiteIP();
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
                        BitUtils64.ResetBits(count, bits, off);
                    }
                    else
                    {
                        BitUtils64.SetBits(count, bits, off);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to fill.</param>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 1; otherwise, the white color is 0;
        /// <c>color</c> > 0 sets the bit on.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteIP(this Image image, System.Drawing.Rectangle rect)
        {
            image.SetWhiteIP(rect.X, rect.Y, rect.Width, rect.Height);
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
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1.
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
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image.BitsPerPixel"/> - 1.
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
                Arrays.Set(y * image.Stride, white, bits, 0);
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
                Arrays.Set((image.Height - (y + height)) * image.Stride, white, bits, (y + height) * image.Stride);
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
        /// <c>image</c> is <b>null</b>.
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
                Arrays.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
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
        /// <c>image</c> is <b>null</b>.
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
                dst.SetBlackIP(x, y, width, height);
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
        /// <c>image</c> is <b>null</b>.
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
        /// <c>image</c> is <b>null</b>.
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

            Arrays.Set(
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
        /// <c>image</c> is <b>null</b>.
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
        /// <c>image</c> is <b>null</b>.
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
    }
}
