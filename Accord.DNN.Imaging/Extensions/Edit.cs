// -----------------------------------------------------------------------
// <copyright file="Edit.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint GetPixel(this Image image, int x, int y)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidatePosition(x, y);

            int pos = y * image.Stride32;
            switch (image.BitsPerPixel)
            {
                case 1:
                    pos += x >> 5;
                    return (image.Bits[pos] >> (1 * (31 - (x & 31)))) & 1;

                case 2:
                    pos += x >> 4;
                    return (image.Bits[pos] >> (2 * (15 - (x & 15)))) & 3;

                case 4:
                    pos += x >> 3;
                    return (image.Bits[pos] >> (4 * (7 - (x & 7)))) & 0xf;

                case 8:
                    pos += x >> 2;
                    return (image.Bits[pos] >> (8 * (3 - (x & 3)))) & 0xff;

                case 16:
                    pos += x >> 1;
                    return (image.Bits[pos] >> (16 * (1 - (x & 1)))) & 0xffff;

                case 32:
                    pos += x;
                    return image.Bits[pos];

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static void SetPixel(this Image image, int x, int y, uint color)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.ValidatePosition(x, y);

            int pos = y * image.Stride32;
            int shift;
            switch (image.BitsPerPixel)
            {
                case 1:
                    pos += x >> 5;
                    shift = 1 * (x & 31);
                    if (color > 0)
                    {
                        image.Bits[pos] |= 0x8000_0000u >> shift;
                    }
                    else
                    {
                        image.Bits[pos] &= ~(0x8000_0000u >> shift);
                    }

                    break;

                case 2:
                    pos += x >> 4;
                    shift = 2 * (x & 15);
                    image.Bits[pos] &= ~(0xc000_0000u >> shift);
                    image.Bits[pos] |= (color & 3) << ((2 * 15) - shift);
                    break;

                case 4:
                    pos += x >> 3;
                    shift = 4 * (x & 7);
                    image.Bits[pos] &= ~(0xf000_0000u >> shift);
                    image.Bits[pos] |= (color & 0xf) << ((4 * 7) - shift);
                    break;

                case 8:
                    pos += x >> 2;
                    shift = 8 * (x & 3);
                    image.Bits[pos] &= ~(0xff00_0000u >> shift);
                    image.Bits[pos] |= (color & 0xff) << ((8 * 3) - shift);
                    break;

                case 16:
                    pos += x >> 1;
                    shift = 16 * (x & 1);
                    image.Bits[pos] &= ~(0xffff_0000u >> shift);
                    image.Bits[pos] |= (color & 0xffff) << ((16 * 1) - shift);
                    break;

                case 32:
                    pos += x;
                    image.Bits[pos] = color;
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

            MKL.Set(
                dst.Bits.Length,
                dst.BitsPerPixel == 1 ? 0 : uint.MaxValue,
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

            uint[] bits = dst.Bits;
            int bitCount = width * dst.BitsPerPixel;
            int stride1 = dst.Stride1;

            for (int i = 0, off = (y * stride1) + (x * dst.BitsPerPixel); i < height; i++, off += stride1)
            {
                if (dst.BitsPerPixel == 1)
                {
                    BitUtils.ResetBits(bitCount, bits, off);
                }
                else
                {
                    BitUtils.SetBits(bitCount, bits, off);
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

            uint[] bits = image.Bits;
            uint white = image.BitsPerPixel == 1 ? 0 : uint.MaxValue;

            // clear top
            if (y > 0)
            {
                MKL.Set(y * image.Stride32, white, bits, 0);
            }

            // clear left
            if (x > 0)
            {
                int count = x * image.BitsPerPixel;
                int pos = y * image.Stride1;
                for (int i = 0; i < height; i++, pos += image.Stride1)
                {
                    BitUtils.SetBits(count, bits, pos);
                }
            }

            // clear right
            if (x + width < image.Width)
            {
                int count = (image.Width - (x + width)) * image.BitsPerPixel;
                int pos = (y * image.Stride1) + ((x + width) * image.BitsPerPixel);
                for (int i = 0; i < height; i++, pos += image.Stride1)
                {
                    BitUtils.SetBits(count, bits, pos);
                }
            }

            // clear bottom
            if (y + height < image.Height)
            {
                MKL.Set((image.Height - (y + height)) * image.Stride32, white, bits, (y + height) * image.Stride32);
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
                NativeMethods.mkl_seti32(
                    dst.Bits.Length,
                    uint.MaxValue,
                    dst.Bits,
                    0);
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

            Image dst = image.Copy();

            uint[] bits = dst.Bits;
            int bitCount = width * dst.BitsPerPixel;
            int stride1 = dst.Stride1;

            for (int i = 0, off = (y * stride1) + (x * dst.BitsPerPixel); i < height; i++, off += stride1)
            {
                if (dst.BitsPerPixel == 1)
                {
                    BitUtils.SetBits(bitCount, bits, off);
                }
                else
                {
                    BitUtils.ResetBits(bitCount, bits, off);
                }
            }

            return dst;
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

            uint[] srcbits = image.Bits;
            uint[] dstbits = dst.Bits;
            for (int i = 0, ii = srcbits.Length; i < ii; i++)
            {
                dstbits[i] = ~srcbits[i];
            }

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

            uint[] bits = image.Bits;
            for (int i = 0, ii = bits.Length; i < ii; i++)
            {
                bits[i] = ~bits[i];
            }
        }

        private static class NativeMethods
        {
            [DllImport("Accord.DNN.CPP.dll")]
            [SuppressUnmanagedCodeSecurity]
            public static extern void mkl_seti32(int n, uint a, [Out] uint[] y, int offy);
        }
    }
}
