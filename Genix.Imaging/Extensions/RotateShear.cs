// -----------------------------------------------------------------------
// <copyright file="RotateShear.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;
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

            while (angle <= -360.0)
            {
                angle += 360.0;
            }

            if (angle == 0.0)
            {
                return image.Copy();
            }

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

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;

            for (int iy = 0, possrc = 0, posdst = 0; iy < image.Height; iy++, possrc += stridesrc1, posdst += stridedst1)
            {
                int offset = Math.Abs((int)((shearTan * iy) + 0.5)) * image.BitsPerPixel;

                if (offset > 0)
                {
                    BitUtils64.ResetBits(offset, bitsdst, posdst);
                }

                BitUtils64.CopyBits(widthsrc1, bitssrc, possrc, bitsdst, posdst + offset);

                if (offset < maxoffset)
                {
                    BitUtils64.ResetBits(widthdst1 - widthsrc1 - offset, bitsdst, posdst + offset + widthsrc1);
                }
            }

            return dst;
        }

        /// <summary>
        /// De-skews this <see cref="Image"/> and aligns it horizontally.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to deskew.</param>
        /// <returns>
        /// The aligned <see cref="Image"/>.
        /// </returns>
        public static Image Deskew(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            ulong[] bits = image.Bits;

            // build histogram
            ulong endMask = ulong.MaxValue << (64 - (image.WidthBits & 63));
            float[][] histogram = new float[stride][];
            for (int ix = 0; ix < stride; ix++)
            {
                float[] h = histogram[ix] = new float[height];
                ulong mask = ix == stride - 1 ? endMask : ulong.MaxValue;
                for (int iy = 0, off = ix; iy < height; iy++, off += stride)
                {
                    h[iy] = BitUtils64.CountOneBits(bits[off] & mask);
                }
            }

            // calculate image variance
            float angleBest = 0.0f;
            float varianceBest = estimateSkewAngle(angleBest);

            // move up or down with 1 degree interval
            // move counterclockwise
            for (float angle = -1.0f; angle >= -10.0f; angle -= 1.0f) 
            {
                float variance = estimateSkewAngle(angle);
                if (variance <= varianceBest)
                {
                    break;
                }

                varianceBest = variance;
                angleBest = angle;
            }

            if (angleBest == 0.0f)
            {
                // move clockwise
                for (float angle = 1.0f; angle <= 10.0f; angle += 1.0f)
                {
                    float variance = estimateSkewAngle(angle);
                    if (variance <= varianceBest)
                    {
                        break;
                    }

                    varianceBest = variance;
                    angleBest = angle;
                }
            }

            // move up or down with 0.1 degree interval
            // move counterclockwise
            float originalAngle = angleBest;
            for (float angle = angleBest - 0.1f, max = angleBest - 0.9f; angle >= max; angle -= 0.1f)
            {
                float variance = estimateSkewAngle(angle);
                if (variance <= varianceBest)
                {
                    break;
                }

                varianceBest = variance;
                angleBest = angle;
            }

            if (originalAngle == angleBest)
            {
                // move clockwise
                for (float angle = angleBest + 0.1f, max = angleBest + 0.9f; angle <= max; angle += 0.1f)
                {
                    float variance = estimateSkewAngle(angle);
                    if (variance <= varianceBest)
                    {
                        break;
                    }

                    varianceBest = variance;
                    angleBest = angle;
                }
            }

            return image.Rotate(angleBest);

            float estimateSkewAngle(float angle)
            {
                const float PiConv = 3.1415926535f / 180.0f;

                int centerX = width / 2;
                float dblTanA = (float)Math.Tan(angle * PiConv);

                float[] ds = new float[height];

                for (int ix = 0; ix < stride; ix++)
                {
                    // negative shift is down
                    int shiftY = (int)Math.Round(dblTanA * (centerX - (ix * 64) - 32), MidpointRounding.AwayFromZero);

                    Mathematics.Add(
                        height - Math.Abs(shiftY),
                        histogram[ix],
                        shiftY < 0 ? 0 : shiftY,
                        ds,
                        shiftY < 0 ? -shiftY : 0);
                }

                return ds.Variance();
            }
        }
    }
}
