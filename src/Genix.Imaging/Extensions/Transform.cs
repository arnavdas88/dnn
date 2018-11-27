// -----------------------------------------------------------------------
// <copyright file="Transform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Genix.Geometry;

    /// <content>
    /// Provides various transformation methods like rotation and mirroring for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Applies affine transformation described by the specified matrix to the <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Affine(Image dst, System.Windows.Media.Matrix matrix, BorderType borderType, uint borderValue)
        {
            const float Eps = 1e-8f;

            if (matrix.IsIdentity)
            {
                return this.Copy(dst, true);
            }

            // IPP does not support 1bpp images - convert to 8bpp
            Image src;
            bool convert1bpp = false;
            if (this.BitsPerPixel == 1)
            {
                src = this.Convert1To8(null);
                borderValue = borderValue != 0 ? 0u : 255u;
                convert1bpp = true;
            }
            else
            {
                src = this;
            }

            // calculate new image size and position
            PointD tr = TransformPoint(src.Width, 0);
            PointD br = TransformPoint(src.Width, src.Height);
            PointD bl = TransformPoint(0, src.Height);

            double x1dst = Core.MinMax.Min(bl.X, tr.X, br.X, 0.0);
            double x2dst = Core.MinMax.Max(bl.X, tr.X, br.X, 0.0);
            double y1dst = Core.MinMax.Min(bl.Y, tr.Y, br.Y, 0.0);
            double y2dst = Core.MinMax.Max(bl.Y, tr.Y, br.Y, 0.0);

            // translate matrix so the transformed image fits into new frame
            matrix.OffsetX = -Core.MinMax.Min(x1dst, x2dst);
            matrix.OffsetY = -Core.MinMax.Min(y1dst, y2dst);

            // note: add epsilon to avoid rounding problems
            int widthdst = (int)Math.Floor(x2dst - x1dst + Eps);
            int heightdst = (int)Math.Floor(y2dst - y1dst + Eps);

            bool inplace = dst == this;
            dst = src.CreateTemplate(dst, widthdst, heightdst, src.BitsPerPixel);

            IPP.Execute(() =>
            {
                return NativeMethods.affine(
                    src.BitsPerPixel,
                    src.Width,
                    src.Height,
                    src.Stride,
                    src.Bits,
                    dst.Width,
                    dst.Height,
                    dst.Stride,
                    dst.Bits,
                    matrix.M11,
                    matrix.M12,
                    matrix.OffsetX,
                    matrix.M21,
                    matrix.M22,
                    matrix.OffsetY,
                    (int)borderType,
                    borderValue);
            });

            dst.AppendTransform(new MatrixTransform(matrix));

            // convert back to 1bpp
            if (convert1bpp)
            {
                dst.Convert8To1(dst, 1);
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

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;

            PointD TransformPoint(int ptx, int pty)
            {
                return new PointD(
                    (matrix.M11 * ptx) + (matrix.M12 * pty) + matrix.OffsetX,
                    (matrix.M21 * ptx) + (matrix.M22 * pty) + matrix.OffsetY);
            }
        }

        /// <summary>
        /// Rotates the <see cref="Image"/> by an arbitrary angle.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="angle">The rotation angle, in degrees, counter-clockwise.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        [CLSCompliant(false)]
        public Image Rotate(Image dst, double angle, BorderType borderType, uint borderValue)
        {
            angle = angle % 360.0;
            float a = (float)(Math.PI * (angle / 180.0));
            if (Math.Abs(a) < 0.001f)
            {
                return this.Copy(dst, true);
            }

            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;
            matrix.Rotate(angle);

            return this.Affine(dst, matrix, borderType, borderValue);
        }

        /// <summary>
        /// Rotates, flips, or rotates and flips the <see cref="Image"/>, and returns re-sized image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="rotateFlipType">A <see cref="RotateFlip"/> member that specifies the type of rotation and flip to apply to the image.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        public Image RotateFlip(Image dst, RotateFlip rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case Imaging.RotateFlip.Rotate90FlipNone:
                case Imaging.RotateFlip.Rotate90FlipX:
                case Imaging.RotateFlip.Rotate90FlipY:
                    dst = this.Rotate90(dst);
                    break;

                case Imaging.RotateFlip.Rotate180FlipNone:
                case Imaging.RotateFlip.Rotate180FlipX:
                case Imaging.RotateFlip.Rotate180FlipY:
                    dst = this.Flip(dst, FlipAxis.Both);
                    break;

                case Imaging.RotateFlip.Rotate270FlipNone:
                case Imaging.RotateFlip.Rotate270FlipX:
                case Imaging.RotateFlip.Rotate270FlipY:
                    dst = this.Rotate270(dst);
                    break;

                default:
                    dst = this.Copy(dst, true);
                    break;
            }

            switch (rotateFlipType)
            {
                case Imaging.RotateFlip.RotateNoneFlipX:
                case Imaging.RotateFlip.Rotate90FlipX:
                case Imaging.RotateFlip.Rotate180FlipX:
                case Imaging.RotateFlip.Rotate270FlipX:
                    dst = dst.Flip(dst, FlipAxis.Y);
                    break;

                case Imaging.RotateFlip.RotateNoneFlipY:
                case Imaging.RotateFlip.Rotate90FlipY:
                case Imaging.RotateFlip.Rotate180FlipY:
                case Imaging.RotateFlip.Rotate270FlipY:
                    dst = dst.Flip(dst, FlipAxis.X);
                    break;
            }

            return dst;
        }

        /// <summary>
        /// Rotates this <see cref="Image"/> 90 degrees counter clockwise.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 1 nor 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Rotate90(Image dst)
        {
            if (this.BitsPerPixel != 1 && this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.Height, this.Width, this.BitsPerPixel);

            IPP.Execute(() =>
            {
                return NativeMethods.rotate90(
                   this.BitsPerPixel,
                   this.Width,
                   this.Height,
                   this.Bits,
                   this.Stride8,
                   dst.Bits,
                   dst.Stride8);
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Rotates this <see cref="Image"/> 90 degrees clockwise.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 1 nor 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Rotate270(Image dst)
        {
            if (this.BitsPerPixel != 1 && this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.Height, this.Width, this.BitsPerPixel);

            IPP.Execute(() =>
            {
                return NativeMethods.rotate270(
                   this.BitsPerPixel,
                   this.Width,
                   this.Height,
                   this.Bits,
                   this.Stride8,
                   dst.Bits,
                   dst.Stride8);
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Mirrors this <see cref="Image"/> about the specified axis.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="flip">The axis to flip the image about.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Flip(Image dst, FlipAxis flip)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            if (flip == Imaging.FlipAxis.None)
            {
                return this.Copy(dst, true);
            }
            else
            {
                dst = this.Copy(dst, false);

                IPP.Execute(() =>
                {
                    return NativeMethods.mirror(
                       this.BitsPerPixel,
                       0,
                       0,
                       this.Width,
                       this.Height,
                       this.Bits,
                       this.Stride8,
                       dst == this ? null : dst.Bits,
                       dst.Stride8,
                       flip);
                });

                return dst;
            }
        }

        /// <summary>
        /// Shears the <see cref="Image"/> by the specified amount.
        /// </summary>
        /// <param name="shearTan">Shearing force.
        /// Each horizontal string with Y coordinate equal to y is shifted horizontally by the <i>skew</i>*y pixels.
        /// </param>
        /// <returns>
        /// A new sheared <see cref="Image"/>.
        /// </returns>
        public Image Shear(double shearTan)
        {
            int maxoffset = Math.Abs((int)((shearTan * (this.Height - 1)) + 0.5)) * this.BitsPerPixel;

            Image dst = new Image(this.Width + maxoffset, this.Height, this);

            // allocate new DIB bits
            int widthsrc1 = this.WidthBits;
            int widthdst1 = dst.WidthBits;
            int stridesrc1 = this.Stride1;
            int stridedst1 = dst.Stride1;

            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;

            for (int iy = 0, possrc = 0, posdst = 0; iy < this.Height; iy++, possrc += stridesrc1, posdst += stridedst1)
            {
                int offset = Math.Abs((int)((shearTan * iy) + 0.5)) * this.BitsPerPixel;

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

            // TODO: add transform
            return dst;
        }

        /// <summary>
        /// De-skews the <see cref="Image"/> and aligns it horizontally.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not one.
        /// </exception>
        /// <remarks>
        /// <para>This method works with binary (1bpp) images only.</para>
        /// </remarks>
        public Image Deskew(Image dst)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            int width = this.Width;
            int height = this.Height;
            int stride = this.Stride;
            ulong[] bits = this.Bits;

            // build histogram
            ulong endMask = this.EndMask;
            float[][] histogram = new float[stride][];
            for (int ix = 0; ix < stride; ix++)
            {
                float[] h = histogram[ix] = new float[height];
                ulong mask = ix == stride - 1 ? endMask : ulong.MaxValue;
                for (int iy = 0, off = ix; iy < height; iy++, off += stride)
                {
                    h[iy] = BitUtils.CountOneBits(bits[off] & mask);
                }
            }

            // calculate image variance
            float angleBest = 0.0f;
            float varianceBest = EstimateSkewAngle(angleBest);

            // move up or down with 1 degree interval
            // move counterclockwise
            for (float angle = -1.0f; angle >= -10.0f; angle -= 1.0f)
            {
                float variance = EstimateSkewAngle(angle);
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
                    float variance = EstimateSkewAngle(angle);
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
                float variance = EstimateSkewAngle(angle);
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
                    float variance = EstimateSkewAngle(angle);
                    if (variance <= varianceBest)
                    {
                        break;
                    }

                    varianceBest = variance;
                    angleBest = angle;
                }
            }

            return this.Rotate(dst, -angleBest, BorderType.BorderRepl, 0);

            float EstimateSkewAngle(float angle)
            {
                const float PiConv = 3.1415926535f / 180.0f;

                int centerX = width / 2;
                float dblTanA = (float)Math.Tan(angle * PiConv);

                float[] ds = new float[height];

                for (int ix = 0; ix < stride; ix++)
                {
                    // negative shift is down
                    int shiftY = (dblTanA * (centerX - (ix * 64) - 32)).Round();

                    Vectors.Add(
                        height - Math.Abs(shiftY),
                        histogram[ix],
                        shiftY < 0 ? 0 : shiftY,
                        ds,
                        shiftY < 0 ? -shiftY : 0);
                }

                return ds.Variance();
            }
        }

        /// <summary>
        /// Detects straight lines in this <see cref="Image"/>.
        /// </summary>
        /// <param name="maxLineCount">Minimum number of lines to detect.</param>
        /// <param name="threshold">Minimum number of points that are required to detect the line.</param>
        /// <param name="deltaRho">Step of radial discretization.</param>
        /// <param name="deltaTheta">Step of angular discretization.</param>
        /// <returns>
        /// The detected lines.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bit per pixel.</para>
        /// </exception>
        public PointPolarF[] HoughLine(int maxLineCount, int threshold, float deltaRho, float deltaTheta)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            float[] lines = new float[2 * maxLineCount];
            int lineCount = 0;

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bits = this.Bits)
                    {
                        return NativeMethods.houghline(
                           0,
                           0,
                           this.Width,
                           this.Height,
                           (byte*)bits,
                           this.Stride8,
                           maxLineCount,
                           threshold,
                           deltaRho,
                           deltaTheta,
                           out lineCount,
                           lines);
                    }
                }
            });

            // convert answer
            PointPolarF[] result = new PointPolarF[lineCount];
            for (int i = 0, j = 0; i < lineCount; i++, j += 2)
            {
                result[i].Rho = lines[j];
                result[i].Theta = lines[j + 1];
            }

            return result;
        }

        /// <summary>
        /// Detects straight lines in this <see cref="Image"/>.
        /// </summary>
        /// <param name="maxLineCount">Minimum number of lines to detect.</param>
        /// <param name="threshold">Minimum number of points that are required to detect the line.</param>
        /// <param name="minLineLength">Minimum length of the line.</param>
        /// <param name="maxLineGap">Maximum length of the gap between lines.</param>
        /// <param name="deltaRho">Step of radial discretization.</param>
        /// <param name="deltaTheta">Step of angular discretization.</param>
        /// <returns>
        /// The detected lines.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bit per pixel.</para>
        /// </exception>
        public Line[] HoughProbLine(int maxLineCount, int threshold, int minLineLength, int maxLineGap, float deltaRho, float deltaTheta)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            int[] lines = new int[4 * maxLineCount];
            int lineCount = 0;

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bits = this.Bits)
                    {
                        return NativeMethods.houghprobline(
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bits,
                            this.Stride8,
                            maxLineCount,
                            threshold,
                            minLineLength,
                            maxLineGap,
                            deltaRho,
                            deltaTheta,
                            out lineCount,
                            lines);
                    }
                }
            });

            // convert answer
            Line[] result = new Line[lineCount];
            for (int i = 0, j = 0; i < lineCount; i++, j += 4)
            {
                result[i].X1 = lines[j];
                result[i].Y1 = lines[j + 1];
                result[i].X2 = lines[j + 2];
                result[i].Y2 = lines[j + 3];
            }

            return result;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
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
               double c12,
               int borderType,
               uint borderValue);

            [DllImport(NativeMethods.DllName)]
            public static extern int rotate90(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                [Out] ulong[] dst,
                int dststep);

            [DllImport(NativeMethods.DllName)]
            public static extern int rotate270(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                [Out] ulong[] dst,
                int dststep);

            [DllImport(NativeMethods.DllName)]
            public static extern int mirror(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                [In, Out] ulong[] src,
                int srcstep,
                [Out] ulong[] dst,
                int dststep,
                FlipAxis flip);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int houghline(
                int x,
                int y,
                int width,
                int height,
                [In] byte* src,
                int stridesrc,
                int maxLineCount,
                int threshold,
                float deltaRho,
                float deltaTheta,
                out int lineCount,
                [Out] float[] lines);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int houghprobline(
                int x,
                int y,
                int width,
                int height,
                [In] byte* src,
                int stridesrc,
                int maxLineCount,
                int threshold,
                int minLineLength,
                int maxLineGap,
                float deltaRho,
                float deltaTheta,
                out int lineCount,
                [Out] int[] lines);
        }
    }
}
