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
        /// Inverts all pixels in this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <returns>
        /// A new inverted <see cref="Image"/>.
        /// </returns>
        public Image Not()
        {
            Image dst = this.Clone(false);
            Vectors.Not(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Inverts all pixels in this <see cref="Image"/> in-place.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NotIP() => Vectors.Not(this.Bits.Length, this.Bits, 0);

        /// <summary>
        /// Computes a larger of each pixel value and a constant value of this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/> this method creates.
        /// </returns>
        [CLSCompliant(false)]
        public Image MaxC(uint color)
        {
            Image dst = this.Clone(true);
            dst.MaxCIP(color);
            return dst;
        }

        /// <summary>
        /// Computes a larger of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image MaxC(int x, int y, int width, int height, uint color)
        {
            Image dst = this.Clone(true);
            dst.MaxCIP(x, y, width, height, color);
            return dst;
        }

        /// <summary>
        /// Computes a larger of each pixel value and a constant value of this <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="color">The constant value.</param>
        [CLSCompliant(false)]
        public void MaxCIP(uint color)
        {
            this.MaxCIP(0, 0, this.Width, this.Height, color);
        }

        /// <summary>
        /// Computes a larger of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        [CLSCompliant(false)]
        public void MaxCIP(int x, int y, int width, int height, uint color)
        {
            this.ValidateArea(x, y, width, height);

            color &= this.MaxColor;
            if (color != 0)
            {
                if (this.BitsPerPixel == this.MaxColor)
                {
                    this.SetToOneIP(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bits = &this.Bits[y * this.Stride])
                        {
                            switch (this.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        int stride8 = this.Stride8;
                                        if (x == 0 && width == this.Width)
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
                                        int stride16 = this.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == this.Width)
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
                                        int stride8 = this.Stride8;

                                        ulong[] colors = this.ColorScanline(Image.CalculateStride(width, this.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            int bytesPerPixel = this.BitsPerPixel / 8;
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
                                        this.BitsPerPixel));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value of this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/> this method creates.
        /// </returns>
        [CLSCompliant(false)]
        public Image MinC(uint color)
        {
            Image dst = this.Clone(true);
            dst.MinCIP(color);
            return dst;
        }

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <returns>
        /// The destination <see cref="Image"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image MinC(int x, int y, int width, int height, uint color)
        {
            Image dst = this.Clone(true);
            dst.MinCIP(x, y, width, height, color);
            return dst;
        }

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value of this <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="color">The constant value.</param>
        [CLSCompliant(false)]
        public void MinCIP(uint color)
        {
            this.MinCIP(0, 0, this.Width, this.Height, color);
        }

        /// <summary>
        /// Computes a smaller of each pixel value and a constant value in a rectangular block of pixels of this <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="color">The constant value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// </exception>
        [CLSCompliant(false)]
        public void MinCIP(int x, int y, int width, int height, uint color)
        {
            this.ValidateArea(x, y, width, height);

            color &= this.MaxColor;
            if (color != this.MaxColor)
            {
                if (this.BitsPerPixel == 0)
                {
                    this.SetToZeroIP(x, y, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (ulong* bits = &this.Bits[y * this.Stride])
                        {
                            switch (this.BitsPerPixel)
                            {
                                case 8:
                                    {
                                        int stride8 = this.Stride8;
                                        if (x == 0 && width == this.Width)
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
                                        int stride16 = this.Stride8 / sizeof(ushort);
                                        if (x == 0 && width == this.Width)
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
                                        ulong[] colors = this.ColorScanline(Image.CalculateStride(width, this.BitsPerPixel), color);
                                        fixed (ulong* mask = colors)
                                        {
                                            int stride8 = this.Stride8;
                                            int bytesPerPixel = this.BitsPerPixel / 8;
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
                                        this.BitsPerPixel));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes maximum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image Maximum(Image src)
        {
            Image dst = this.Clone(true);
            dst.MaximumIP(src);
            return dst;
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public Image Maximum(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.MaximumIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Computes maximum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void MaximumIP(Image src)
        {
            this.MaximumIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Computes maximum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public void MaximumIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            switch (this.BitsPerPixel)
            {
                case 1:
                    this.OrIP(x, y, width, height, src, xsrc, ysrc);
                    break;

                case 8:
                case 24:
                case 32:
                    if (src == null)
                    {
                        throw new ArgumentNullException(nameof(src));
                    }

                    this.ValidateArea(x, y, width, height);
                    src.ValidateArea(xsrc, ysrc, width, height);

                    if (this.BitsPerPixel != src.BitsPerPixel)
                    {
                        throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
                    }

                    int stridesrc8 = src.Stride8;
                    int stridedst8 = this.Stride8;
                    unsafe
                    {
                        fixed (ulong* pbitssrc = src.Bits, pbitsdst = this.Bits)
                        {
                            byte* bitssrc = (byte*)pbitssrc + (ysrc * stridesrc8) + (xsrc * this.BitsPerPixel / 8);
                            byte* bitsdst = (byte*)pbitsdst + (y * stridedst8) + (x * this.BitsPerPixel / 8);

                            if (x == 0 && xsrc == 0 && stridesrc8 == stridedst8 && width == this.Width)
                            {
                                // operation is performed on entire pixel line
                                // do all lines at once
                                Vectors.Max(stridesrc8 * height, bitssrc, bitsdst);
                            }
                            else
                            {
                                int count = width * this.BitsPerPixel / 8;
                                for (int iy = 0; iy < height; iy++)
                                {
                                    Vectors.Max(count, bitssrc, bitsdst);

                                    bitssrc += stridesrc8;
                                    bitsdst += stridedst8;
                                }
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

        /// <summary>
        /// Computes minimum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image Minimum(Image src)
        {
            Image dst = this.Clone(true);
            dst.MinimumIP(src);
            return dst;
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the destination rectangle.</param>
        /// <param name="width">The width of the source and destination rectangles.</param>
        /// <param name="height">The height of the source and destination rectangles.</param>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <param name="xsrc">The x-coordinate of the upper-left corner of the source rectangle.</param>
        /// <param name="ysrc">The y-coordinate of the upper-left corner of the source rectangle.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public Image Minimum(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.MinimumIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Computes minimum values for each pixel from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="src"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void MinimumIP(Image src)
        {
            this.MinimumIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Computes minimum values for each pixel in a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.</para>
        /// <para>-or-</para>
        /// <para>The rectangular area described by <paramref name="xsrc"/>, <paramref name="ysrc"/>, <paramref name="width"/> and <paramref name="height"/> is outside of <paramref name="src"/> bounds.</para>
        /// </exception>
        public void MinimumIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            switch (this.BitsPerPixel)
            {
                case 1:
                    this.AndIP(x, y, width, height, src, xsrc, ysrc);
                    break;

                case 8:
                case 24:
                case 32:
                    if (src == null)
                    {
                        throw new ArgumentNullException(nameof(src));
                    }

                    this.ValidateArea(x, y, width, height);
                    src.ValidateArea(xsrc, ysrc, width, height);

                    if (this.BitsPerPixel != src.BitsPerPixel)
                    {
                        throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
                    }

                    int stridesrc8 = src.Stride8;
                    int stridedst8 = this.Stride8;
                    unsafe
                    {
                        fixed (ulong* pbitssrc = src.Bits, pbitsdst = this.Bits)
                        {
                            byte* bitssrc = (byte*)pbitssrc + (ysrc * stridesrc8) + (xsrc * this.BitsPerPixel / 8);
                            byte* bitsdst = (byte*)pbitsdst + (y * stridedst8) + (x * this.BitsPerPixel / 8);

                            if (x == 0 && xsrc == 0 && stridesrc8 == stridedst8 && width == this.Width)
                            {
                                // operation is performed on entire pixel line
                                // do all lines at once
                                Vectors.Min(stridesrc8 * height, bitssrc, bitsdst);
                            }
                            else
                            {
                                int count = width * this.BitsPerPixel / 8;
                                for (int iy = 0; iy < height; iy++)
                                {
                                    Vectors.Min(count, bitssrc, bitsdst);

                                    bitssrc += stridesrc8;
                                    bitsdst += stridedst8;
                                }
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