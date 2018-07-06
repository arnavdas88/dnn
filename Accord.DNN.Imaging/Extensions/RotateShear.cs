// -----------------------------------------------------------------------
// <copyright file="RotateShear.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Leptonica;

    /// <summary>
    /// Provides rotation and mirroring extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class RotateShear
    {
        ////static const unsigned char bM[8] = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        /// <summary>
        /// Rotates the <see cref="Image"/> by an arbitrary angle.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to rotate.</param>
        /// <param name="angle">The rotation angle, in degrees.</param>
        /// <returns>
        /// A new rotated <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Rotate(this Image image, double angle)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            while (angle >= 360.0)
            {
                angle -= 360.0;
            }

            while (angle < 0.0)
            {
                angle += 360.0;
            }

            if (angle == 0.0)
            {
                return image.Copy();
            }

#if true
            float a = (float)(Math.PI * (angle / 180.0));
            if (Math.Abs(a) < 0.001f)
            {
                return image.Copy();
            }

            using (Pix pixs = image.CreatePix())
            {
                RotateFlags flags = RotateFlags.L_ROTATE_AREA_MAP;
                if (image.BitsPerPixel == 1)
                {
                    flags = Math.Abs(a) > 0.06f ? RotateFlags.L_ROTATE_SAMPLING : RotateFlags.L_ROTATE_SHEAR;
                }

                using (Pix pixd = pixs.pixRotate(a, flags, BackgroundFlags.L_BRING_IN_WHITE, image.Width, image.Height))
                {
                    return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                }
            }
#else

            /*if (angle == 180.0)
            {
                return image.Rotate180();
            }
            else if (angle == 90.0)
            {
                return image.Rotate90(true);
            }
            else if (angle == 270.0)
            {
                return image.Rotate90(false);
            }*/

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            angle = 3.14159265 * angle / 180;

            // calculate new image size and position
            int x1 = image.Width - 1;
            int y1 = 0;
            _RotatePoint(ref x1, ref y1, angle);

            int x2 = image.Width - 1;
            int y2 = image.Height - 1;
            _RotatePoint(ref x2, ref y2, angle);

            int x3 = 0;
            int y3 = image.Height - 1;
            _RotatePoint(ref x3, ref y3, angle);

            int left = MKL.Min(x1, x2, x3, 0);
            int top = MKL.Min(y1, y2, y3, 0);
            int right = MKL.Max(x1, x2, x3, 0);
            int bottom = MKL.Max(y1, y2, y3, 0);

            // allocate new image
            Image dst = new Image(
                right - left + 1,
                bottom - top + 1,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution);

            ////double sinA = Math.Sin(angle);
            ////double cosA = Math.Cos(angle);

#if false
            /*unsigned char** pp = new unsigned char*[heightDst];
            unsigned char* p = BitsDst->Get();
            for (int n = 0; n < heightDst; n++, p += strideDst)
                pp[n] = p;

            int nByteWidth = _BYTEWIDTH(_width * _bitCount);
            const unsigned char* pBits = _Bits->Get();*/

            for (int y = 0; y < _height; y++, pBits += _stride)
            {
                double ysinA = sinA * y;
                double ycosA = cosA * y;

                const unsigned char* p = pBits;
                for (int nX = 0, nX8 = 0; nX < nByteWidth; nX++, nX8 += 8, p++)
                {
                    unsigned char b = *p;
                    if (b == 0)
                    {
                        continue;
                    }

                    double xfactor = cosA * (double)nX8 - ysinA - left;
                    double yfactor = sinA * (double)nX8 + ycosA - top;

                    if ((b & 0x80) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x40) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x20) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x10) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x08) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x04) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x02) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }

                    xfactor += cosA;
                    yfactor += sinA;

                    if ((b & 0x01) != 0)
                    {
                        uint x = (uint)xfactor;
                        pp[(int)yfactor][x >> 3] |= bM[x & 0x07];
                    }
                }
            }
#endif

            return dst;

            void _RotatePoint(ref int x, ref int y, double a)
            {
                int tempx = (int)Math.Round(((double)x * Math.Cos(a)) - ((double)y * Math.Sin(a)));
                int tempy = (int)Math.Round(((double)y * Math.Cos(a)) + ((double)x * Math.Sin(a)));

                x = tempx;
                y = tempy;
            }
#endif
        }

        /// <summary>
        /// Shears the <see cref="Image"/> by the specified amount.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to shear.</param>
        /// <param name="shearTan">Shearing force.
        /// Each horizontal string with Y coordinate equal to y is shifted horizontally by the <i>skew</i>*y pixels.
        /// </param>
        /// <returns>
        /// A new sheared <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Shear(this Image image, double shearTan)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int maxoffset = Math.Abs((int)((shearTan * (image.Height - 1)) + 0.5)) * image.BitsPerPixel;

            Image dst = new Image(
               image.Width + maxoffset,
               image.Height,
               image.BitsPerPixel,
               image.HorizontalResolution,
               image.VerticalResolution);

            // allocate new DIB bits
            int widthsrc1 = image.WidthBits;
            int widthdst1 = dst.WidthBits;
            int stridesrc1 = image.Stride1;
            int stridedst1 = dst.Stride1;

            uint[] bitssrc = image.Bits;
            uint[] bitsdst = dst.Bits;

            for (int iy = 0, possrc = 0, posdst = 0; iy < image.Height; iy++, possrc += stridesrc1, posdst += stridedst1)
            {
                int offset = Math.Abs((int)((shearTan * iy) + 0.5)) * image.BitsPerPixel;

                if (offset > 0)
                {
                    BitUtils.ResetBits(offset, bitsdst, posdst);
                }

                BitUtils.CopyBits(widthsrc1, bitssrc, possrc, bitsdst, posdst + offset);

                if (offset < maxoffset)
                {
                    BitUtils.ResetBits(widthdst1 - widthsrc1 - offset, bitsdst, posdst + offset + widthsrc1);
                }
            }

            return dst;
        }
    }
}
