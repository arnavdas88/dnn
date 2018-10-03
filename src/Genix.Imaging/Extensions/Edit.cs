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
                return (uint)BitUtils.GetBits(this.Bits[pos], xpos, rem) |
                       (uint)(BitUtils.GetBits(this.Bits[pos + 1], 0, 24 - rem) << rem);
            }
            else
            {
                return (uint)BitUtils.GetBits(this.Bits[pos], xpos, this.BitsPerPixel);
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
                    this.Bits[pos] = BitUtils.SetBit(this.Bits[pos], xpos);
                }
                else
                {
                    this.Bits[pos] = BitUtils.ResetBit(this.Bits[pos], xpos);
                }
            }
            else if (this.BitsPerPixel == 24 && xpos + 24 > 64)
            {
                int rem = 64 - xpos;
                this.Bits[pos] = BitUtils.CopyBits(this.Bits[pos], xpos, rem, color);
                this.Bits[pos + 1] = BitUtils.CopyBits(this.Bits[pos + 1], 0, 24 - rem, color >> rem);
            }
            else
            {
                this.Bits[pos] = BitUtils.CopyBits(this.Bits[pos], xpos, this.BitsPerPixel, color);
            }
        }

        /// <summary>
        /// Sets all image pixels to the specified color.
        /// </summary>
        /// <param name="color">The color to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public void SetColor(uint color)
        {
            if (this.BitsPerPixel == 24)
            {
                byte bcolor = (byte)(color & 0xff);
                if (bcolor == (byte)((color >> 8) & 0xff) && bcolor == (byte)((color >> 16) & 0xff))
                {
                    // all components are the same - set bytes
                    unsafe
                    {
                        fixed (ulong* bits = this.Bits)
                        {
                            Vectors.Set(this.Height * this.Stride8, bcolor, (byte*)bits);
                        }
                    }
                }
                else
                {
                    ulong[] colors = this.ColorScanline(Image.CalculateStride(this.Width, 24), color);
                    Vectors.Tile(this.Stride, this.Height, colors, 0, this.Bits, 0);

                }
            }
            else
            {
                Vectors.Set(this.Bits.Length, this.ColorBits(color), this.Bits, 0);
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to the specified color.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="color">The color to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        public void SetColor(int x, int y, int width, int height, uint color)
        {
            this.ValidateArea(x, y, width, height);

            if (this.BitsPerPixel == 24)
            {
                byte bcolor = (byte)(color & 0xff);
                if (bcolor == (byte)((color >> 8) & 0xff) && bcolor == (byte)((color >> 16) & 0xff))
                {
                    // all components are the same - set bytes
                    unsafe
                    {
                        fixed (ulong* bits = &this.Bits[y * this.Stride])
                        {
                            byte* bbits = (byte*)bits;
                            if (x == 0 && width == this.Width)
                            {
                                // set multiple scan lines at once
                                // if entire image width has to be set
                                Vectors.Set(height * this.Stride8, bcolor, bbits);
                            }
                            else
                            {
                                bbits += x * 3;
                                for (int i = 0, count = width * 3, stride8 = this.Stride8; i < height; i++, bbits += stride8)
                                {
                                    Vectors.Set(count, bcolor, bbits);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ulong[] colors = this.ColorScanline(Image.CalculateStride(width, 24), color);

                    if (x == 0 && width == this.Width)
                    {
                        // set multiple scan lines at once
                        // if entire image width has to be set
                        Vectors.Tile(this.Stride, height, colors, 0, this.Bits, y * this.Stride);
                    }
                    else
                    {
                        unsafe
                        {
                            fixed (ulong* bits = &this.Bits[y * this.Stride], ucolors = colors)
                            {
                                byte* bbits = (byte*)bits + (x * 3);
                                byte* bcolors = (byte*)ucolors;

                                for (int i = 0, count = width * 3, stride8 = this.Stride8; i < height; i++, bbits += stride8)
                                {
                                    Vectors.Copy(count, bcolors, bbits);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ulong colorbits = this.ColorBits(color);
                ulong[] bits = this.Bits;

                if (x == 0 && width == this.Width)
                {
                    // set multiple scan lines at once
                    // if entire image width has to be set
                    Vectors.Set(height * this.Stride, colorbits, bits, y * this.Stride);
                }
                else
                {
                    int stride1 = this.Stride1;
                    int count = width * this.BitsPerPixel;
                    int off = (y * stride1) + (x * this.BitsPerPixel);

                    if (colorbits == 0)
                    {
                        for (int i = 0; i < height; i++, off += stride1)
                        {
                            BitUtils.ResetBits(count, bits, off);
                        }
                    }
                    else if (colorbits == ulong.MaxValue)
                    {
                        for (int i = 0; i < height; i++, off += stride1)
                        {
                            BitUtils.SetBits(count, bits, off);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < height; i++, off += stride1)
                        {
                            BitUtils.SetBits(count, colorbits, bits, off);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to the specified color.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <param name="color">The color to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(Rectangle rect, uint color) =>
            this.SetColor(rect.X, rect.Y, rect.Width, rect.Height, color);

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the specified color.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        public void SetBorder(int x, int y, int width, int height, BorderType borderType, uint borderValue)
        {
            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                // nothing to set
                return;
            }

            if (borderType == BorderType.BorderRepl)
            {
                if (width == 0 || height == 0)
                {
                    // if border occupies entire image - set entire image to color of the pixel at specified origin
                    this.SetColor(this.GetPixel(x, y));
                    return;
                }

                ulong[] bits = this.Bits;
                int stride = this.Stride;
                int stride1 = this.Stride1;
                int bitsPerPixel = this.BitsPerPixel;

                // fill left side
                if (x > 0)
                {
                    // sets all pixels on the left to first row pixel within the area
                    int count = x * bitsPerPixel;
                    for (int i = y, ii = y + height, pos = y * stride1; i < ii; i++, pos += stride1)
                    {
                        ulong ucolor = this.ColorBits(this.GetPixel(x, i));
                        BitUtils.SetBits(count, ucolor, bits, pos);
                    }
                }

                // fill right side
                if (x + width < this.Width)
                {
                    // sets all pixels on the left to last row pixel within the area
                    int x2 = x + width;
                    int count = (this.Width - x2) * bitsPerPixel;
                    for (int i = y, ii = y + height, pos = (y * stride1) + (x2 * bitsPerPixel); i < ii; i++, pos += stride1)
                    {
                        ulong ucolor = this.ColorBits(this.GetPixel(x2 - 1, i));
                        BitUtils.SetBits(count, ucolor, bits, pos);
                    }
                }

                // fill top
                if (y > 0)
                {
                    // copy first area row to all rows above
                    Vectors.Tile(stride, y, bits, y * stride, bits, 0);
                }

                // fill bottom
                if (y + height < this.Height)
                {
                    // copy last area row to all rows below
                    int y2 = y + height;
                    Vectors.Tile(stride, this.Height - y2, bits, (y2 - 1) * stride, bits, y2 * stride);
                }
            }
            else if (borderType == BorderType.BorderConst)
            {
                if (width == 0 || height == 0)
                {
                    // if border occupies entire image - set entire image to specified color
                    this.SetColor(borderValue);
                    return;
                }

                ulong[] bits = this.Bits;
                int stride1 = this.Stride1;
                int bitsPerPixel = this.BitsPerPixel;
                ulong color = this.ColorBits(borderValue);

                int x2 = x + width;
                int y2 = y + height;

                // fill top area and the left part of first partial stride
                int count = (y * stride1) + (x * bitsPerPixel);
                int pos = 0;
                if (count > 0)
                {
                    BitUtils.SetBits(count, color, bits, pos);
                }

                // fill partial strides (together right part and left part of the next line)
                if (height > 1)
                {
                    count = stride1 - (width * bitsPerPixel);
                    if (count > 0)
                    {
                        pos = (y * stride1) + (x2 * bitsPerPixel);
                        for (int i = 1; i < height; i++, pos += stride1)
                        {
                            BitUtils.SetBits(count, color, bits, pos);
                        }
                    }
                }

                // fill bottom area and the right part of last partial stride
                pos = ((y2 - 1) * stride1) + (x2 * bitsPerPixel);
                count = (this.Height * stride1) - pos;
                if (count > 0)
                {
                    BitUtils.SetBits(count, color, bits, pos);
                }
            }
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the specified color.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBorder(Rectangle rect, BorderType borderType, uint borderValue) =>
            this.SetBorder(rect.X, rect.Y, rect.Width, rect.Height, borderType, borderValue);
    }
}
