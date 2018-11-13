// -----------------------------------------------------------------------
// <copyright file="Morphology.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <content>
    /// Provides morphology extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Dilates this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Dilation assigns to each pixel a maximum value of all pixels covered by its structuring element.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Dilate(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            if (se == null)
            {
                throw new ArgumentNullException(nameof(se));
            }

            if (iterations <= 0)
            {
                throw new ArgumentException("The number of iterations is equal to or less than zero.");
            }

            Image src = this;
            bool inplace = dst == this;

            Size sesize = se.Size;
            Point anchor = se.GetAnchor(StructuringElement.DefaultAnchor);

            // estimate border size
            int bx1 = Math.Max(anchor.X, 0) * iterations;
            int by1 = Math.Max(anchor.Y, 0) * iterations;
            int bx2 = src.Width - (Math.Max(sesize.Width - 1 - anchor.X, 0) * iterations);
            int by2 = src.Height - (Math.Max(sesize.Height - 1 - anchor.Y, 0) * iterations);

            // extreme case - border is larger than image
            // simply set entire image to the max color
            if (bx2 <= bx1 || by2 <= by1)
            {
                if (borderType == BorderType.BorderRepl)
                {
                    borderValue = this.Max();
                }
                else
                {
                    borderValue &= this.MaxColor;
                    if (borderValue != this.MaxColor)
                    {
                        borderValue = Color.Max(this.Max(), borderValue, this.BitsPerPixel);
                    }
                }

                if (!inplace)
                {
                    dst = this.CreateTemplate(dst, this.BitsPerPixel);
                }

                dst.SetColor(borderValue);
            }
            else
            {
                dst = this.CreateTemplate(dst, this.BitsPerPixel);

                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    // for next iteration, copy destination image back into source
                    if (iteration > 0)
                    {
                        UpdateSource();
                    }

                    // special case for rectangular kernel
                    // instead of applying m x n mask
                    // we sequentially apply m x 1 and 1 x n masks
                    if (se is BrickStructuringElement && sesize.Width > 1 && sesize.Height > 1)
                    {
                        ApplySE(
                            StructuringElement.Brick(
                                sesize.Width,
                                1,
                                se.Anchor == StructuringElement.DefaultAnchor ? StructuringElement.DefaultAnchor : new Point(se.Anchor.X, 0)),
                            iteration == 0);

                        // for next iteration, copy destination image back into source
                        UpdateSource();

                        ApplySE(
                            StructuringElement.Brick(
                                1,
                                sesize.Height,
                                se.Anchor == StructuringElement.DefaultAnchor ? StructuringElement.DefaultAnchor : new Point(0, se.Anchor.Y)),
                            false);
                    }
                    else
                    {
                        ApplySE(se, iteration == 0);
                    }
                }

                // maximize border pixels
                if (borderType == BorderType.BorderConst)
                {
                    dst.MaxCBorder(bx1, by1, bx2 - bx1, by2 - by1, borderValue);
                }

                if (inplace)
                {
                    this.Attach(dst);
                    return this;
                }
            }

            return dst;

            void UpdateSource()
            {
                if (src == this)
                {
                    src = dst.Clone(false);
                }

                Vectors.Copy(dst.Bits.Length, dst.Bits, 0, src.Bits, 0);
            }

            void ApplySE(StructuringElement currentSE, bool copySrcToDst)
            {
                // initialize dst with src if s.e. contains anchor point
                // anchor point will later be excluded from comparison
                if (currentSE.GetElements().Contains(Point.Empty))
                {
                    if (copySrcToDst)
                    {
                        Vectors.Copy(src.Bits.Length, src.Bits, 0, dst.Bits, 0);
                    }
                }
                else
                {
                    dst.SetToZero();
                }

                // use van-Herk/Gil-Werman (vHGW) algorithm for 1 x n and m x 1 kernels
                if (currentSE is BrickStructuringElement brick)
                {
                    // 1. works for s.e.
                    // 2. anchor should be inside the s.e. with at least one pixel on each side
                    // 3. use standard algorithm for small s.e. on binary images (it is faster)
                    if (brick.Width == 1 &&
                        anchor.Y.Between(1, brick.Height - 2) &&
                        (src.BitsPerPixel != 1 || brick.Height >= 5))
                    {
                        ComputeRows_vHGW();
                        return;
                    }

#if false
                    if (src.BitsPerPixel == 8 && brick.Height == 1 && anchor.X.Between(1, brick.Width - 2))
                    {
                        ComputeCols_vHGW();
                        return;
                    }
#endif
                }

                // apply s.e.
                foreach (Point point in currentSE.GetElements())
                {
                    if (point != Point.Empty)
                    {
                        ApplySEPoint(point);
                    }
                }

                void ApplySEPoint(Point point)
                {
                    int xdst = Core.MinMax.Max(-point.X, 0);
                    int ydst = Core.MinMax.Max(-point.Y, 0);
                    int xsrc = Core.MinMax.Max(point.X, 0);
                    int ysrc = Core.MinMax.Max(point.Y, 0);
                    int width = src.Width - Math.Abs(point.X);
                    int height = src.Height - Math.Abs(point.Y);

                    dst.MaxEvery(xdst, ydst, width, height, src, xsrc, ysrc);
                }

                void ComputeRows_vHGW()
                {
                    int height = src.Height;

                    // calculate left and right buffer sizes
                    int lsize = anchor.Y;
                    int rsize = sesize.Height - 1 - lsize;
                    int tailsize = Math.Min(lsize, rsize);
                    int lbufsize = lsize + tailsize;
                    int rbufsize = rsize + tailsize;

                    // allocate buffers
                    int stride = src.Stride;
                    ulong[] lbuf = new ulong[lbufsize * stride];
                    ulong[] rbuf = new ulong[rbufsize * stride];

                    switch (src.BitsPerPixel)
                    {
                        case 1:
                            Compute1();
                            break;

                        case 8:
                        case 24:
                        case 32:
                            Compute8();
                            break;

                        case 16:
                            Compute16();
                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }

                    void Compute1()
                    {
                        ulong[] bitssrc = src.Bits;
                        ulong[] bitsdst = dst.Bits;

                        for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                        {
                            // compute left buffer
                            {
                                int offbits = i * stride;
                                int offbuf = 0;

                                // first buffer
                                if (i < height)
                                {
                                    Vectors.Or(stride, bitssrc, offbits, bitssrc, offbits - stride, lbuf, offbuf);
                                }
                                else if (i == height)
                                {
                                    Vectors.Copy(stride, bitssrc, offbits - stride, lbuf, offbuf);
                                }
                                else
                                {
                                    Vectors.Set(stride, 0, lbuf, offbuf);
                                }

                                offbits -= 2 * stride;
                                offbuf += stride;

                                int ibuf = 1;
                                for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, offbits -= stride, offbuf += stride)
                                {
                                    if (j < height)
                                    {
                                        Vectors.Or(stride, bitssrc, offbits, lbuf, offbuf - stride, lbuf, offbuf);
                                    }
                                    else
                                    {
                                        Vectors.Copy(stride, lbuf, offbuf - stride, lbuf, offbuf);
                                    }
                                }

                                int tilecount = lbufsize - ibuf;
                                if (tilecount > 0)
                                {
                                    Vectors.Tile(stride, tilecount, lbuf, offbuf - stride, lbuf, offbuf);
                                }
                            }

                            // compute right buffer
                            {
                                int offbits = i * stride;
                                int offbuf = 0;

                                if (i >= height)
                                {
                                    Vectors.Set(rbufsize * stride, 0, rbuf, offbuf);
                                }
                                else if (i == height - 1)
                                {
                                    Vectors.Tile(stride, rbufsize, bitssrc, offbits, rbuf, offbuf);
                                }
                                else
                                {
                                    Vectors.Or(stride, bitssrc, offbits, bitssrc, offbits + stride, rbuf, offbuf);
                                    offbits += 2 * stride;
                                    offbuf += stride;

                                    int ibuf = 1;
                                    for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, offbits += stride, offbuf += stride)
                                    {
                                        Vectors.Or(stride, bitssrc, offbits, rbuf, offbuf - stride, rbuf, offbuf);
                                    }

                                    int tilecount = rbufsize - ibuf;
                                    if (tilecount > 0)
                                    {
                                        Vectors.Tile(stride, tilecount, rbuf, offbuf - stride, rbuf, offbuf);
                                    }
                                }
                            }

                            // compute maximums
                            {
                                int offdst = (i - tailsize) * stride;
                                int idxl = lbufsize - 1;
                                int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                int offl = idxl * stride;
                                int offr = idxr * stride;

                                for (int j = -tailsize; j <= tailsize; j++, offdst += stride, idxl--, offl -= stride, idxr++, offr += stride)
                                {
                                    if ((i + j).Between(0, height - 1))
                                    {
                                        if (!idxr.Between(0, rbufsize - 1))
                                        {
                                            Vectors.Copy(stride, lbuf, offl, bitsdst, offdst);
                                        }
                                        else if (!idxl.Between(0, lbufsize - 1))
                                        {
                                            Vectors.Copy(stride, rbuf, offr, bitsdst, offdst);
                                        }
                                        else
                                        {
                                            Vectors.Or(stride, lbuf, offl, rbuf, offr, bitsdst, offdst);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    void Compute8()
                    {
                        int stride8 = src.Stride8;

                        unsafe
                        {
                            fixed (ulong* ubitssrc = src.Bits, ubitsdst = dst.Bits)
                            {
                                fixed (ulong* lbufptr = lbuf, rbufptr = rbuf)
                                {
                                    for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                                    {
                                        byte* bitssrc = (byte*)ubitssrc + (i * stride8);

                                        // compute left buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)lbufptr;

                                            // first buffer
                                            if (i < height)
                                            {
                                                Vectors.Max(stride8, bits, bits - stride8, buf);
                                            }
                                            else if (i == height)
                                            {
                                                Vectors.Copy(stride8, bits - stride8, buf);
                                            }
                                            else
                                            {
                                                Vectors.Set(stride8, 0, buf);
                                            }

                                            bits -= 2 * stride8;
                                            buf += stride8;

                                            int ibuf = 1;
                                            for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, bits -= stride8, buf += stride8)
                                            {
                                                if (j < height)
                                                {
                                                    Vectors.Max(stride8, bits, buf - stride8, buf);
                                                }
                                                else
                                                {
                                                    Vectors.Copy(stride8, buf - stride8, buf);
                                                }
                                            }

                                            int tilecount = lbufsize - ibuf;
                                            if (tilecount > 0)
                                            {
                                                Vectors.Tile(stride8, tilecount, buf - stride8, buf);
                                            }
                                        }

                                        // compute right buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)rbufptr;

                                            if (i >= height)
                                            {
                                                Vectors.Set(rbufsize * stride8, 0, buf);
                                            }
                                            else if (i == height - 1)
                                            {
                                                Vectors.Tile(stride8, rbufsize, bits, buf);
                                            }
                                            else
                                            {
                                                Vectors.Max(stride8, bits, bits + stride8, buf);
                                                bits += 2 * stride8;
                                                buf += stride8;

                                                int ibuf = 1;
                                                for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, bits += stride8, buf += stride8)
                                                {
                                                    Vectors.Max(stride8, bits, buf - stride8, buf);
                                                }

                                                int tilecount = rbufsize - ibuf;
                                                if (tilecount > 0)
                                                {
                                                    Vectors.Tile(stride8, tilecount, buf - stride8, buf);
                                                }
                                            }
                                        }

                                        // compute maximums
                                        {
                                            byte* bitsdst = (byte*)ubitsdst + ((i - tailsize) * stride8);
                                            int idxl = lbufsize - 1;
                                            int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                            byte* bitsl = (byte*)lbufptr + (idxl * stride8);
                                            byte* bitsr = (byte*)rbufptr + (idxr * stride8);

                                            for (int j = -tailsize; j <= tailsize; j++, bitsdst += stride8, idxl--, bitsl -= stride8, idxr++, bitsr += stride8)
                                            {
                                                if ((i + j).Between(0, height - 1))
                                                {
                                                    if (!idxr.Between(0, rbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride8, bitsl, bitsdst);
                                                    }
                                                    else if (!idxl.Between(0, lbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride8, bitsr, bitsdst);
                                                    }
                                                    else
                                                    {
                                                        Vectors.Max(stride8, bitsl, bitsr, bitsdst);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    void Compute16()
                    {
                        int stride16 = src.Stride8 / 2;

                        unsafe
                        {
                            fixed (ulong* ubitssrc = src.Bits, ubitsdst = dst.Bits)
                            {
                                fixed (ulong* lbufptr = lbuf, rbufptr = rbuf)
                                {
                                    for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                                    {
                                        ushort* bitssrc = (ushort*)ubitssrc + (i * stride16);

                                        // compute left buffer
                                        {
                                            ushort* bits = bitssrc;
                                            ushort* buf = (ushort*)lbufptr;

                                            // first buffer
                                            if (i < height)
                                            {
                                                Vectors.Max(stride16, bits, bits - stride16, buf);
                                            }
                                            else if (i == height)
                                            {
                                                Vectors.Copy(stride16, bits - stride16, buf);
                                            }
                                            else
                                            {
                                                Vectors.Set(stride16, 0, buf);
                                            }

                                            bits -= 2 * stride16;
                                            buf += stride16;

                                            int ibuf = 1;
                                            for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, bits -= stride16, buf += stride16)
                                            {
                                                if (j < height)
                                                {
                                                    Vectors.Max(stride16, bits, buf - stride16, buf);
                                                }
                                                else
                                                {
                                                    Vectors.Copy(stride16, buf - stride16, buf);
                                                }
                                            }

                                            int tilecount = lbufsize - ibuf;
                                            if (tilecount > 0)
                                            {
                                                Vectors.Tile(stride16, tilecount, buf - stride16, buf);
                                            }
                                        }

                                        // compute right buffer
                                        {
                                            ushort* bits = bitssrc;
                                            ushort* buf = (ushort*)rbufptr;

                                            if (i >= height)
                                            {
                                                Vectors.Set(rbufsize * stride16, 0, buf);
                                            }
                                            else if (i == height - 1)
                                            {
                                                Vectors.Tile(stride16, rbufsize, bits, buf);
                                            }
                                            else
                                            {
                                                Vectors.Max(stride16, bits, bits + stride16, buf);
                                                bits += 2 * stride16;
                                                buf += stride16;

                                                int ibuf = 1;
                                                for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, bits += stride16, buf += stride16)
                                                {
                                                    Vectors.Max(stride16, bits, buf - stride16, buf);
                                                }

                                                int tilecount = rbufsize - ibuf;
                                                if (tilecount > 0)
                                                {
                                                    Vectors.Tile(stride16, tilecount, buf - stride16, buf);
                                                }
                                            }
                                        }

                                        // compute maximums
                                        {
                                            ushort* bitsdst = (ushort*)ubitsdst + ((i - tailsize) * stride16);
                                            int idxl = lbufsize - 1;
                                            int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                            ushort* bitsl = (ushort*)lbufptr + (idxl * stride16);
                                            ushort* bitsr = (ushort*)rbufptr + (idxr * stride16);

                                            for (int j = -tailsize; j <= tailsize; j++, bitsdst += stride16, idxl--, bitsl -= stride16, idxr++, bitsr += stride16)
                                            {
                                                if ((i + j).Between(0, height - 1))
                                                {
                                                    if (!idxr.Between(0, rbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride16, bitsl, bitsdst);
                                                    }
                                                    else if (!idxl.Between(0, lbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride16, bitsr, bitsdst);
                                                    }
                                                    else
                                                    {
                                                        Vectors.Max(stride16, bitsl, bitsr, bitsdst);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
#if false
                void ComputeCols_vHGW()
                {
                    int width = src.Width;
                    int height = src.Height;
                    int stride = src.Stride;
                    int stride8 = src.Stride8;
                    int bytesPerPixel = src.BitsPerPixel / 8;

                    // calculate left and right buffer sizes
                    int lsize = anchor.X;
                    int rsize = sesize.Width - 1 - lsize;
                    int tailsize = Math.Min(lsize, rsize);
                    int lbufsize = lsize + tailsize;
                    int rbufsize = rsize + tailsize;

                    // allocate buffers
                    int bufsize = height * bytesPerPixel;
                    byte[] lbuf = new byte[lbufsize * bufsize];
                    byte[] rbuf = new byte[rbufsize * bufsize];

                    switch (src.BitsPerPixel)
                    {
                        /*case 1:
                            Compute1();
                            break;*/

                        case 8:
                        case 24:
                        case 32:
                            Compute8();
                            break;

                        /*case 16:
                            Compute16();
                            break;*/

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }

                    void Compute8()
                    {
                        unsafe
                        {
                            fixed (ulong* ubitssrc = src.Bits, ubitsdst = dst.Bits)
                            {
                                fixed (byte* lbufptr = lbuf, rbufptr = rbuf)
                                {
                                    for (int i = tailsize, ii = width + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                                    {
                                        byte* bitssrc = (byte*)ubitssrc + (i * bytesPerPixel);

                                        // compute left buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)lbufptr;

                                            // first buffer
                                            if (i < width)
                                            {
                                                Vectors.Max(height, bits, stride8, bits - bytesPerPixel, stride8, buf, 1);
                                            }
                                            else if (i == width)
                                            {
                                                Vectors.Copy(height, bits - bytesPerPixel, stride8, buf, 1);
                                            }
                                            else
                                            {
                                                Vectors.Set(bufsize, 0, buf);
                                            }

                                            bits -= 2 * bytesPerPixel;
                                            buf += bufsize;

                                            int ibuf = 1;
                                            for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, bits -= bytesPerPixel, buf += bufsize)
                                            {
                                                if (j < width)
                                                {
                                                    Vectors.Max(height, bits, stride8, buf - bufsize, 1, buf, 1);
                                                }
                                                else
                                                {
                                                    Vectors.Copy(bufsize, buf - bufsize, buf);
                                                }
                                            }

                                            int tilecount = lbufsize - ibuf;
                                            if (tilecount > 0)
                                            {
                                                Vectors.Tile(bufsize, tilecount, buf - bufsize, buf);
                                            }
                                        }

                                        // compute right buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)rbufptr;

                                            if (i >= width)
                                            {
                                                Vectors.Set(rbufsize * bufsize, 0, buf);
                                            }
                                            else
                                            {
                                                int ibuf = 1;
                                                if (i == width - 1)
                                                {
                                                    Vectors.Copy(height, bits, stride8, buf, 1);
                                                    buf += bufsize;
                                                }
                                                else
                                                {
                                                    Vectors.Max(height, bits, stride8, bits + bytesPerPixel, stride8, buf, 1);
                                                    bits += 2 * bytesPerPixel;
                                                    buf += bufsize;

                                                    for (int j = i + 2; j < width && ibuf < rbufsize; j++, ibuf++, bits += bytesPerPixel, buf += bufsize)
                                                    {
                                                        Vectors.Max(height, bits, stride8, buf - bufsize, 1, buf, 1);
                                                    }
                                                }

                                                int tilecount = rbufsize - ibuf;
                                                if (tilecount > 0)
                                                {
                                                    Vectors.Tile(bufsize, tilecount, buf - bufsize, buf);
                                                }
                                            }
                                        }

                                        // compute maximums
                                        {
                                            int idxl = lbufsize - 1;
                                            int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                            byte* bitsl = (byte*)lbufptr + (idxl * bufsize);
                                            byte* bitsr = (byte*)rbufptr + (idxr * bufsize);

                                            for (int j = -tailsize; j <= tailsize; j++, idxl--, bitsl -= bufsize, idxr++, bitsr += bufsize)
                                            {
                                                if ((i + j).Between(0, width - 1))
                                                {
                                                    byte* bitsdst = (byte*)ubitsdst + ((i + j) * bytesPerPixel);

                                                    if (!idxr.Between(0, rbufsize - 1))
                                                    {
                                                        Vectors.Copy(height, bitsl, 1, bitsdst, stride8);
                                                    }
                                                    else if (!idxl.Between(0, lbufsize - 1))
                                                    {
                                                        Vectors.Copy(height, bitsr, 1, bitsdst, stride8);
                                                    }
                                                    else
                                                    {
                                                        Vectors.Max(height, bitsl, 1, bitsr, 1, bitsdst, stride8);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Dilates this <see cref="Image"/> by using the 3x3 structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Dilation assigns to each pixel a maximum value of all pixels covered by its structuring element.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Dilate3x3(Image dst, BorderType borderType, uint borderValue)
        {
            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            if (dst.Width <= 2 || dst.Height <= 2)
            {
                if (borderType == BorderType.BorderRepl)
                {
                    borderValue = this.Max();
                }
                else
                {
                    borderValue &= this.MaxColor;
                    if (borderValue != this.MaxColor)
                    {
                        borderValue = Color.Max(this.Max(), borderValue, this.BitsPerPixel);
                    }
                }

                dst.SetColor(borderValue);
            }
            else
            {
                Vectors.Copy(dst.Bits.Length, this.Bits, 0, dst.Bits, 0);

                dst.MaxEvery(0, 0, this.Width, this.Height - 1, this, 0, 1);
                dst.MaxEvery(0, 1, this.Width, this.Height - 1, this, 0, 0);
                dst.MaxEvery(0, 0, this.Width - 1, this.Height, this, 1, 0);
                dst.MaxEvery(1, 0, this.Width - 1, this.Height, this, 0, 0);

                if (borderType == BorderType.BorderConst)
                {
                    dst.MaxCBorder(1, 1, this.Width - 1, this.Height - 1, borderValue);
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Erodes this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Erosion assigns to each pixel a minimum value of all pixels covered by its structuring element.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Erode(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            if (se == null)
            {
                throw new ArgumentNullException(nameof(se));
            }

            if (iterations <= 0)
            {
                throw new ArgumentException("The number of iterations is equal to or less than zero.");
            }

            Image src = this;
            bool inplace = dst == this;

            Size sesize = se.Size;
            Point anchor = se.GetAnchor(StructuringElement.DefaultAnchor);

            // estimate border size
            int bx1 = Math.Max(anchor.X, 0) * iterations;
            int by1 = Math.Max(anchor.Y, 0) * iterations;
            int bx2 = src.Width - (Math.Max(sesize.Width - 1 - anchor.X, 0) * iterations);
            int by2 = src.Height - (Math.Max(sesize.Height - 1 - anchor.Y, 0) * iterations);

            // extreme case - border is larger than image
            // simply set entire image to the min color
            if (bx2 <= bx1 || by2 <= by1)
            {
                if (borderType == BorderType.BorderRepl)
                {
                    borderValue = this.Min();
                }
                else
                {
                    borderValue &= this.MaxColor;
                    if (borderValue != 0)
                    {
                        borderValue = Color.Min(this.Min(), borderValue, this.BitsPerPixel);
                    }
                }

                uint mincolor = src.Min();
                if (borderType == BorderType.BorderConst)
                {
                    mincolor = Color.Min(mincolor, borderValue & this.MaxColor, this.BitsPerPixel);
                }

                if (!inplace)
                {
                    dst = this.CreateTemplate(dst, this.BitsPerPixel);
                }

                dst.SetColor(mincolor);
            }
            else
            {
                dst = this.CreateTemplate(dst, this.BitsPerPixel);

                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    // for next iteration, copy destination image back into source
                    if (iteration > 0)
                    {
                        UpdateSource();
                    }

                    // special case for rectangular kernel
                    // instead of applying m x n mask
                    // we sequentially apply m x 1 and 1 x n masks
                    if (se is BrickStructuringElement && sesize.Width > 1 && sesize.Height > 1)
                    {
                        ApplySE(
                            StructuringElement.Brick(
                                sesize.Width,
                                1,
                                se.Anchor == StructuringElement.DefaultAnchor ? StructuringElement.DefaultAnchor : new Point(se.Anchor.X, 0)),
                            iteration == 0);

                        // for next iteration, copy destination image back into source
                        UpdateSource();

                        ApplySE(
                            StructuringElement.Brick(
                                1,
                                sesize.Height,
                                se.Anchor == StructuringElement.DefaultAnchor ? StructuringElement.DefaultAnchor : new Point(0, se.Anchor.Y)),
                            false);
                    }
                    else
                    {
                        ApplySE(se, iteration == 0);
                    }
                }

                // maximize border pixels
                if (borderType == BorderType.BorderConst)
                {
                    dst.MinCBorder(bx1, by1, bx2 - bx1, by2 - by1, borderValue);
                }

                if (inplace)
                {
                    this.Attach(dst);
                    return this;
                }
            }

            return dst;

            void UpdateSource()
            {
                if (src == this)
                {
                    src = dst.Clone(false);
                }

                Vectors.Copy(dst.Bits.Length, dst.Bits, 0, src.Bits, 0);
            }

            void ApplySE(StructuringElement currentSE, bool copySrcToDst)
            {
                // initialize dst with src if s.e. contains anchor point
                // anchor point will later be excluded from comparison
                if (currentSE.GetElements().Contains(Point.Empty))
                {
                    if (copySrcToDst)
                    {
                        Vectors.Copy(src.Bits.Length, src.Bits, 0, dst.Bits, 0);
                    }
                }
                else
                {
                    dst.SetToOne();
                }

                // use van-Herk/Gil-Werman (vHGW) algorithm for 1 x n and m x 1 kernels
                if (currentSE is BrickStructuringElement brick)
                {
                    // 1. works for s.e.
                    // 2. anchor should be inside the s.e. with at least one pixel on each side
                    // 3. use standard algorithm for small s.e. on binary images (it is faster)
                    if (brick.Width == 1 &&
                        anchor.Y.Between(1, brick.Height - 2) &&
                        (src.BitsPerPixel != 1 || brick.Height >= 5))
                    {
                        ComputeRows_vHGW();
                        return;
                    }

#if false
                    if (src.BitsPerPixel == 8 && brick.Height == 1 && anchor.X.Between(1, brick.Width - 2))
                    {
                        ComputeCols_vHGW();
                        return;
                    }
#endif
                }

                // apply s.e.
                foreach (Point point in currentSE.GetElements())
                {
                    if (point != Point.Empty)
                    {
                        ApplySEPoint(point);
                    }
                }

                void ApplySEPoint(Point point)
                {
                    int xdst = Core.MinMax.Max(-point.X, 0);
                    int ydst = Core.MinMax.Max(-point.Y, 0);
                    int xsrc = Core.MinMax.Max(point.X, 0);
                    int ysrc = Core.MinMax.Max(point.Y, 0);
                    int width = src.Width - Math.Abs(point.X);
                    int height = src.Height - Math.Abs(point.Y);

                    dst.MinEvery(xdst, ydst, width, height, src, xsrc, ysrc);
                }

                void ComputeRows_vHGW()
                {
                    int height = src.Height;

                    // calculate left and right buffer sizes
                    int lsize = anchor.Y;
                    int rsize = sesize.Height - 1 - lsize;
                    int tailsize = Math.Min(lsize, rsize);
                    int lbufsize = lsize + tailsize;
                    int rbufsize = rsize + tailsize;

                    // allocate buffers
                    int stride = src.Stride;
                    ulong[] lbuf = new ulong[lbufsize * stride];
                    ulong[] rbuf = new ulong[rbufsize * stride];

                    switch (src.BitsPerPixel)
                    {
                        case 1:
                            Compute1();
                            break;

                        case 8:
                        case 24:
                        case 32:
                            Compute8();
                            break;

                        case 16:
                            Compute16();
                            break;

                        default:
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_UnsupportedDepth,
                                this.BitsPerPixel));
                    }

                    void Compute1()
                    {
                        ulong[] bitssrc = src.Bits;
                        ulong[] bitsdst = dst.Bits;

                        for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                        {
                            // compute left buffer
                            {
                                int offbits = i * stride;
                                int offbuf = 0;

                                // first buffer
                                if (i < height)
                                {
                                    Vectors.And(stride, bitssrc, offbits, bitssrc, offbits - stride, lbuf, offbuf);
                                }
                                else if (i == height)
                                {
                                    Vectors.Copy(stride, bitssrc, offbits - stride, lbuf, offbuf);
                                }
                                else
                                {
                                    Vectors.Set(stride, byte.MaxValue, lbuf, offbuf);
                                }

                                offbits -= 2 * stride;
                                offbuf += stride;

                                int ibuf = 1;
                                for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, offbits -= stride, offbuf += stride)
                                {
                                    if (j < height)
                                    {
                                        Vectors.And(stride, bitssrc, offbits, lbuf, offbuf - stride, lbuf, offbuf);
                                    }
                                    else
                                    {
                                        Vectors.Copy(stride, lbuf, offbuf - stride, lbuf, offbuf);
                                    }
                                }

                                int tilecount = lbufsize - ibuf;
                                if (tilecount > 0)
                                {
                                    Vectors.Tile(stride, tilecount, lbuf, offbuf - stride, lbuf, offbuf);
                                }
                            }

                            // compute right buffer
                            {
                                int offbits = i * stride;
                                int offbuf = 0;

                                if (i >= height)
                                {
                                    Vectors.Set(rbufsize * stride, byte.MaxValue, rbuf, offbuf);
                                }
                                else if (i == height - 1)
                                {
                                    Vectors.Tile(stride, rbufsize, bitssrc, offbits, rbuf, offbuf);
                                }
                                else
                                {
                                    Vectors.And(stride, bitssrc, offbits, bitssrc, offbits + stride, rbuf, offbuf);
                                    offbits += 2 * stride;
                                    offbuf += stride;

                                    int ibuf = 1;
                                    for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, offbits += stride, offbuf += stride)
                                    {
                                        Vectors.And(stride, bitssrc, offbits, rbuf, offbuf - stride, rbuf, offbuf);
                                    }

                                    int tilecount = rbufsize - ibuf;
                                    if (tilecount > 0)
                                    {
                                        Vectors.Tile(stride, tilecount, rbuf, offbuf - stride, rbuf, offbuf);
                                    }
                                }
                            }

                            // compute maximums
                            {
                                int offdst = (i - tailsize) * stride;
                                int idxl = lbufsize - 1;
                                int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                int offl = idxl * stride;
                                int offr = idxr * stride;

                                for (int j = -tailsize; j <= tailsize; j++, offdst += stride, idxl--, offl -= stride, idxr++, offr += stride)
                                {
                                    if ((i + j).Between(0, height - 1))
                                    {
                                        if (!idxr.Between(0, rbufsize - 1))
                                        {
                                            Vectors.Copy(stride, lbuf, offl, bitsdst, offdst);
                                        }
                                        else if (!idxl.Between(0, lbufsize - 1))
                                        {
                                            Vectors.Copy(stride, rbuf, offr, bitsdst, offdst);
                                        }
                                        else
                                        {
                                            Vectors.And(stride, lbuf, offl, rbuf, offr, bitsdst, offdst);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    void Compute8()
                    {
                        int stride8 = src.Stride8;

                        unsafe
                        {
                            fixed (ulong* ubitssrc = src.Bits, ubitsdst = dst.Bits)
                            {
                                fixed (ulong* lbufptr = lbuf, rbufptr = rbuf)
                                {
                                    for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                                    {
                                        byte* bitssrc = (byte*)ubitssrc + (i * stride8);

                                        // compute left buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)lbufptr;

                                            // first buffer
                                            if (i < height)
                                            {
                                                Vectors.Min(stride8, bits, bits - stride8, buf);
                                            }
                                            else if (i == height)
                                            {
                                                Vectors.Copy(stride8, bits - stride8, buf);
                                            }
                                            else
                                            {
                                                Vectors.Set(stride8, byte.MaxValue, buf);
                                            }

                                            bits -= 2 * stride8;
                                            buf += stride8;

                                            int ibuf = 1;
                                            for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, bits -= stride8, buf += stride8)
                                            {
                                                if (j < height)
                                                {
                                                    Vectors.Min(stride8, bits, buf - stride8, buf);
                                                }
                                                else
                                                {
                                                    Vectors.Copy(stride8, buf - stride8, buf);
                                                }
                                            }

                                            int tilecount = lbufsize - ibuf;
                                            if (tilecount > 0)
                                            {
                                                Vectors.Tile(stride8, tilecount, buf - stride8, buf);
                                            }
                                        }

                                        // compute right buffer
                                        {
                                            byte* bits = bitssrc;
                                            byte* buf = (byte*)rbufptr;

                                            if (i >= height)
                                            {
                                                Vectors.Set(rbufsize * stride8, byte.MaxValue, buf);
                                            }
                                            else if (i == height - 1)
                                            {
                                                Vectors.Tile(stride8, rbufsize, bits, buf);
                                            }
                                            else
                                            {
                                                Vectors.Min(stride8, bits, bits + stride8, buf);
                                                bits += 2 * stride8;
                                                buf += stride8;

                                                int ibuf = 1;
                                                for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, bits += stride8, buf += stride8)
                                                {
                                                    Vectors.Min(stride8, bits, buf - stride8, buf);
                                                }

                                                int tilecount = rbufsize - ibuf;
                                                if (tilecount > 0)
                                                {
                                                    Vectors.Tile(stride8, tilecount, buf - stride8, buf);
                                                }
                                            }
                                        }

                                        // compute maximums
                                        {
                                            byte* bitsdst = (byte*)ubitsdst + ((i - tailsize) * stride8);
                                            int idxl = lbufsize - 1;
                                            int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                            byte* bitsl = (byte*)lbufptr + (idxl * stride8);
                                            byte* bitsr = (byte*)rbufptr + (idxr * stride8);

                                            for (int j = -tailsize; j <= tailsize; j++, bitsdst += stride8, idxl--, bitsl -= stride8, idxr++, bitsr += stride8)
                                            {
                                                if ((i + j).Between(0, height - 1))
                                                {
                                                    if (!idxr.Between(0, rbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride8, bitsl, bitsdst);
                                                    }
                                                    else if (!idxl.Between(0, lbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride8, bitsr, bitsdst);
                                                    }
                                                    else
                                                    {
                                                        Vectors.Min(stride8, bitsl, bitsr, bitsdst);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    void Compute16()
                    {
                        int stride16 = src.Stride8 / 2;

                        unsafe
                        {
                            fixed (ulong* ubitssrc = src.Bits, ubitsdst = dst.Bits)
                            {
                                fixed (ulong* lbufptr = lbuf, rbufptr = rbuf)
                                {
                                    for (int i = tailsize, ii = height + tailsize, step = (2 * tailsize) + 1; i < ii; i += step)
                                    {
                                        ushort* bitssrc = (ushort*)ubitssrc + (i * stride16);

                                        // compute left buffer
                                        {
                                            ushort* bits = bitssrc;
                                            ushort* buf = (ushort*)lbufptr;

                                            // first buffer
                                            if (i < height)
                                            {
                                                Vectors.Min(stride16, bits, bits - stride16, buf);
                                            }
                                            else if (i == height)
                                            {
                                                Vectors.Copy(stride16, bits - stride16, buf);
                                            }
                                            else
                                            {
                                                Vectors.Set(stride16, ushort.MaxValue, buf);
                                            }

                                            bits -= 2 * stride16;
                                            buf += stride16;

                                            int ibuf = 1;
                                            for (int j = i - 2; j >= 0 && ibuf < lbufsize; j--, ibuf++, bits -= stride16, buf += stride16)
                                            {
                                                if (j < height)
                                                {
                                                    Vectors.Min(stride16, bits, buf - stride16, buf);
                                                }
                                                else
                                                {
                                                    Vectors.Copy(stride16, buf - stride16, buf);
                                                }
                                            }

                                            int tilecount = lbufsize - ibuf;
                                            if (tilecount > 0)
                                            {
                                                Vectors.Tile(stride16, tilecount, buf - stride16, buf);
                                            }
                                        }

                                        // compute right buffer
                                        {
                                            ushort* bits = bitssrc;
                                            ushort* buf = (ushort*)rbufptr;

                                            if (i >= height)
                                            {
                                                Vectors.Set(rbufsize * stride16, ushort.MaxValue, buf);
                                            }
                                            else if (i == height - 1)
                                            {
                                                Vectors.Tile(stride16, rbufsize, bits, buf);
                                            }
                                            else
                                            {
                                                Vectors.Min(stride16, bits, bits + stride16, buf);
                                                bits += 2 * stride16;
                                                buf += stride16;

                                                int ibuf = 1;
                                                for (int j = i + 2; j < height && ibuf < rbufsize; j++, ibuf++, bits += stride16, buf += stride16)
                                                {
                                                    Vectors.Min(stride16, bits, buf - stride16, buf);
                                                }

                                                int tilecount = rbufsize - ibuf;
                                                if (tilecount > 0)
                                                {
                                                    Vectors.Tile(stride16, tilecount, buf - stride16, buf);
                                                }
                                            }
                                        }

                                        // compute maximums
                                        {
                                            ushort* bitsdst = (ushort*)ubitsdst + ((i - tailsize) * stride16);
                                            int idxl = lbufsize - 1;
                                            int idxr = Math.Max(0, rbufsize - (2 * tailsize)) - 1;
                                            ushort* bitsl = (ushort*)lbufptr + (idxl * stride16);
                                            ushort* bitsr = (ushort*)rbufptr + (idxr * stride16);

                                            for (int j = -tailsize; j <= tailsize; j++, bitsdst += stride16, idxl--, bitsl -= stride16, idxr++, bitsr += stride16)
                                            {
                                                if ((i + j).Between(0, height - 1))
                                                {
                                                    if (!idxr.Between(0, rbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride16, bitsl, bitsdst);
                                                    }
                                                    else if (!idxl.Between(0, lbufsize - 1))
                                                    {
                                                        Vectors.Copy(stride16, bitsr, bitsdst);
                                                    }
                                                    else
                                                    {
                                                        Vectors.Min(stride16, bitsl, bitsr, bitsdst);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perform morphological opening operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Opening is Erosion followed by Dilation.
        /// It is useful in removing noise.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MorphOpen(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            StructuringElement mirrorSE = se.Mirror();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                dst = (iteration == 0 ? this : dst).Erode(dst, se, 1, borderType, borderValue);
                dst.Dilate(dst, mirrorSE, 1, borderType, borderValue);
            }

            return dst;
        }

        /// <summary>
        /// Perform morphological closing operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Closing is reverse of Opening, Dilation followed by Erosion.
        /// It is useful in closing small holes inside the foreground objects, or small black points on the object.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MorphClose(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            StructuringElement mirrorSE = se.Mirror();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                dst = (iteration == 0 ? this : dst).Dilate(dst, se, 1, borderType, borderValue);
                dst.Erode(dst, mirrorSE, 1, borderType, borderValue);
            }

            return dst;
        }

        /// <summary>
        /// Perform morphological gradient operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>Morphological Gradient is the difference between Dilation and Erosion of an image.</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MorphGradient(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            StructuringElement mirrorSE = se.Mirror();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Image src = iteration == 0 ? this : dst;
                Image dilation = src.Dilate(null, se, 1, borderType, borderValue);
                Image erosion = src.Erode(null, mirrorSE, 1, borderType, borderValue);
                dst = dilation.Sub(dst, erosion, 0);
            }

            return dst;
        }

        /// <summary>
        /// Perform morphological top hat operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>Top Hat is the difference between input image and Opening of the image.</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MorphTopHat(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Image src = iteration == 0 ? this : dst;
                Image opening = src.MorphOpen(null, se, 1, borderType, borderValue);
                dst = src.Sub(dst, opening, 0);
            }

            return dst;
        }

        /// <summary>
        /// Perform morphological black hat operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="se">The structuring element used for operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="se"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The number of iterations is equal to or less than zero.</para>
        /// </exception>
        /// <remarks>
        /// <para>Black Hat is the difference between the Closing of the input image and input image.</para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image MorphBlackHat(Image dst, StructuringElement se, int iterations, BorderType borderType, uint borderValue)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Image src = iteration == 0 ? this : dst;
                Image closing = src.MorphClose(null, se, 1, borderType, borderValue);
                dst = closing.Sub(dst, src, 0);
            }

            return dst;
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
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
        public Image Despeckle(Image dst)
        {
            dst = this.Copy(dst, true);

            // create masks
            ulong[] mask = new ulong[dst.Bits.Length];
            ulong[] notbits = new ulong[dst.Bits.Length];
            Vectors.Not(dst.Bits.Length, dst.Bits, 0, notbits, 0);

            // remove isolated pixels
            Image.BuildORMask(dst, StructuringElement.Square(3), null, mask, false);
            Vectors.And(mask.Length, mask, 0, dst.Bits, 0);

            // 0 0 0
            // 0 x 0
            // x x x
            Image.BuildORMask(dst, StructuringElement.Brick(3, 2, new Point(1, 1)), null, mask, true);
            Image.BuildORMask(dst, StructuringElement.Brick(3, 1, new Point(1, -1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, dst.Bits, 0);

            // x x x
            // 0 x 0
            // 0 0 0
            Image.BuildORMask(dst, StructuringElement.Brick(3, 2, new Point(1, 0)), null, mask, true);
            Image.BuildORMask(dst, StructuringElement.Brick(3, 1, new Point(1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, dst.Bits, 0);

            // x 0 0
            // x x 0
            // x 0 0
            Image.BuildORMask(dst, StructuringElement.Brick(2, 3, new Point(0, 1)), null, mask, true);
            Image.BuildORMask(dst, StructuringElement.Brick(1, 3, new Point(1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, dst.Bits, 0);

            // 0 0 x
            // 0 x x
            // 0 0 x
            Image.BuildORMask(dst, StructuringElement.Brick(2, 3, new Point(1, 1)), null, mask, true);
            Image.BuildORMask(dst, StructuringElement.Brick(1, 3, new Point(-1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, dst.Bits, 0);

            // fill isolated gaps
            Image.BuildANDMask(dst, StructuringElement.Cross(3, 3), null, mask, true);
            Vectors.Or(mask.Length, mask, 0, dst.Bits, 0);

            // x x x
            // x 0 x
            // 0 0 0
            Image.BuildANDMask(dst, StructuringElement.Brick(3, 2, new Point(1, 1)), null, mask, true);
            Image.BuildANDMask(dst, StructuringElement.Brick(3, 1, new Point(1, -1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, dst.Bits, 0);

            // 0 0 0
            // x 0 x
            // x x x
            Image.BuildANDMask(dst, StructuringElement.Brick(3, 2, new Point(1, 0)), null, mask, true);
            Image.BuildANDMask(dst, StructuringElement.Brick(3, 1, new Point(1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, dst.Bits, 0);

            // 0 x x
            // 0 0 x
            // 0 x x
            Image.BuildANDMask(dst, StructuringElement.Brick(2, 3, new Point(0, 1)), null, mask, true);
            Image.BuildANDMask(dst, StructuringElement.Brick(1, 3, new Point(1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, dst.Bits, 0);

            // x x 0
            // x 0 0
            // x x 0
            Image.BuildANDMask(dst, StructuringElement.Brick(2, 3, new Point(1, 1)), null, mask, true);
            Image.BuildANDMask(dst, StructuringElement.Brick(1, 3, new Point(-1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, dst.Bits, 0);

            return dst;
        }

        /// <summary>
        /// Finds connected components on this <see cref="Image"/>.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <returns>
        /// A set of <see cref="ConnectedComponent"/> objects found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para>The <see cref="Image{T}.BitsPerPixel"/> is not 1.</para>
        /// </exception>
        public HashSet<ConnectedComponent> FindConnectedComponents(int connectivity)
            => this.FindConnectedComponents(connectivity, 0, 0, this.Width, this.Height);

        /// <summary>
        /// Finds connected components on this <see cref="Image"/>
        /// withing the rectangular area specified by a pair of coordinates, width and height.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A set of <see cref="ConnectedComponent"/> objects found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para>The <see cref="Image{T}.BitsPerPixel"/> is not 1.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        public HashSet<ConnectedComponent> FindConnectedComponents(
            int connectivity,
            int x,
            int y,
            int width,
            int height)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            if (connectivity != 4 && connectivity != 8)
            {
                throw new ArgumentException("The connectivity is neither 4 nor 8.");
            }

            this.ValidateArea(x, y, width, height);

            HashSet<ConnectedComponent> all = new HashSet<ConnectedComponent>();
            List<Stroke> last = new List<Stroke>();
            List<Stroke> current = new List<Stroke>();

            bool connectivity8 = connectivity == 8;
            int stride1 = this.Stride1;
            ulong[] bits = this.Bits;

            for (int iy = y, iiy = iy + height, ypos = (iy * stride1) + x; iy < iiy; iy++, ypos += stride1)
            {
                // find intervals
                int lastIndex = 0;
                for (int xpos = ypos, xposend = ypos + width; xpos < xposend;)
                {
                    int start = BitUtils.BitScanOneForward(xposend - xpos, bits, xpos);
                    if (start == -1)
                    {
                        break;
                    }

                    int end = BitUtils.BitScanZeroForward(xposend - (start + 1), bits, start + 1);
                    if (end == -1)
                    {
                        end = xposend;
                    }

                    // merge interval
                    MergeStroke(start - ypos, end - start);
                    xpos = end + 1;

                    void MergeStroke(int x1, int length)
                    {
                        // the component we will attach the stroke to
                        ConnectedComponent component = null;

                        // start matching from the position we stopped at last time
                        for (int i = lastIndex, x2 = x1 + length, ii = last.Count; i < ii; i++)
                        {
                            Stroke lastStroke = last[i];
                            if (ConnectedComponent.StrokesIntersect(connectivity8, lastStroke.X, lastStroke.Length, x1, length))
                            {
                                if (component == null)
                                {
                                    component = lastStroke.Component;
                                    component.AddStroke(iy, x1, length);
                                }
                                else
                                {
                                    ConnectedComponent anotherComponent = lastStroke.Component;
                                    if (anotherComponent != component)
                                    {
                                        // merge components if strokes touches more than one components
                                        component.MergeWith(anotherComponent);

                                        // remove merged component from the set
                                        all.Remove(anotherComponent);

                                        // replace merged component in previous line
                                        ReplaceComponent(last, i);

                                        // replace merged component in this line
                                        ReplaceComponent(current, current.Count);

                                        void ReplaceComponent(List<Stroke> strokes, int anchorPosition)
                                        {
                                            Rectangle anotherBounds = anotherComponent.Bounds;
                                            for (int j = anchorPosition, jj = strokes.Count; j < jj; j++)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
                                                }
                                                else if (strokes[j].Component != component && strokes[j].X > anotherBounds.Right)
                                                {
                                                    Debug.Assert(strokes.Skip(anchorPosition).All(s => s.Component != anotherComponent), "Component must be removed.");
                                                    break;
                                                }
                                            }

                                            for (int j = anchorPosition - 1; j >= 0; j--)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
                                                }
                                                else if (strokes[j].Component != component && strokes[j].X < anotherBounds.X)
                                                {
                                                    Debug.Assert(strokes.Take(anchorPosition - 1).All(s => s.Component != anotherComponent), "Component must be removed.");
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                lastIndex = i;
                            }
                            else if (x2 < lastStroke.X)
                            {
                                // all remaining strokes from previous line are to the left of this one
                                // we can stop matching
                                break;
                            }
                        }

                        if (component == null)
                        {
                            component = new ConnectedComponent(iy, x1, length);
                            all.Add(component);
                        }

                        current.Add(new Stroke(x1, length, component));
                    }
                }

                Swapping.Swap(ref current, ref last);
                current.Clear();
            }

            Debug.Assert(this.Power(x, y, width, height) == (ulong)all.Sum(value => value.Power), "The number of pixels on image and in components must match.");
            return all;
        }

        /// <summary>
        /// Finds connected components on this <see cref="Image"/>
        /// withing the rectangular area specified by <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="area">The width, height, and location of the area.</param>
        /// <returns>
        /// A set of <see cref="ConnectedComponent"/> objects found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para>The <see cref="Image{T}.BitsPerPixel"/> is not 1.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The area is out of image bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ISet<ConnectedComponent> FindConnectedComponents(int connectivity, Rectangle area) =>
            this.FindConnectedComponents(connectivity, area.X, area.Y, area.Width, area.Height);

        /// <summary>
        /// Draws the <see cref="ConnectedComponent"/> on this <see cref="Image"/> using the specified color.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to draw.</param>
        /// <param name="color">The color of the connected component.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="component"/> is <b>null</b>.
        /// </exception>
        [CLSCompliant(false)]
        public void DrawConnectedComponent(ConnectedComponent component, uint color)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;
            int bitsPerPixel = this.BitsPerPixel;
            ulong colorbits = this.ColorBits(color);

            if (colorbits == 0)
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils.ResetBits(length * bitsPerPixel, bits, (y * stride1) + (x * bitsPerPixel));
                }
            }
            else if (colorbits == ulong.MaxValue)
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils.SetBits(length * bitsPerPixel, bits, (y * stride1) + (x * bitsPerPixel));
                }
            }
            else
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils.SetBits(length * bitsPerPixel, colorbits, bits, (y * stride1) + (x * bitsPerPixel));
                }
            }
        }

        /// <summary>
        /// Draws the collection of <see cref="ConnectedComponent"/> objects on this <see cref="Image"/> using the specified color.
        /// </summary>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to draw.</param>
        /// <param name="color">The color of the connected component.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        [CLSCompliant(false)]
        public void DrawConnectedComponents(IEnumerable<ConnectedComponent> components, uint color)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                this.DrawConnectedComponent(component, color);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the <see cref="ConnectedComponent"/> from this <see cref="Image"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="component"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void RemoveConnectedComponent(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;

            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils.ResetBits(length, bits, (y * stride1) + x);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from this <see cref="Image"/>.
        /// </summary>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void RemoveConnectedComponents(IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                this.RemoveConnectedComponent(component);
            }
        }

        /// <summary>
        /// Crops the black pixels contained in the <see cref="ConnectedComponent"/> from this <see cref="Image"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="component"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public Image CropConnectedComponent(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            // calculate area to crop
            Rectangle bounds = component.Bounds;

            // allocate new image
            Image dst = new Image(bounds.Size, this);

            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc = this.Stride;
            int stridedst = dst.Stride;

            // copy bits
            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils.CopyBits(length, bitssrc, (y * stridesrc) + x, bitsdst, ((y - bounds.Y) * stridedst) + x - bounds.X);
            }

            return dst;
        }

        /// <summary>
        /// Crops the black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from this <see cref="Image"/>.
        /// </summary>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public Image CropConnectedComponents(IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            // calculate area to crop
            Rectangle bounds = Rectangle.Union(components.Select(x => x.Bounds));
            if (bounds.IsEmpty)
            {
                return new Image(1, 1, this);
            }

            // allocate new image
            Image dst = new Image(bounds.Size, this);

            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc1 = this.Stride1;
            int stridedst1 = dst.Stride1;

            // copy bits
            foreach (ConnectedComponent component in components)
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils.CopyBits(length, bitssrc, (y * stridesrc1) + x, bitsdst, ((y - bounds.Y) * stridedst1) + x - bounds.X);
                }
            }

            return dst;
        }

        /// <summary>
        /// Computes the distance from each pixel to the nearest background pixel.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="bitsPerPixel">The destination image color depth, in number of bits per pixel (8 or 16).</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method computes Manhattan distance from each foreground pixels to its nearest background pixel.
        /// The credit for algorithm design goes to Luc Vincent.
        /// </para>
        /// <para>
        /// Initially, the destination image is initialized by ones for corresponding foreground pixels and zeros for background pixels.
        /// The algorithm does two sequential pixel scans: the first scan goes from upper-left to bottom-right image corner; the second scan goes in reverse order.
        /// For 4 connectivity during first scan each foreground pixels is computed as <c>i(x, y) = min(i(x - 1,y), i(x, y - 1)) + 1</c> for 4 connectivity.
        /// During second scan each foreground pixels is computed as <c>i(x, y) = min(i(x, y), min(i(x + 1, y), i(x, y + 1)) + 1)</c>.
        /// For 8 connectivity two top corner pixels are added to the formula during first pass;
        /// two bottom corner pixels are added to the formula during second pass.
        /// </para>
        /// <para>The Leptonica analog is <c>pixDistanceFunction</c>.</para>
        /// <para>The method works on binary images only.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para><paramref name="bitsPerPixel"/> is neither 8 nor 16.</para>
        /// <para>-or-</para>
        /// <para>The <see cref="Image{T}.BitsPerPixel"/> is not 1.</para>
        /// </exception>
        public Image DistanceToBackground(int connectivity, int bitsPerPixel)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            if (bitsPerPixel != 8 && bitsPerPixel != 16)
            {
                throw new ArgumentException("The destination image depth is neither 8 nor 16.");
            }

            Image dst = bitsPerPixel == 8 ? this.Convert1To8(null, 0, 1) : this.Convert1To16(null, 0, 1);
            int stride = dst.Stride;

            int xx = dst.Width - 1;
            int yy = dst.Height - 1;

            switch (connectivity)
            {
                case 4:
                    Compute4();
                    break;

                case 8:
                    Compute8();
                    break;

                default:
                    throw new ArgumentException("The connectivity is neither 4 nor 8.");
            }

            void Compute4()
            {
                unsafe
                {
                    fixed (ulong* ubits = dst.Bits)
                    {
                        if (bitsPerPixel == 8)
                        {
                            byte* bits = (byte*)ubits;
                            stride *= sizeof(ulong) / sizeof(byte);

                            // scan from upper-left to bottom-right corner
                            // start from pixel with coordinates { 1, 1 }
                            for (int iy = 1, offy = stride + 1; iy < yy; iy++, offy += stride)
                            {
                                for (int ix = 1, offx = offy; ix < xx; ix++, offx++)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        bits[offx] += Core.MinMax.Min(bits[offx - stride], bits[offx - 1], (byte)254);
                                    }
                                }
                            }

                            // scan from bottom-right to upper-left corner
                            // start from pixel with coordinates { width - 2, height - 2 }
                            for (int iy = yy - 1, offy = (iy * stride) + xx - 1; iy > 0; iy--, offy -= stride)
                            {
                                for (int ix = xx - 1, offx = offy; ix > 0; ix--, offx--)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        byte val = Core.MinMax.Min(bits[offx + stride], bits[offx + 1], (byte)254);
                                        bits[offx] = Core.MinMax.Min(++val, bits[offx]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ushort* bits = (ushort*)ubits;
                            stride *= sizeof(ulong) / sizeof(ushort);

                            // scan from upper-left to bottom-right corner
                            // start from pixel with coordinates { 1, 1 }
                            for (int iy = 1, offy = stride + 1; iy < yy; iy++, offy += stride)
                            {
                                for (int ix = 1, offx = offy; ix < xx; ix++, offx++)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        bits[offx] += Core.MinMax.Min(bits[offx - stride], bits[offx - 1], (ushort)65534);
                                    }
                                }
                            }

                            // scan from bottom-right to upper-left corner
                            // start from pixel with coordinates { width - 2, height - 2 }
                            for (int iy = yy - 1, offy = (iy * stride) + xx - 1; iy > 0; iy--, offy -= stride)
                            {
                                for (int ix = xx - 1, offx = offy; ix > 0; ix--, offx--)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        ushort val = Core.MinMax.Min(bits[offx + stride], bits[offx + 1], (ushort)65534);
                                        bits[offx] = Core.MinMax.Min(++val, bits[offx]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            void Compute8()
            {
                unsafe
                {
                    fixed (ulong* ubits = dst.Bits)
                    {
                        if (bitsPerPixel == 8)
                        {
                            byte* bits = (byte*)ubits;
                            stride *= sizeof(ulong) / sizeof(byte);

                            // scan from upper-left to bottom-right corner
                            // start from pixel with coordinates { 1, 1 }
                            for (int iy = 1, offy = stride + 1; iy < yy; iy++, offy += stride)
                            {
                                for (int ix = 1, offx = offy; ix < xx; ix++, offx++)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        int offxp = offx - stride;
                                        bits[offx] += Core.MinMax.Min(bits[offxp - 1], bits[offxp], bits[offxp + 1], bits[offx - 1], (byte)254);
                                    }
                                }
                            }

                            // scan from bottom-right to upper-left corner
                            // start from pixel with coordinates { width - 2, height - 2 }
                            for (int iy = yy - 1, offy = (iy * stride) + xx - 1; iy > 0; iy--, offy -= stride)
                            {
                                for (int ix = xx - 1, offx = offy; ix > 0; ix--, offx--)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        int offxn = offx + stride;
                                        byte val = Core.MinMax.Min(bits[offxn - 1], bits[offxn], bits[offxn + 1], bits[offx + 1], (byte)254);
                                        bits[offx] = Core.MinMax.Min(++val, bits[offx]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ushort* bits = (ushort*)ubits;
                            stride *= sizeof(ulong) / sizeof(ushort);

                            // scan from upper-left to bottom-right corner
                            // start from pixel with coordinates { 1, 1 }
                            for (int iy = 1, offy = stride + 1; iy < yy; iy++, offy += stride)
                            {
                                for (int ix = 1, offx = offy; ix < xx; ix++, offx++)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        int offxp = offx - stride;
                                        bits[offx] += Core.MinMax.Min(bits[offxp - 1], bits[offxp], bits[offxp + 1], bits[offx - 1], (ushort)65534);
                                    }
                                }
                            }

                            // scan from bottom-right to upper-left corner
                            // start from pixel with coordinates { width - 2, height - 2 }
                            for (int iy = yy - 1, offy = (iy * stride) + xx - 1; iy > 0; iy--, offy -= stride)
                            {
                                for (int ix = xx - 1, offx = offy; ix > 0; ix--, offx--)
                                {
                                    if (bits[offx] != 0)
                                    {
                                        int offxn = offx + stride;
                                        ushort val = Core.MinMax.Min(bits[offxn - 1], bits[offxn], bits[offxn + 1], bits[offx + 1], (ushort)65534);
                                        bits[offx] = Core.MinMax.Min(++val, bits[offx]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// Performs flood fill (binary reconstruction) of the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="mask">The mask image.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="mask"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para>The depth of this <see cref="Image"/> is not 1 bit per pixel.</para>
        /// <para>-or-</para>
        /// <para>The depth of <paramref name="mask"/> is not the same as the depth of this <see cref="Image"/>.</para>
        /// </exception>
        public Image FloodFill(Image dst, int connectivity, Image mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            if (connectivity != 4 && connectivity != 8)
            {
                throw new ArgumentException("The connectivity is neither 4 nor 8.");
            }

            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            if (mask.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame, nameof(mask));
            }

            // copy to destination image
            dst = this.Copy(dst, true);

            int stridesrc = dst.Stride;
            int stridemask = mask.Stride;

            int width = Math.Min(dst.Width, mask.Width);
            int height = Math.Min(dst.Height, mask.Height);
            int stride = Math.Min(stridesrc, stridemask);

            ulong[] bits = dst.Bits;
            ulong[] bitsmask = mask.Bits;
            ulong[] buffer = new ulong[stridesrc];

            const int MaxIter = 40;
            int iter = 0;
            int top = 0;
            int bottom = height - 1;
            while (++iter < MaxIter && top <= bottom)
            {
                // scan from upper-left to bottom-right corner
                Vectors.Set(stride, 0, buffer, 0);
                for (int i = top, off = i * stridesrc, offmask = i * stridemask; i < height; i++, off += stridesrc, offmask += stridemask)
                {
                    int off64 = off * 64;

                    // or from above
                    if (i > 0)
                    {
                        int offprev = off - stridesrc;
                        Vectors.Copy(stride, bits, offprev, buffer, 0);

                        if (connectivity == 8)
                        {
                            BitUtils.Or(width - 1, bits, offprev * 64, buffer, 1);
                            BitUtils.Or(width - 1, bits, (offprev * 64) + 1, buffer, 0);
                        }
                    }

                    // or from left
                    BitUtils.Or(width - 1, bits, off64, buffer, 1);

                    // mask pixels
                    Vectors.And(stride, bitsmask, offmask, buffer, 0);
                    Vectors.Xand(stride, bits, off, buffer, 0);

                    // check if any pixel will change
                    int bitpos = BitUtils.BitScanOneForward(width, buffer, 0);
                    if (bitpos == -1)
                    {
                        // pixel count did not change - shrink upper boundary if we are at the top
                        if (i == top)
                        {
                            top++;
                        }

                        if (i >= bottom)
                        {
                            // no need to do it any longer
                            break;
                        }
                    }
                    else
                    {
                        // pixel count did change - expand lower boundary
                        bottom = Core.MinMax.Max(i, bottom);

                        do
                        {
                            int bitoff = bitpos >> 6;

                            int lastbitpos = BitUtils.BitScanOneReverse(width, buffer, width - 1);
                            int lastbitoff = lastbitpos >> 6;

                            int count = lastbitoff - bitoff + 1;
                            Vectors.Or(count, buffer, bitoff, bits, off + bitoff);

                            // shift changed pixels to the left (from LSB to MSB)
                            // on image they are shifted to the right
                            if (((lastbitpos + 1) & 63) == 0)
                            {
                                count = Math.Min(count + 1, buffer.Length - bitoff);
                            }

                            Vectors.Shl(count, 1, buffer, bitoff);

                            // mask pixels
                            // this would leave only new pixels that have to be set
                            Vectors.And(count, bitsmask, offmask + bitoff, buffer, bitoff);
                            Vectors.Xand(count, bits, off + bitoff, buffer, bitoff);

                            bitpos = BitUtils.BitScanOneForward(width - (bitpos + 1), buffer, bitpos + 1);
                        }
                        while (bitpos != -1);
                    }
                }

                // scan from bottom-right to upper-left corner
                Vectors.Set(stride, 0, buffer, 0);
                for (int i = bottom, off = i * stridesrc, offmask = i * stridemask; i >= 0; i--, off -= stridesrc, offmask -= stridemask)
                {
                    int off64 = off * 64;

                    // or from below
                    if (i < height - 1)
                    {
                        int offnext = off + stridesrc;
                        Vectors.Copy(stride, bits, offnext, buffer, 0);

                        if (connectivity == 8)
                        {
                            BitUtils.Or(width - 1, bits, offnext * 64, buffer, 1);
                            BitUtils.Or(width - 1, bits, (offnext * 64) + 1, buffer, 0);
                        }
                    }

                    // or from right
                    BitUtils.Or(width - 1, bits, off64 + 1, buffer, 0);

                    // mask pixels
                    Vectors.And(stride, bitsmask, offmask, buffer, 0);
                    Vectors.Xand(stride, bits, off, buffer, 0);

                    // check if any pixel will change
                    int lastbitpos = BitUtils.BitScanOneReverse(width, buffer, width - 1);
                    if (lastbitpos == -1)
                    {
                        // pixel count did not change - shrink lower boundary if we are at the bottom
                        if (i == bottom)
                        {
                            bottom--;
                        }

                        if (i <= top)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // pixel count did change - expand upper boundary
                        top = Core.MinMax.Min(i, top);

                        do
                        {
                            int bitpos = BitUtils.BitScanOneForward(width, buffer, 0);
                            int bitoff = bitpos >> 6;

                            int lastbitoff = lastbitpos >> 6;

                            int count = lastbitoff - bitoff + 1;
                            Vectors.Or(count, buffer, bitoff, bits, off + bitoff);

                            // shift changed pixels to the right (from MSB to LSB)
                            // on image they are shifted to the left
                            if ((bitpos & 63) == 0 && bitoff > 0)
                            {
                                bitoff--;
                                count++;
                            }

                            Vectors.Shr(count, 1, buffer, bitoff);

                            // mask pixels
                            // this would leave only new pixels that have to be set
                            Vectors.And(count, bitsmask, offmask + bitoff, buffer, bitoff);
                            Vectors.Xand(count, bits, off + bitoff, buffer, bitoff);

                            lastbitpos = BitUtils.BitScanOneReverse(lastbitpos, buffer, lastbitpos - 1);
                        }
                        while (lastbitpos != -1);
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// Performs flood fill (binary reconstruction) of the <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="mask">The mask image.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="mask"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="connectivity"/> is neither 4 nor 8.</para>
        /// <para>-or-</para>
        /// <para>The <see cref="Image{T}.BitsPerPixel"/> is not 1.</para>
        /// </exception>
        public void MorphFloodFill(int connectivity, Image mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            if (connectivity != 4 && connectivity != 8)
            {
                throw new ArgumentException("The connectivity is neither 4 nor 8.");
            }

            if (mask.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            StructuringElement se = connectivity == 8 ? StructuringElement.Square(3) : StructuringElement.Cross(3);

            ulong pixelCount = this.Power();
            while (pixelCount > 0)
            {
                this.Dilate(this, se, 1, BorderType.BorderConst, 0);
                this.And(this, mask);

                ulong oldPixelCount = pixelCount;
                pixelCount = this.Power();
                if (oldPixelCount == pixelCount)
                {
                    break;
                }
            }
        }

        private static void BuildORMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                Vectors.Set(mask.Length, 0, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point != Point.Empty)
                {
                    if (point.X == 0)
                    {
                        Vectors.Or(
                            (height - Math.Abs(point.Y)) * stride,
                            bits,
                            Core.MinMax.Max(point.Y, 0) * stride,
                            mask,
                            Core.MinMax.Max(-point.Y, 0) * stride);
                    }
                    else
                    {
                        int count = width - Math.Abs(point.X);
                        int offx = (Core.MinMax.Max(point.Y, 0) * stride1) + Core.MinMax.Max(point.X, 0);
                        int offy = (Core.MinMax.Max(-point.Y, 0) * stride1) + Core.MinMax.Max(-point.X, 0);
                        for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                        {
                            BitUtils.Or(count, bits, offx, mask, offy);
                        }
                    }
                }
            }
        }

        private static void BuildANDMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                Vectors.Set(mask.Length, ulong.MaxValue, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point != Point.Empty)
                {
                    if (point.X == 0)
                    {
                        Vectors.And(
                            (height - Math.Abs(point.Y)) * stride,
                            bits,
                            Core.MinMax.Max(point.Y, 0) * stride,
                            mask,
                            Core.MinMax.Max(-point.Y, 0) * stride);
                    }
                    else
                    {
                        int count = width - Math.Abs(point.X);
                        int offx = (Core.MinMax.Max(point.Y, 0) * stride1) + Core.MinMax.Max(point.X, 0);
                        int offy = (Core.MinMax.Max(-point.Y, 0) * stride1) + Core.MinMax.Max(-point.X, 0);
                        for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                        {
                            BitUtils.And(count, bits, offx, mask, offy);
                        }
                    }
                }
            }
        }

        [DebuggerDisplay("{X} {Length} {Component}")]
        private sealed class Stroke
        {
            private readonly int x;
            private readonly int length;
            private ConnectedComponent component;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stroke(int x, int length, ConnectedComponent component)
            {
                this.x = x;
                this.length = length;
                this.component = component;
            }

            public int X => this.x;

            public int Length => this.length;

            public int X2 => this.x + this.length;

            public ConnectedComponent Component
            {
                get => this.component;
                set => this.component = value;
            }
        }
    }
}
