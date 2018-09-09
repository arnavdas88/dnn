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
        public Image NOT()
        {
            Image dst = this.Clone(false);
            BitUtils64.WordsNOT(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Inverts all pixels in this <see cref="Image"/> in-place.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NOTIP() => BitUtils64.WordsNOT(this.Bits.Length, this.Bits, 0);

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
            this.MaximumIP(0, 0, Core.MinMax.Min(this.Width, src.Width), Core.MinMax.Min(this.Height, src.Height), src, 0, 0);
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

                    int stridesrc = src.Stride8;
                    int stridedst = this.Stride8;
                    unsafe
                    {
                        fixed (ulong* pbitssrc = src.Bits, pbitsdst = this.Bits)
                        {
                            IntPtr bitssrc = new IntPtr(pbitssrc);
                            IntPtr bitsdst = new IntPtr(pbitsdst);

                            bitssrc = IntPtr.Add(bitssrc, (ysrc * stridesrc) + (xsrc * this.BitsPerPixel / 8));
                            bitsdst = IntPtr.Add(bitsdst, (y * stridedst) + (x * this.BitsPerPixel / 8));

                            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
                            {
                                // operation is performed on entire pixel line
                                // do all lines at once
                                Math8u.Max(stridesrc * height, bitssrc, bitsdst, bitsdst);
                            }
                            else
                            {
                                int count = width * this.BitsPerPixel / 8;
                                for (int iy = 0; iy < height; iy++)
                                {
                                    Math8u.Max(count, bitssrc, bitsdst, bitsdst);

                                    bitssrc = IntPtr.Add(bitssrc, stridesrc);
                                    bitsdst = IntPtr.Add(bitsdst, stridedst);
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
            this.MinimumIP(0, 0, Core.MinMax.Min(this.Width, src.Width), Core.MinMax.Min(this.Height, src.Height), src, 0, 0);
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

                    int stridesrc = src.Stride8;
                    int stridedst = this.Stride8;
                    unsafe
                    {
                        fixed (ulong* pbitssrc = src.Bits, pbitsdst = this.Bits)
                        {
                            IntPtr bitssrc = new IntPtr(pbitssrc);
                            IntPtr bitsdst = new IntPtr(pbitsdst);

                            bitssrc = IntPtr.Add(bitssrc, (ysrc * stridesrc) + (xsrc * this.BitsPerPixel / 8));
                            bitsdst = IntPtr.Add(bitsdst, (y * stridedst) + (x * this.BitsPerPixel / 8));

                            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
                            {
                                // operation is performed on entire pixel line
                                // do all lines at once
                                Math8u.Min(stridesrc * height, bitssrc, bitsdst, bitsdst);
                            }
                            else
                            {
                                int count = width * this.BitsPerPixel / 8;
                                for (int iy = 0; iy < height; iy++)
                                {
                                    Math8u.Min(count, bitssrc, bitsdst, bitsdst);

                                    bitssrc = IntPtr.Add(bitssrc, stridesrc);
                                    bitsdst = IntPtr.Add(bitsdst, stridedst);
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