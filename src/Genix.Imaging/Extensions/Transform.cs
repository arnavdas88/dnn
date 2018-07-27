// -----------------------------------------------------------------------
// <copyright file="Transform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Leptonica;

    /// <summary>
    /// Provides various transformation methods like rotation and mirroring for the <see cref="Image"/> class.
    /// </summary>
    public static class Transform
    {
        /// <summary>
        /// Applies affine transformation described by the specified matrix to the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to rotate.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>
        /// A new transformed <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Affine(this Image image, System.Windows.Media.Matrix matrix)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (matrix.IsIdentity)
            {
                return image.Copy();
            }

            // calculate new image size and position
            System.Windows.Point tr = transformPoint(image.Width - 1, 0);
            System.Windows.Point br = transformPoint(image.Width - 1, image.Height - 1);
            System.Windows.Point bl = transformPoint(0, image.Height - 1);

            double x1dst = Maximum.Min(bl.X, tr.X, br.X, 0.0);
            double x2dst = Maximum.Max(bl.X, tr.X, br.X, 0.0);
            double y1dst = Maximum.Min(bl.Y, tr.Y, br.Y, 0.0);
            double y2dst = Maximum.Max(bl.Y, tr.Y, br.Y, 0.0);

            int widthdst = (int)Math.Round(x2dst - x1dst, MidpointRounding.AwayFromZero) + 1;
            int heightdst = (int)Math.Round(y2dst - y1dst, MidpointRounding.AwayFromZero) + 1;

            // translate matrix so the transformed image fits into new frame
            matrix.OffsetX = -Maximum.Min(x1dst, x2dst);
            matrix.OffsetY = -Maximum.Min(y1dst, y2dst);

            // IPP does not support 1bpp images - convert to 8bpp
            bool convert1bpp = false;
            if (image.BitsPerPixel == 1)
            {
                image = image.Convert1To8(255, 0);
                convert1bpp = true;
            }

            Image transformedImage = new Image(widthdst, heightdst, image);

            if (NativeMethods.affine(
                image.BitsPerPixel,
                image.Width,
                image.Height,
                image.Stride,
                image.Bits,
                transformedImage.Width,
                transformedImage.Height,
                transformedImage.Stride,
                transformedImage.Bits,
                matrix.M11,
                matrix.M12,
                matrix.OffsetX,
                matrix.M21,
                matrix.M22,
                matrix.OffsetY) != 0)
            {
                throw new OutOfMemoryException();
            }

            // convert back to 1bpp
            if (convert1bpp)
            {
                transformedImage = transformedImage.Convert8To1(1);
                /*using (Pix pixs = transformedImage.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        if (pixd != null)
                        {
                            return pixd.CreateImage(transformedImage.HorizontalResolution, transformedImage.VerticalResolution);
                        }
                    }
                }*/
            }

            return transformedImage;

            System.Windows.Point transformPoint(int ptx, int pty)
            {
                return new System.Windows.Point(
                    (matrix.M11 * ptx) + (matrix.M12 * pty) + matrix.OffsetX,
                    (matrix.M21 * ptx) + (matrix.M22 * pty) + matrix.OffsetY);
            }
        }

        /// <summary>
        /// Rotates the <see cref="Image"/> by an arbitrary angle.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to rotate.</param>
        /// <param name="angle">The rotation angle, in degrees, counter-clockwise.</param>
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

            angle = angle % 360.0;
            if (angle == 0.0)
            {
                return image.Copy();
            }

            float a = (float)(Math.PI * (angle / 180.0));
            if (Math.Abs(a) < 0.001f)
            {
                return image.Copy();
            }

            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;
            matrix.Rotate(angle);

            return image.Affine(matrix);
        }

        /// <summary>
        /// Rotates, flips, or rotates and flips this <see cref="Image"/>, and returns re-sized image.
        /// </summary>
        /// <param name="image">The image to rotate.</param>
        /// <param name="rotateFlipType">A <see cref="RotateFlip"/> member that specifies the type of rotation and flip to apply to the image.</param>
        /// <returns>A rotated <see cref="Image"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        public static Image RotateFlip(this Image image, RotateFlip rotateFlipType)
        {
            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;

            switch (rotateFlipType)
            {
                case Imaging.RotateFlip.Rotate90FlipNone:
                case Imaging.RotateFlip.Rotate90FlipX:
                case Imaging.RotateFlip.Rotate90FlipY:
                    matrix.Rotate(90);
                    break;

                case Imaging.RotateFlip.Rotate180FlipNone:
                case Imaging.RotateFlip.Rotate180FlipX:
                case Imaging.RotateFlip.Rotate180FlipY:
                    matrix.Rotate(180);
                    break;

                case Imaging.RotateFlip.Rotate270FlipNone:
                case Imaging.RotateFlip.Rotate270FlipX:
                case Imaging.RotateFlip.Rotate270FlipY:
                    matrix.Rotate(270);
                    break;
            }

            switch (rotateFlipType)
            {
                case Imaging.RotateFlip.RotateNoneFlipX:
                case Imaging.RotateFlip.Rotate90FlipX:
                case Imaging.RotateFlip.Rotate180FlipX:
                case Imaging.RotateFlip.Rotate270FlipX:
                    matrix.Append(new System.Windows.Media.Matrix(-1, 0, 0, 1, 0, 0));
                    break;

                case Imaging.RotateFlip.RotateNoneFlipY:
                case Imaging.RotateFlip.Rotate90FlipY:
                case Imaging.RotateFlip.Rotate180FlipY:
                case Imaging.RotateFlip.Rotate270FlipY:
                    matrix.Append(new System.Windows.Media.Matrix(1, 0, 0, -1, 0, 0));
                    break;
            }

            return image.Affine(matrix);
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
            ulong endMask = image.EndMask;
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

            return image.Rotate(-angleBest);

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

        private static class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int affine(
                int bitsPerPixel,
                int widthsrc,
                int heightsrc,
                int stridesrc,
                [In] ulong[] src,
                int widthdst,
                int heightdst,
                int stridedst,
                [Out] ulong[] dst,
                double c00,
                double c01,
                double c02,
                double c10,
                double c11,
                double c12);
        }
    }
}
