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
            dst = this.Copy(dst);

            color &= dst.MaxColor;
            if (color != 0)
            {
                if (dst.BitsPerPixel == dst.MaxColor)
                {
                    dst.SetToOne(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bits = &dst.Bits[y * dst.Stride])
                        {
                            switch (dst.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        int stride8 = dst.Stride8;
                                        if (x == 0 && width == dst.Width)
                                        {
                                            Vectors.MaxC(height * stride8, (byte)color, (byte*)bits);
                                        }
                                        else
                                        {
                                            for (int i = 0, off = x; i < height; i++, off += stride8)
                                            {
                                                Vectors.MaxC(width, (byte)color, (byte*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                case 16:
                                    {
                                        int stride16 = dst.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == dst.Width)
                                        {
                                            Vectors.MaxC(height * stride16, (ushort)color, (ushort*)bits);
                                        }
                                        else
                                        {
                                            for (int i = 0, off = x; i < height; i++, off += stride16)
                                            {
                                                Vectors.MaxC(width, (ushort)color, (ushort*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                case 24:
                                case 32:
                                    {
                                        int stride8 = dst.Stride8;

                                        ulong[] colors = dst.ColorScanline(Image.CalculateStride(width, dst.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            int bytesPerPixel = dst.BitsPerPixel / 8;
                                            for (int i = 0, off = x * bytesPerPixel, count = width * bytesPerPixel; i < height; i++, off += stride8)
                                            {
                                                Vectors.Max(count, (byte*)mask, (byte*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                default:
                                    throw new NotSupportedException(string.Format(
                                        CultureInfo.InvariantCulture,
                                        Properties.Resources.E_UnsupportedDepth,
                                        dst.BitsPerPixel));
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
            dst = this.Copy(dst);

            color &= dst.MaxColor;
            if (color != dst.MaxColor)
            {
                if (dst.BitsPerPixel == 0)
                {
                    dst.SetToZero(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bits = &dst.Bits[y * dst.Stride])
                        {
                            switch (dst.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        int stride8 = dst.Stride8;
                                        if (x == 0 && width == dst.Width)
                                        {
                                            Vectors.MinC(height * stride8, (byte)color, (byte*)bits);
                                        }
                                        else
                                        {
                                            for (int i = 0, off = x; i < height; i++, off += stride8)
                                            {
                                                Vectors.MinC(width, (byte)color, (byte*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                case 16:
                                    {
                                        int stride16 = dst.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == dst.Width)
                                        {
                                            Vectors.MinC(height * stride16, (ushort)color, (ushort*)bits);
                                        }
                                        else
                                        {
                                            for (int i = 0, off = x; i < height; i++, off += stride16)
                                            {
                                                Vectors.MinC(width, (ushort)color, (ushort*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                case 24:
                                case 32:
                                    {
                                        ulong[] colors = dst.ColorScanline(Image.CalculateStride(width, dst.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            int stride8 = dst.Stride8;
                                            int bytesPerPixel = dst.BitsPerPixel / 8;
                                            for (int i = 0, off = x * bytesPerPixel, count = width * bytesPerPixel; i < height; i++, off += stride8)
                                            {
                                                Vectors.Min(count, (byte*)mask, (byte*)bits + off);
                                            }
                                        }
                                    }

                                    break;

                                default:
                                    throw new NotSupportedException(string.Format(
                                        CultureInfo.InvariantCulture,
                                        Properties.Resources.E_UnsupportedDepth,
                                        dst.BitsPerPixel));
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
        public Image Maximum(Image dst, Image src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            return this.Maximum(dst, 0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
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
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Maximum(Image dst, int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            int bitsPerPixel = this.BitsPerPixel;
            if (bitsPerPixel == 1)
            {
                return this.Or(dst, x, y, width, height, src, xsrc, ysrc);
            }

            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
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

            dst = this.Copy(dst);

            int stridesrc8 = src.Stride8;
            int stridedst8 = dst.Stride8;
            unsafe
            {
                fixed (ulong* pbitssrc = src.Bits, pbitsdst = dst.Bits)
                {
                    byte* bitssrc = (byte*)pbitssrc + (ysrc * stridesrc8) + (xsrc * bitsPerPixel / 8);
                    byte* bitsdst = (byte*)pbitsdst + (y * stridedst8) + (x * bitsPerPixel / 8);

                    if (x == 0 && xsrc == 0 && stridesrc8 == stridedst8 && width == dst.Width)
                    {
                        // operation is performed on entire pixel line
                        // do all lines at once
                        Vectors.Max(stridesrc8 * height, bitssrc, bitsdst);
                    }
                    else
                    {
                        int count = width * bitsPerPixel / 8;
                        for (int iy = 0; iy < height; iy++)
                        {
                            Vectors.Max(count, bitssrc, bitsdst);

                            bitssrc += stridesrc8;
                            bitsdst += stridedst8;
                        }
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="dstBounds">The location and dimensions of the destination rectangle.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="srcOrigin">The x- and y-coordinates of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="dstBounds"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="srcOrigin"/> and <paramref name="dstBounds"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Maximum(Image dst, Rectangle dstBounds, Image src, Point srcOrigin) =>
            this.Maximum(dst, dstBounds.X, dstBounds.Y, dstBounds.Width, dstBounds.Height, src, srcOrigin.X, srcOrigin.Y);

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
        public Image Minimum(Image dst, Image src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            return this.Minimum(dst, 0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
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
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Minimum(Image dst, int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            int bitsPerPixel = this.BitsPerPixel;
            if (bitsPerPixel == 1)
            {
                return this.And(dst, x, y, width, height, src, xsrc, ysrc);
            }

            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
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

            dst = this.Copy(dst);

            int stridesrc8 = src.Stride8;
            int stridedst8 = dst.Stride8;
            unsafe
            {
                fixed (ulong* pbitssrc = src.Bits, pbitsdst = dst.Bits)
                {
                    byte* bitssrc = (byte*)pbitssrc + (ysrc * stridesrc8) + (xsrc * bitsPerPixel / 8);
                    byte* bitsdst = (byte*)pbitsdst + (y * stridedst8) + (x * bitsPerPixel / 8);

                    if (x == 0 && xsrc == 0 && stridesrc8 == stridedst8 && width == dst.Width)
                    {
                        // operation is performed on entire pixel line
                        // do all lines at once
                        Vectors.Min(stridesrc8 * height, bitssrc, bitsdst);
                    }
                    else
                    {
                        int count = width * bitsPerPixel / 8;
                        for (int iy = 0; iy < height; iy++)
                        {
                            Vectors.Min(count, bitssrc, bitsdst);

                            bitssrc += stridesrc8;
                            bitsdst += stridedst8;
                        }
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="dstBounds">The location and dimensions of the destination rectangle.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="srcOrigin">The x- and y-coordinates of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The depth of <paramref name="src"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="dstBounds"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="srcOrigin"/> and <paramref name="dstBounds"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Minimum(Image dst, Rectangle dstBounds, Image src, Point srcOrigin) =>
            this.Minimum(dst, dstBounds.X, dstBounds.Y, dstBounds.Width, dstBounds.Height, src, srcOrigin.X, srcOrigin.Y);
    }
}