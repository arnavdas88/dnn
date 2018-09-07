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
    /// Provides logical extension methods for the <see cref="Image"/> class.
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
                Arrays.AND(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.BitsAND(count, bitssrc, possrc, bitsdst, posdst);
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
                Arrays.OR(stridesrc * height / 64, bitssrc, possrc / 64, bitsdst, posdst / 64);
            }
            else
            {
                int count = width * this.BitsPerPixel;
                for (int iy = 0; iy < height; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.OR(count, bitssrc, possrc, bitsdst, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical XOR operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image XOR(Image op)
        {
            Image dst = this.Clone(true);
            dst.XORIP(op);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void XORIP(Image op)
        {
            if (op == null)
            {
                throw new ArgumentNullException(nameof(op));
            }

            if (this.BitsPerPixel != op.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int minheight = Math.Min(this.Height, op.Height);
            if (this.Stride == op.Stride && this.Width <= op.Width)
            {
                Arrays.XOR(this.Stride * minheight, op.Bits, 0, this.Bits, 0);
            }
            else
            {
                int minwidth = Math.Min(this.Width, op.Width);
                int stridesrc = op.Stride1;
                int stridedst = this.Stride1;
                for (int iy = 0, possrc = 0, posdst = 0; iy < minheight; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.XOR(minwidth, op.Bits, possrc, this.Bits, posdst);
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