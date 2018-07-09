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
