﻿
// -----------------------------------------------------------------------
// <copyright file="Logical.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 9/7/2018 5:57:58 PM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
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
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
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
        public Image And(Image src)
        {
            Image dst = this.Clone(true);
            dst.AndIP(src);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
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
        public Image And(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.AndIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void AndIP(Image src)
        {
            this.AndIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Performs logical AND operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void AndIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
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

            int stridesrc = src.Stride1;
            int stridedst = this.Stride1;
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = this.Bits;

            int possrc = (ysrc * stridesrc) + (xsrc * this.BitsPerPixel);
            int posdst = (y * stridedst) + (x * this.BitsPerPixel);

            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
            {
                // operation is performed on entire pixel line
                // do all lines at once
                Arrays.And(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils.And(count, bitssrc, possrc, bitsdst, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical OR operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
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
        public Image Or(Image src)
        {
            Image dst = this.Clone(true);
            dst.OrIP(src);
            return dst;
        }

        /// <summary>
        /// Performs logical OR operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
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
        public Image Or(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.OrIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Performs logical OR operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void OrIP(Image src)
        {
            this.OrIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Performs logical OR operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void OrIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
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

            int stridesrc = src.Stride1;
            int stridedst = this.Stride1;
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = this.Bits;

            int possrc = (ysrc * stridesrc) + (xsrc * this.BitsPerPixel);
            int posdst = (y * stridedst) + (x * this.BitsPerPixel);

            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
            {
                // operation is performed on entire pixel line
                // do all lines at once
                Arrays.Or(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils.Or(count, bitssrc, possrc, bitsdst, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical XOR operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
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
        public Image Xor(Image src)
        {
            Image dst = this.Clone(true);
            dst.XorIP(src);
            return dst;
        }

        /// <summary>
        /// Performs logical XOR operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
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
        public Image Xor(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.XorIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Performs logical XOR operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void XorIP(Image src)
        {
            this.XorIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Performs logical XOR operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void XorIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
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

            int stridesrc = src.Stride1;
            int stridedst = this.Stride1;
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = this.Bits;

            int possrc = (ysrc * stridesrc) + (xsrc * this.BitsPerPixel);
            int posdst = (y * stridedst) + (x * this.BitsPerPixel);

            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
            {
                // operation is performed on entire pixel line
                // do all lines at once
                Arrays.Xor(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils.Xor(count, bitssrc, possrc, bitsdst, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical XAND operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="src">The right-side operand of this operation.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// </remarks>
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
        public Image Xand(Image src)
        {
            Image dst = this.Clone(true);
            dst.XandIP(src);
            return dst;
        }

        /// <summary>
        /// Performs logical XAND operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
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
        public Image Xand(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
            Image dst = this.Clone(true);
            dst.XandIP(x, y, width, height, src, xsrc, ysrc);
            return dst;
        }

        /// <summary>
        /// Performs logical XAND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void XandIP(Image src)
        {
            this.XandIP(0, 0, Math.Min(this.Width, src.Width), Math.Min(this.Height, src.Height), src, 0, 0);
        }

        /// <summary>
        /// Performs logical XAND operation on a rectangular block of pixels from this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
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
        public void XandIP(int x, int y, int width, int height, Image src, int xsrc, int ysrc)
        {
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

            int stridesrc = src.Stride1;
            int stridedst = this.Stride1;
            ulong[] bitssrc = src.Bits;
            ulong[] bitsdst = this.Bits;

            int possrc = (ysrc * stridesrc) + (xsrc * this.BitsPerPixel);
            int posdst = (y * stridedst) + (x * this.BitsPerPixel);

            if (x == 0 && xsrc == 0 && stridesrc == stridedst && width == this.Width)
            {
                // operation is performed on entire pixel line
                // do all lines at once
                Arrays.Xand(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils.Xand(count, bitssrc, possrc, bitsdst, posdst);
                }
            }
        }
	}
}