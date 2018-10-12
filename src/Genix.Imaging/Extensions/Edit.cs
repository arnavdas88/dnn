// -----------------------------------------------------------------------
// <copyright file="Edit.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
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
        /// Gets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="point">The x- and y-coordinate of the pixel to retrieve.</param>
        /// <returns>
        /// The color of the specified pixel.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The pixel coordinates specified by <paramref name="point"/> is outside this <see cref="Image"/> bounds.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint GetPixel(Point point) => this.GetPixel(point.X, point.Y);

        /// <summary>
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
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
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="point">The x- and y-coordinate of the pixel to set.</param>
        /// <param name="color">The color to assign to the specified pixel.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The pixel coordinates specified by <paramref name="point"/> is outside this <see cref="Image"/> bounds.</para>
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
        public void SetPixel(Point point, uint color) => this.SetPixel(point.X, point.Y, color);

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
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="color">The color to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(Rectangle area, uint color) => this.SetColor(area.X, area.Y, area.Width, area.Height, color);

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

                if (bitsPerPixel == 24)
                {
                    // fill left and right sides
                    int x2 = x + width;
                    if (x > 0 || x2 < this.Width)
                    {
                        unsafe
                        {
                            fixed (ulong* ubits = &this.Bits[y * this.Stride])
                            {
                                byte* bbits = (byte*)ubits;
                                for (int i = 0, count2 = this.Width - x2, stride8 = this.Stride8; i < height; i++, bbits += stride8)
                                {
                                    if (x > 0)
                                    {
                                        Vectors.Tile(3, x, bbits + (x * 3), bbits);
                                    }

                                    if (count2 > 0)
                                    {
                                        Vectors.Tile(3, count2, bbits + (x2 * 3) - 3, bbits + (x2 * 3));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
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
                    int x2 = x + width;
                    if (x2 < this.Width)
                    {
                        // sets all pixels on the left to last row pixel within the area
                        int count = (this.Width - x2) * bitsPerPixel;
                        for (int i = y, ii = y + height, pos = (y * stride1) + (x2 * bitsPerPixel); i < ii; i++, pos += stride1)
                        {
                            ulong ucolor = this.ColorBits(this.GetPixel(x2 - 1, i));
                            BitUtils.SetBits(count, ucolor, bits, pos);
                        }
                    }
                }

                // fill top
                if (y > 0)
                {
                    // copy first area row to all rows above
                    Vectors.Tile(stride, y, bits, y * stride, bits, 0);
                }

                // fill bottom
                int y2 = y + height;
                if (y2 < this.Height)
                {
                    // copy last area row to all rows below
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

                int bitsPerPixel = this.BitsPerPixel;
                if (bitsPerPixel == 24)
                {
                    ulong[] colors = this.ColorScanline(Image.CalculateStride(this.Width, 24), borderValue);

                    // fill top
                    if (y > 0)
                    {
                        Vectors.Tile(this.Stride, y, colors, 0, this.Bits, 0);
                    }

                    // fill left and right
                    int x2 = x + width;
                    if (x > 0 || x2 < this.Width)
                    {
                        unsafe
                        {
                            fixed (ulong* bits = &this.Bits[y * this.Stride], ucolors = colors)
                            {
                                byte* bbits = (byte*)bits;
                                byte* bcolors = (byte*)ucolors;

                                for (int i = 0, count1 = x * 3, count2 = (this.Width - x2) * 3, stride8 = this.Stride8; i < height; i++, bbits += stride8)
                                {
                                    if (count1 > 0)
                                    {
                                        Vectors.Copy(count1, bcolors, bbits);
                                    }

                                    if (count2 > 0)
                                    {
                                        Vectors.Copy(count2, bcolors + (x2 * 3), bbits + (x2 * 3));
                                    }
                                }
                            }
                        }
                    }

                    // fill bottom
                    int y2 = y + height;
                    if (y2 < this.Height)
                    {
                        Vectors.Tile(this.Stride, this.Height - y2, colors, 0, this.Bits, y2 * this.Stride);
                    }
                }
                else
                {
                    ulong[] bits = this.Bits;
                    int stride1 = this.Stride1;
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
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the specified color.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBorder(Rectangle area, BorderType borderType, uint borderValue) => this.SetBorder(area.X, area.Y, area.Width, area.Height, borderType, borderValue);

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the larger of specified color and their current values.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="borderValue">The value of border pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        public void MaxCBorder(int x, int y, int width, int height, uint borderValue)
        {
            borderValue &= this.MaxColor;
            if (borderValue == 0)
            {
                // nothing to set
                return;
            }

            if (borderValue == this.MaxColor)
            {
                // set to maximum value
                this.SetBorder(x, y, width, height, BorderType.BorderConst, borderValue);
                return;
            }

            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                // nothing to set
                return;
            }

            if (width == 0 || height == 0)
            {
                // if border occupies entire image - max entire image with the specified color
                this.MaxC(this, borderValue);
                return;
            }

            unsafe
            {
                fixed (ulong* bits = this.Bits)
                {
                    byte* ptr = (byte*)bits;
                    int stride8 = this.Stride8;

                    int x2 = x + width;
                    int y2 = y + height;

                    int bitsPerPixel = this.BitsPerPixel;
                    switch (bitsPerPixel)
                    {
                        case 8:
                            // fill top area and the left part of first partial stride
                            if (y > 0 || x > 0)
                            {
                                Vectors.MaxC((y * stride8) + x, (byte)borderValue, ptr);
                            }

                            // fill partial strides (together right part and left part of the next line)
                            if (height > 1 && width < this.Width)
                            {
                                byte* ptr2 = ptr + (y * stride8) + x2;
                                for (int i = 1, count = stride8 - width; i < height; i++, ptr2 += stride8)
                                {
                                    Vectors.MaxC(count, (byte)borderValue, ptr2);
                                }
                            }

                            // fill bottom area and the right part of last partial stride
                            if (y2 < this.Height || x2 < this.Width)
                            {
                                int off = ((y2 - 1) * stride8) + x2;
                                int count = (this.Height * stride8) - off;
                                Vectors.MaxC(count, (byte)borderValue, ptr + off);
                            }

                            break;

                        case 16:
                            int stride16 = stride8 / sizeof(ushort);
                            ushort* usptr = (ushort*)bits;

                            // fill top area and the left part of first partial stride
                            if (y > 0 || x > 0)
                            {
                                Vectors.MaxC((y * stride16) + x, (ushort)borderValue, usptr);
                            }

                            // fill partial strides (together right part and left part of the next line)
                            if (height > 1 && width < this.Width)
                            {
                                ushort* usptr2 = usptr + (y * stride16) + x2;
                                for (int i = 1, count = stride16 - width; i < height; i++, usptr2 += stride16)
                                {
                                    Vectors.MaxC(count, (ushort)borderValue, usptr2);
                                }
                            }

                            // fill bottom area and the right part of last partial stride
                            if (y2 < this.Height || x2 < this.Width)
                            {
                                int off = ((y2 - 1) * stride16) + x2;
                                int count = (this.Height * stride16) - off;
                                Vectors.MaxC(count, (ushort)borderValue, usptr + off);
                            }

                            break;

                        case 24:
                        case 32:
                            fixed (ulong* mask = this.ColorScanline(this.Stride, borderValue))
                            {
                                int i = 0;

                                // fill top area
                                for (; i < y; i++, ptr += stride8)
                                {
                                    Vectors.Max(stride8, (byte*)mask, ptr);
                                }

                                // fill left and right
                                if (width == this.Width)
                                {
                                    i += height;
                                    ptr += height * stride8;
                                }
                                else
                                {
                                    for (int lcount = x * bitsPerPixel / 8, rcount = (this.Width - x2) * bitsPerPixel / 8, roff = x2 * bitsPerPixel / 8; i < y2; i++, ptr += stride8)
                                    {
                                        if (lcount > 0)
                                        {
                                            Vectors.Max(lcount, (byte*)mask, ptr);
                                        }

                                        if (rcount > 0)
                                        {
                                            Vectors.Max(rcount, (byte*)mask + roff, ptr + roff);
                                        }
                                    }
                                }

                                // fill bottom area
                                for (int ii = this.Height; i < ii; i++, ptr += stride8)
                                {
                                    Vectors.Max(stride8, (byte*)mask, ptr);
                                }
                            }

                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the larger of specified color and their current values.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="borderValue">The value of border pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MaxCBorder(Rectangle area, uint borderValue) => this.MaxCBorder(area.X, area.Y, area.Width, area.Height, borderValue);

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the smaller of specified color and their current values.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="borderValue">The value of border pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        public void MinCBorder(int x, int y, int width, int height, uint borderValue)
        {
            borderValue &= this.MaxColor;
            if (borderValue == this.MaxColor)
            {
                // nothing to set
                return;
            }

            if (borderValue == 0)
            {
                // set to maximum value
                this.SetBorder(x, y, width, height, BorderType.BorderConst, borderValue);
                return;
            }

            this.ValidateArea(x, y, width, height);

            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                // nothing to set
                return;
            }

            if (width == 0 || height == 0)
            {
                // if border occupies entire image - max entire image with the specified color
                this.MinC(this, borderValue);
                return;
            }

            unsafe
            {
                fixed (ulong* bits = this.Bits)
                {
                    byte* ptr = (byte*)bits;
                    int stride8 = this.Stride8;

                    int x2 = x + width;
                    int y2 = y + height;

                    int bitsPerPixel = this.BitsPerPixel;
                    switch (bitsPerPixel)
                    {
                        case 8:
                            // fill top area and the left part of first partial stride
                            if (y > 0 || x > 0)
                            {
                                Vectors.MinC((y * stride8) + x, (byte)borderValue, ptr);
                            }

                            // fill partial strides (together right part and left part of the next line)
                            if (height > 1 && width < this.Width)
                            {
                                byte* ptr2 = ptr + (y * stride8) + x2;
                                for (int i = 1, count = stride8 - width; i < height; i++, ptr2 += stride8)
                                {
                                    Vectors.MinC(count, (byte)borderValue, ptr2);
                                }
                            }

                            // fill bottom area and the right part of last partial stride
                            if (y2 < this.Height || x2 < this.Width)
                            {
                                int off = ((y2 - 1) * stride8) + x2;
                                int count = (this.Height * stride8) - off;
                                Vectors.MinC(count, (byte)borderValue, ptr + off);
                            }

                            break;

                        case 16:
                            int stride16 = stride8 / sizeof(ushort);
                            ushort* usptr = (ushort*)bits;

                            // fill top area and the left part of first partial stride
                            if (y > 0 || x > 0)
                            {
                                Vectors.MinC((y * stride16) + x, (ushort)borderValue, usptr);
                            }

                            // fill partial strides (together right part and left part of the next line)
                            if (height > 1 && width < this.Width)
                            {
                                ushort* usptr2 = usptr + (y * stride16) + x2;
                                for (int i = 1, count = stride16 - width; i < height; i++, usptr2 += stride16)
                                {
                                    Vectors.MinC(count, (ushort)borderValue, usptr2);
                                }
                            }

                            // fill bottom area and the right part of last partial stride
                            if (y2 < this.Height || x2 < this.Width)
                            {
                                int off = ((y2 - 1) * stride16) + x2;
                                int count = (this.Height * stride16) - off;
                                Vectors.MinC(count, (ushort)borderValue, usptr + off);
                            }

                            break;

                        case 24:
                        case 32:
                            fixed (ulong* mask = this.ColorScanline(this.Stride, borderValue))
                            {
                                int i = 0;

                                // fill top area
                                for (; i < y; i++, ptr += stride8)
                                {
                                    Vectors.Min(stride8, (byte*)mask, ptr);
                                }

                                // fill left and right
                                if (width == this.Width)
                                {
                                    i += height;
                                    ptr += height * stride8;
                                }
                                else
                                {
                                    for (int lcount = x * bitsPerPixel / 8, rcount = (this.Width - x2) * bitsPerPixel / 8, roff = x2 * bitsPerPixel / 8; i < y2; i++, ptr += stride8)
                                    {
                                        if (lcount > 0)
                                        {
                                            Vectors.Min(lcount, (byte*)mask, ptr);
                                        }

                                        if (rcount > 0)
                                        {
                                            Vectors.Min(rcount, (byte*)mask + roff, ptr + roff);
                                        }
                                    }
                                }

                                // fill bottom area
                                for (int ii = this.Height; i < ii; i++, ptr += stride8)
                                {
                                    Vectors.Min(stride8, (byte*)mask, ptr);
                                }
                            }

                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to the smaller of specified color and their current values.
        /// </summary>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <param name="borderValue">The value of border pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinCBorder(Rectangle area, uint borderValue) => this.MinCBorder(area.X, area.Y, area.Width, area.Height, borderValue);
    }
}
