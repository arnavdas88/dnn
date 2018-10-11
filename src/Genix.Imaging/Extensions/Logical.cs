// -----------------------------------------------------------------------
// <copyright file="Logical.cs" company="Noname, Inc.">
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
    using Genix.Drawing;

    /// <content>
    /// <para>
    /// Provides logical extension methods for the <see cref="Image"/> class.
    /// </para>
    /// <para>
    ///             +---+---+---+---+
    ///    If A is: | 1 | 0 | 1 | 0 |
    ///   and B is: | 1 | 1 | 0 | 0 |
    ///             +---+---+---+---+
    ///     Then:        yields:
    /// +-----------+---+---+---+---+
    /// | FALSE     | 0 | 0 | 0 | 0 |
    /// | A NOR B   | 0 | 0 | 0 | 1 |
    /// | A XAND B  | 0 | 0 | 1 | 0 |
    /// | NOT B     | 0 | 0 | 1 | 1 |
    /// | B XAND A  | 0 | 1 | 0 | 0 |
    /// | NOT A     | 0 | 1 | 0 | 1 |
    /// | A XOR B   | 0 | 1 | 1 | 0 |
    /// | A NAND B  | 0 | 1 | 1 | 1 |
    /// | A AND B   | 1 | 0 | 0 | 0 |
    /// | A XNOR B  | 1 | 0 | 0 | 1 |
    /// | A         | 1 | 0 | 1 | 0 |
    /// | B XNAND A | 1 | 0 | 1 | 1 |
    /// | B         | 1 | 1 | 0 | 0 |
    /// | A XNAND B | 1 | 1 | 0 | 1 |
    /// | A OR B    | 1 | 1 | 1 | 0 |
    /// | TRUE      | 1 | 1 | 1 | 1 |
    /// +-----------+---+---+---+---+.
    /// </para>
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Inverts all pixels in this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Not(Image dst)
        {
            if (dst == this)
            {
                Vectors.Not(this.Bits.Length, this.Bits, 0);
            }
            else
            {
                dst = this.CreateTemplate(dst, this.BitsPerPixel);
                Vectors.Not(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Computes a larger of each pixel value and a constant value of this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image MaxC(Image dst, uint color) => this.MaxC(dst, 0, 0, this.Width, this.Height, color);

        /// <summary>
        /// Computes a larger of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MaxC(Image dst, int x, int y, int width, int height, uint color)
        {
            this.ValidateArea(x, y, width, height);

            // create a copy of this image
            dst = this.Copy(dst, false);

            color &= this.MaxColor;
            if (color != 0)
            {
                if (color == this.MaxColor)
                {
                    dst.SetToOne(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bitssrc = &this.Bits[y * this.Stride], bitsdst = &dst.Bits[y * dst.Stride])
                        {
                            switch (dst.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        byte* ptrsrc = (byte*)bitssrc + x;
                                        byte* ptrdst = (byte*)bitsdst + x;

                                        int stride8 = this.Stride8;
                                        if (x == 0 && width == this.Width)
                                        {
                                            Vectors.MaxC(height * stride8, ptrsrc, (byte)color, ptrdst);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < height; i++, ptrsrc += stride8, ptrdst += stride8)
                                            {
                                                Vectors.MaxC(width, ptrsrc, (byte)color, ptrdst);
                                            }
                                        }
                                    }

                                    break;

                                case 16:
                                    {
                                        ushort* ptrsrc = (ushort*)bitssrc + x;
                                        ushort* ptrdst = (ushort*)bitsdst + x;

                                        int stride16 = dst.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == this.Width)
                                        {
                                            Vectors.MaxC(height * stride16, ptrsrc, (ushort)color, ptrdst);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < height; i++, ptrsrc += stride16, ptrdst += stride16)
                                            {
                                                Vectors.MaxC(width, ptrsrc, (ushort)color, ptrdst);
                                            }
                                        }
                                    }

                                    break;

                                case 24:
                                case 32:
                                    {
                                        int bytesPerPixel = this.BitsPerPixel / 8;
                                        byte* ptrsrc = (byte*)bitssrc + (x * bytesPerPixel);
                                        byte* ptrdst = (byte*)bitsdst + (x * bytesPerPixel);

                                        int stride8 = dst.Stride8;

                                        ulong[] colors = dst.ColorScanline(Image.CalculateStride(width, this.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            for (int i = 0, count = width * bytesPerPixel; i < height; i++, ptrsrc += stride8, ptrdst += stride8)
                                            {
                                                Vectors.Max(count, ptrsrc, (byte*)mask, ptrdst);
                                            }
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
            }

            return dst;
        }

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value of this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public Image MinC(Image dst, uint color) => this.MinC(dst, 0, 0, this.Width, this.Height, color);

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MinC(Image dst, int x, int y, int width, int height, uint color)
        {
            this.ValidateArea(x, y, width, height);

            // create a copy of this image
            dst = this.Copy(dst, false);

            color &= this.MaxColor;
            if (color != this.MaxColor)
            {
                if (color == 0)
                {
                    dst.SetToZero(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bitssrc = &this.Bits[y * this.Stride], bitsdst = &dst.Bits[y * dst.Stride])
                        {
                            switch (dst.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        byte* ptrsrc = (byte*)bitssrc + x;
                                        byte* ptrdst = (byte*)bitsdst + x;

                                        int stride8 = this.Stride8;
                                        if (x == 0 && width == this.Width)
                                        {
                                            Vectors.MinC(height * stride8, ptrsrc, (byte)color, ptrdst);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < height; i++, ptrsrc += stride8, ptrdst += stride8)
                                            {
                                                Vectors.MinC(width, ptrsrc, (byte)color, ptrdst);
                                            }
                                        }
                                    }

                                    break;

                                case 16:
                                    {
                                        ushort* ptrsrc = (ushort*)bitssrc + x;
                                        ushort* ptrdst = (ushort*)bitsdst + x;

                                        int stride16 = dst.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == this.Width)
                                        {
                                            Vectors.MinC(height * stride16, ptrsrc, (ushort)color, ptrdst);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < height; i++, ptrsrc += stride16, ptrdst += stride16)
                                            {
                                                Vectors.MinC(width, ptrsrc, (ushort)color, ptrdst);
                                            }
                                        }
                                    }

                                    break;

                                case 24:
                                case 32:
                                    {
                                        int bytesPerPixel = this.BitsPerPixel / 8;
                                        byte* ptrsrc = (byte*)bitssrc + (x * bytesPerPixel);
                                        byte* ptrdst = (byte*)bitsdst + (x * bytesPerPixel);

                                        int stride8 = dst.Stride8;

                                        ulong[] colors = dst.ColorScanline(Image.CalculateStride(width, this.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            for (int i = 0, count = width * bytesPerPixel; i < height; i++, ptrsrc += stride8, ptrdst += stride8)
                                            {
                                                Vectors.Min(count, ptrsrc, (byte*)mask, ptrdst);
                                            }
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
            }

            return dst;
        }

        /// <summary>
        /// Computes maximum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image MaxEvery(Image dst, Image src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            // copy this image to destination
            dst = this.Copy(dst, true);

            dst.MaxEvery(
                0,
                0,
                Math.Min(this.Width, src.Width),
                Math.Min(this.Height, src.Height),
                src,
                0,
                0);

            return dst;
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public void MaxEvery(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            int bitsPerPixel = this.BitsPerPixel;
            if (bitsPerPixel == 1)
            {
                this.Or(x, y, width, height, src, xsrc, ysrc);
                return;
            }

            this.ValidateArea(x, y, width, height);
            src.ValidateArea(xsrc, ysrc, width, height);

            if (bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    bitsPerPixel));
            }

            if (src.BitsPerPixel != bitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int stride8dst = this.Stride8;
            int stride8src = src.Stride8;
            unsafe
            {
                fixed (ulong* bitsdst = &this.Bits[y * this.Stride],
                              bitssrc = &src.Bits[y * src.Stride])
                {
                    byte* ptrdst = (byte*)bitsdst + (x * bitsPerPixel / 8);
                    byte* ptrsrc = (byte*)bitssrc + (xsrc * bitsPerPixel / 8);

                    if (x == 0 && xsrc == 0 && width == this.Width && stride8src == stride8dst)
                    {
                        // operation is performed on entire area
                        // do all lines at once
                        Vectors.Max(stride8dst * height, ptrsrc, ptrdst);
                    }
                    else
                    {
                        int count = width * bitsPerPixel / 8;
                        for (int iy = 0; iy < height; iy++, ptrsrc += stride8src, ptrdst += stride8dst)
                        {
                            Vectors.Max(count, ptrsrc, ptrdst);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="area">The location and dimensions of the destination rectangle.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="origin">The x- and y-coordinates of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="origin"/> and <paramref name="area"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MaxEvery(Rectangle area, Image src, Point origin) =>
            this.MaxEvery(area.X, area.Y, area.Width, area.Height, src, origin.X, origin.Y);

        /// <summary>
        /// Computes minimum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image MinEvery(Image dst, Image src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            // copy this image to destination
            dst = this.Copy(dst, true);

            dst.MinEvery(
                0,
                0,
                Math.Min(this.Width, src.Width),
                Math.Min(this.Height, src.Height),
                src,
                0,
                0);

            return dst;
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public void MinEvery(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            int bitsPerPixel = this.BitsPerPixel;
            if (bitsPerPixel == 1)
            {
                this.And(x, y, width, height, src, xsrc, ysrc);
                return;
            }

            this.ValidateArea(x, y, width, height);
            src.ValidateArea(xsrc, ysrc, width, height);

            if (bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    bitsPerPixel));
            }

            if (src.BitsPerPixel != bitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int stride8dst = this.Stride8;
            int stride8src = src.Stride8;
            unsafe
            {
                fixed (ulong* bitsdst = &this.Bits[y * this.Stride],
                              bitssrc = &src.Bits[y * src.Stride])
                {
                    byte* ptrdst = (byte*)bitsdst + (x * bitsPerPixel / 8);
                    byte* ptrsrc = (byte*)bitssrc + (xsrc * bitsPerPixel / 8);

                    if (x == 0 && xsrc == 0 && width == this.Width && stride8src == stride8dst)
                    {
                        // operation is performed on entire area
                        // do all lines at once
                        Vectors.Min(stride8dst * height, ptrsrc, ptrdst);
                    }
                    else
                    {
                        int count = width * bitsPerPixel / 8;
                        for (int iy = 0; iy < height; iy++, ptrsrc += stride8src, ptrdst += stride8dst)
                        {
                            Vectors.Min(count, ptrsrc, ptrdst);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="area">The location and dimensions of the destination rectangle.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="origin">The x- and y-coordinates of the upper-left corner of the source rectangle.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="area"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="origin"/> and <paramref name="area"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinEvery(Rectangle area, Image src, Point origin) =>
            this.MinEvery(area.X, area.Y, area.Width, area.Height, src, origin.X, origin.Y);
    }
}