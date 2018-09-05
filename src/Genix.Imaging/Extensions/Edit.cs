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
    using Genix.Drawing;

    /// <content>
    /// Provides editing extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Gets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>
        /// The color of the specified pixel.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="x"/> is less than 0, or greater than or equal to <see cref="Image{T}.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is less than 0, or greater than or equal to <see cref="Image{T}.Height"/>.</para>
        /// </exception>
        [CLSCompliant(false)]
        public uint GetPixel(int x, int y)
        {
            this.ValidatePosition(x, y);

            int xpos = x * this.BitsPerPixel;
            int pos = (y * this.Stride) + (xpos >> 6);
            xpos &= 63;

            if (this.BitsPerPixel == 24 && xpos + 24 > 64)
            {
                int rem = 64 - xpos;
                return (uint)BitUtils64.GetBits(this.Bits[pos], xpos, rem) |
                       (uint)(BitUtils64.GetBits(this.Bits[pos + 1], 0, 24 - rem) << rem);
            }
            else
            {
                return (uint)BitUtils64.GetBits(this.Bits[pos], xpos, this.BitsPerPixel);
            }
        }

        /// <summary>
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <param name="color">The color to assign to the specified pixel.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="x"/> is less than 0, or greater than or equal to <see cref="Image{T}.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is less than 0, or greater than or equal to <see cref="Image{T}.Height"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1,
        /// <paramref name="color"/> > 0 sets the bit on.
        /// </para>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 2, 4, 8, and 16,
        /// <paramref name="color"/> > 0 is masked to the maximum allowable pixel value, and any(invalid) higher order bits are discarded.
        /// </para>
        /// </remarks>
        [CLSCompliant(false)]
        public void SetPixel(int x, int y, uint color)
        {
            this.ValidatePosition(x, y);

            int xpos = x * this.BitsPerPixel;
            int pos = (y * this.Stride) + (xpos >> 6);
            xpos &= 63;

            if (this.BitsPerPixel == 1)
            {
                if (color > 0)
                {
                    this.Bits[pos] = BitUtils64.SetBit(this.Bits[pos], xpos);
                }
                else
                {
                    this.Bits[pos] = BitUtils64.ResetBit(this.Bits[pos], xpos);
                }
            }
            else if (this.BitsPerPixel == 24 && xpos + 24 > 64)
            {
                int rem = 64 - xpos;
                this.Bits[pos] = BitUtils64.CopyBits(this.Bits[pos], xpos, rem, color);
                this.Bits[pos + 1] = BitUtils64.CopyBits(this.Bits[pos + 1], 0, 24 - rem, color >> rem);
            }
            else
            {
                this.Bits[pos] = BitUtils64.CopyBits(this.Bits[pos], xpos, this.BitsPerPixel, color);
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to white color not-in-place.
        /// </summary>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        public Image SetWhite()
        {
            Image dst = this.Clone(false);

            if (dst.BitsPerPixel != 1)
            {
                Arrays.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        public Image SetWhite(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetWhite();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetWhiteIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetWhite(Rectangle rect) => this.SetWhite(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to white color in-place.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteIP() => Arrays.Set(
            this.Bits.Length,
            this.BitsPerPixel == 1 ? 0ul : ulong.MaxValue,
            this.Bits,
            0);

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to white color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteIP(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetWhiteIP();
            }
            else
            {
                ulong[] bits = this.Bits;
                int count = width * this.BitsPerPixel;
                int stride1 = this.Stride1;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (this.BitsPerPixel == 1)
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
        /// Sets all <see cref="Image"/> pixels in the specified area to white color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteIP(Rectangle rect) => this.SetWhiteIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all <see cref="Image"/> pixels outside the specified area to white color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new cleared <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        public Image SetWhiteBorder(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Image dst = this.Clone(false);
            Image.CopyArea(this, x, y, width, height, dst, x, y);

            if (this.BitsPerPixel > 1)
            {
                dst.SetWhiteBorderIP(x, y, width, height);
            }

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels outside the specified area to white color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the white color is 0; otherwise, the white color is 2^<see cref="Image{T}.BitsPerPixel"/> - 1.
        /// </para>
        /// </remarks>
        public void SetWhiteBorderIP(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            ulong[] bits = this.Bits;
            ulong white = this.BitsPerPixel == 1 ? 0ul : ulong.MaxValue;

            // clear top
            if (y > 0)
            {
                Arrays.Set(y * this.Stride, white, bits, 0);
            }

            // clear left
            if (x > 0)
            {
                int count = x * this.BitsPerPixel;
                int pos = y * this.Stride1;
                for (int i = 0; i < height; i++, pos += this.Stride1)
                {
                    BitUtils64.SetBits(count, bits, pos);
                }
            }

            // clear right
            if (x + width < this.Width)
            {
                int count = (this.Width - (x + width)) * this.BitsPerPixel;
                int pos = (y * this.Stride1) + ((x + width) * this.BitsPerPixel);
                for (int i = 0; i < height; i++, pos += this.Stride1)
                {
                    BitUtils64.SetBits(count, bits, pos);
                }
            }

            // clear bottom
            if (y + height < this.Height)
            {
                Arrays.Set((this.Height - (y + height)) * this.Stride, white, bits, (y + height) * this.Stride);
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to black color not-in-place.
        /// </summary>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        public Image SetBlack()
        {
            Image dst = this.Clone(false);

            if (dst.BitsPerPixel == 1)
            {
                Arrays.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        public Image SetBlack(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetBlack();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetBlackIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new filled <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetBlack(Rectangle rect) => this.SetBlack(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all <see cref="Image"/> pixels to black color in-place.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlackIP() => Arrays.Set(
            this.Bits.Length,
            this.BitsPerPixel == 1 ? ulong.MaxValue : 0ul,
            this.Bits,
            0);

        /// <summary>
        /// Sets all <see cref="Image"/> pixels in the specified area to black color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        public void SetBlackIP(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetBlackIP();
            }
            else
            {
                ulong[] bits = this.Bits;
                int count = width * this.BitsPerPixel;
                int stride1 = this.Stride1;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (this.BitsPerPixel == 1)
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
        /// Sets all <see cref="Image"/> pixels in the specified area to black color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1, the black color is 1; otherwise, the black color is 0.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlackIP(Rectangle rect) => this.SetBlackIP(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
