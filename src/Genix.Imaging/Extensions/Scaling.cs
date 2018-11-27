// -----------------------------------------------------------------------
// <copyright file="Scaling.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Genix.Geometry;

    /// <summary>
    /// Provides scaling extension methods for the <see cref="Image"/> class.
    /// </summary>
    public partial class Image
    {
        /// <summary>
        /// Scales the <see cref="Image"/> proportionally in both dimensions without changing its resolution.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Scale(Image dst, double scaleFactor, ScalingOptions options) =>
            this.Scale(dst, scaleFactor, scaleFactor, options);

        /// <summary>
        /// Scales the <see cref="Image"/> in both dimensions without changing its resolution.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="scaleFactorX">The horizontal scaling factor.</param>
        /// <param name="scaleFactorY">The vertical scaling factor.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        public Image Scale(Image dst, double scaleFactorX, double scaleFactorY, ScalingOptions options)
        {
            int newWidth = (int)((this.Width * scaleFactorX) + 0.5);
            int newHeight = (int)((this.Height * scaleFactorY) + 0.5);

            return this.ScaleToSize(dst, newWidth, newHeight, options);
        }

        /// <summary>
        /// Scales the <see cref="Image"/> vertically and horizontally without changing its resolution.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="width">The desired width of the image, in pixels.</param>
        /// <param name="height">The desired height of the image, in pixels.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        public Image ScaleToSize(Image dst, int width, int height, ScalingOptions options)
        {
            if (width == this.Width && height == this.Height)
            {
                return this.Copy(dst, true);
            }

            if (width <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidWidth, nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHeight, nameof(height));
            }

            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;
            matrix.Scale((double)width / this.Width, (double)height / this.Height);

            dst = this.Affine(dst, matrix, BorderType.BorderConst, this.WhiteColor);
            Debug.Assert(width == dst.Width && height == dst.Height, "Image dimensions are wrong.");
            return dst;
#if false

            if (this.BitsPerPixel == 1 &&
                width > 16 &&
                height > 16 &&
                options.HasFlag(ScalingOptions.Upscale1Bpp))
            {
#if true
                Image image8bpp = this.Convert1To8(255, 0);
                Image imageScaled = new Image(width, height, image8bpp);

                // swap bytes to little-endian and back
                if (NativeMethods.scale8(
                    image8bpp.Width,
                    image8bpp.Height,
                    image8bpp.Stride,
                    image8bpp.Bits,
                    imageScaled.Width,
                    imageScaled.Height,
                    imageScaled.Stride,
                    imageScaled.Bits) != 0)
                {
                    throw new OutOfMemoryException();
                }

                using (Pix pixs = imageScaled.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        if (pixd != null)
                        {
                            return pixd.CreateImage(this.HorizontalResolution, this.VerticalResolution);
                        }
                    }
                }
#else
                using (Pix pixs = this.Convert1To8(255, 0).CreatePix())
                {
                    using (Pix pixd1 = pixs.pixScaleToSize(width, height))
                    {
                        using (Pix pixd = pixd1.pixOtsu(false))
                        {
                            if (pixd != null)
                            {
                                return pixd.CreateImage(this.HorizontalResolution, this.VerticalResolution);
                            }
                        }
                    }
                }
#endif
            }

            using (Pix pixs = this.CreatePix())
            {
                using (Pix pixd = pixs.pixScaleToSize(width, height))
                {
                    return pixd.CreateImage(this.HorizontalResolution, this.VerticalResolution);
                }
            }
#endif
        }

        /// <summary>
        /// Fits the <see cref="Image"/> into the area of specified dimensions.
        /// </summary>
        /// <param name="width">The width, in pixels, of the required image.</param>
        /// <param name="height">The height, in pixels, of the required image.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The operation cannot be performed on the image with current color depth.
        /// </exception>
        /// <remarks>
        /// This method modifies the current <see cref="Image"/> by resizing and inflating it when necessary.
        /// The resulting image should be <paramref name="width"/> x <paramref name="height"/> in size.
        /// </remarks>
        public Image FitToSize(int width, int height, ScalingOptions options)
        {
            Image dst = this;

            // if image is bigger than required, crop black area first
            // try to preserve black area relative position within the image
            if (dst.Width > width || dst.Height > height)
            {
                Rectangle blackArea = dst.BlackArea();
                if (!blackArea.IsEmpty && blackArea != dst.Bounds)
                {
                    ////dst = dst.Crop(blackArea);

                    if (blackArea.Height < height)
                    {
                        int dy = Core.MinMax.Min((height - blackArea.Height) / 2, blackArea.Y, dst.Height - blackArea.Bottom);
                        blackArea.Inflate(0, dy, 0, height - blackArea.Height - dy);
                    }

                    if (width > 0 && blackArea.Width < width)
                    {
                        int dx = Core.MinMax.Min((width - blackArea.Width) / 2, blackArea.X, dst.Width - blackArea.Right);
                        blackArea.Inflate(dx, 0, width - blackArea.Width - dx, 0);
                    }

                    blackArea.Intersect(dst.Bounds);
                    if (blackArea != dst.Bounds)
                    {
                        dst = dst.Crop(blackArea);
                    }
                }
            }

            if (dst.Width > width || dst.Height > height)
            {
                double horizontalFactor = (double)width / dst.Width;
                double verticalFactor = (double)height / dst.Height;
                double scaleFactor = Core.MinMax.Min(horizontalFactor, verticalFactor);

                int newWidth = (int)((dst.Width * scaleFactor) + 0.5f);
                int newHeight = (int)((dst.Height * scaleFactor) + 0.5f);

                if (dst.Width != newWidth || dst.Height != newHeight)
                {
                    dst.ScaleToSize(dst, newWidth, newHeight, options);
                }
            }

            if (dst.Width < width || dst.Height < height)
            {
                int dx = width - dst.Width;
                int dy = height - dst.Height;
                dst = dst.Inflate(dx / 2, dy / 2, dx - (dx / 2), dy - (dy / 2), BorderType.BorderConst, dst.WhiteColor);
            }

            return dst != this ? dst : dst.Copy(null, true);
        }

        /// <summary>
        /// Changes the size of the <see cref="Image"/> to the specified dimensions without changing its scale.
        /// </summary>
        /// <param name="width">The desired width of the image, in pixels.</param>
        /// <param name="height">The desired height of the image, in pixels.</param>
        /// <returns>
        /// A new resized <see cref="Image"/>.
        /// </returns>
        public Image Resize(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidWidth);
            }

            if (height <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHeight);
            }

            return this.Inflate(0, 0, width - this.Width, height - this.Height, BorderType.BorderConst, this.WhiteColor);
        }

        /// <summary>
        /// Creates and returns an enlarged copy of the specified <see cref="Image"/>.
        /// Positive input parameters mean inflating, negative mean cropping.
        /// </summary>
        /// <param name="left">The amount by which to expand or shrink the left side of the <see cref="Image"/>.</param>
        /// <param name="top">The amount by which to expand or shrink the top side of the <see cref="Image"/>.</param>
        /// <param name="right">The amount by which to expand or shrink the right side of the <see cref="Image"/>. </param>
        /// <param name="bottom">The amount by which to expand or shrink the bottom side of the <see cref="Image"/>.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// A new inflated <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>Result width is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para>Result height is less than or equal to zero.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image Inflate(int left, int top, int right, int bottom, BorderType borderType, uint borderValue)
        {
            // calculate and verify target area in source coordinates
            Rectangle bounds = Rectangle.FromLTRB(
                -left,
                -top,
                this.Width + right,
                this.Height + bottom);

            if (bounds.Width <= 0)
            {
                throw new ArgumentException("The new image width is invalid.");
            }

            if (bounds.Height <= 0)
            {
                throw new ArgumentException("The new image height is invalid.");
            }

            Image dst = new Image(bounds.Size, this);

            // calculate source area to copy from
            Rectangle srcarea = Rectangle.Intersect(bounds, this.Bounds);

            // calculate destination area to copy to
            Rectangle dstarea = Rectangle.Offset(srcarea, -bounds.X, -bounds.Y);

            Image.CopyArea(dst, dstarea.X, dstarea.Y, srcarea.Width, srcarea.Height, this, srcarea.X, srcarea.Y);

            // set border
            dst.SetBorder(dstarea, borderType, borderValue);

            dst.AppendTransform(new MatrixTransform(left, top));
            return dst;
        }

        /// <summary>
        /// Reduces the height of the <see cref="Image"/> by the factor of 2.
        /// </summary>
        /// <returns>The scaled <see cref="Image"/>.</returns>
        public Image Reduce1x2()
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                (this.Height + 1) >> 1,
                this.BitsPerPixel,
                this.HorizontalResolution,
                this.VerticalResolution / 2);

            int stride = this.Stride;
            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;

            int offsrc = 0;
            int offdst = 0;
            for (int i = 0, ii = this.Height >> 1; i < ii; i++, offsrc += 2 * stride, offdst += stride)
            {
                Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
            }

            if ((this.Height & 1) != 0)
            {
                Vectors.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
            }

            dst.AppendTransform(new MatrixTransform(1.0, 0.5));
            return dst;
        }

        /// <summary>
        /// Reduces the height of the <see cref="Image"/> by the factor of 3.
        /// </summary>
        /// <returns>The scaled <see cref="Image"/>.</returns>
        public Image Reduce1x3()
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                (this.Height + 2) / 3,
                this.BitsPerPixel,
                this.HorizontalResolution,
                this.VerticalResolution / 3);

            int stride = this.Stride;
            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;

            int offsrc = 0;
            int offdst = 0;
            for (int i = 0, ii = this.Height / 3; i < ii; i++, offsrc += 3 * stride, offdst += stride)
            {
                Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitsdst, offdst);
            }

            switch (this.Height % 3)
            {
                case 1:
                    Vectors.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
                    break;

                case 2:
                    Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
                    break;
            }

            dst.AppendTransform(new MatrixTransform(1.0, 1.0 / 3));
            return dst;
        }

        /// <summary>
        /// Reduces the height of the <see cref="Image"/> by the factor of 4.
        /// </summary>
        /// <returns>The scaled <see cref="Image"/>.</returns>
        public Image Reduce1x4()
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                this.Width,
                (this.Height + 3) / 4,
                this.BitsPerPixel,
                this.HorizontalResolution,
                this.VerticalResolution / 4);

            int stride = this.Stride;
            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;

            int offsrc = 0;
            int offdst = 0;
            for (int i = 0, ii = this.Height / 4; i < ii; i++, offsrc += 4 * stride, offdst += stride)
            {
                Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitssrc, offsrc + (3 * stride), bitsdst, offdst);
            }

            switch (this.Height % 4)
            {
                case 1:
                    Vectors.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
                    break;

                case 2:
                    Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
                    break;

                case 3:
                    Vectors.Or(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitsdst, offdst);
                    break;
            }

            dst.AppendTransform(new MatrixTransform(1.0, 0.25));
            return dst;
        }

        /// <summary>
        /// Reduces the size and resolution of this <see cref="Image"/> by a factor of 2 by downsampling.
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
        public Image ScaleByDownsampling2(Image dst)
        {
            int width = this.Width;
            int height = this.Height;
            int bitsPerPixel = this.BitsPerPixel;
            int dstwidth = width / 2;
            int dstheight = height / 2;

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, dstwidth, dstheight, bitsPerPixel);

            switch (bitsPerPixel)
            {
                case 1:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            uint* ptrdst = (uint*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride32dst = dst.Stride * 2;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stridesrc, ptrdst += stride32dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 32, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (uint)(
                                        (bits & 0x0000_0001) |
                                        ((bits >> 1) & 0x0000_0002) |
                                        ((bits >> 2) & 0x0000_0004) |
                                        ((bits >> 3) & 0x0000_0008) |
                                        ((bits >> 4) & 0x0000_0010) |
                                        ((bits >> 5) & 0x0000_0020) |
                                        ((bits >> 6) & 0x0000_0040) |
                                        ((bits >> 7) & 0x0000_0080) |
                                        ((bits >> 8) & 0x0000_0100) |
                                        ((bits >> 9) & 0x0000_0200) |
                                        ((bits >> 10) & 0x0000_0400) |
                                        ((bits >> 11) & 0x0000_0800) |
                                        ((bits >> 12) & 0x0000_1000) |
                                        ((bits >> 13) & 0x0000_2000) |
                                        ((bits >> 14) & 0x0000_4000) |
                                        ((bits >> 15) & 0x0000_8000) |
                                        ((bits >> 16) & 0x0001_0000) |
                                        ((bits >> 17) & 0x0002_0000) |
                                        ((bits >> 18) & 0x0004_0000) |
                                        ((bits >> 19) & 0x0008_0000) |
                                        ((bits >> 20) & 0x0010_0000) |
                                        ((bits >> 21) & 0x0020_0000) |
                                        ((bits >> 22) & 0x0040_0000) |
                                        ((bits >> 23) & 0x0080_0000) |
                                        ((bits >> 24) & 0x0100_0000) |
                                        ((bits >> 25) & 0x0200_0000) |
                                        ((bits >> 26) & 0x0400_0000) |
                                        ((bits >> 27) & 0x0800_0000) |
                                        ((bits >> 28) & 0x1000_0000) |
                                        ((bits >> 29) & 0x2000_0000) |
                                        ((bits >> 30) & 0x4000_0000) |
                                        ((bits >> 31) & 0x8000_0000));
                                }
                            }
                        }
                    }

                    break;

                case 2:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            uint* ptrdst = (uint*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride32dst = dst.Stride * 2;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stridesrc, ptrdst += stride32dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 16, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (uint)(
                                        (bits & 0x0000_0003) |
                                        ((bits >> 2) & 0x0000_000c) |
                                        ((bits >> 4) & 0x0000_0030) |
                                        ((bits >> 6) & 0x0000_00c0) |
                                        ((bits >> 8) & 0x0000_0300) |
                                        ((bits >> 10) & 0x0000_0c00) |
                                        ((bits >> 12) & 0x0000_3000) |
                                        ((bits >> 14) & 0x0000_c000) |
                                        ((bits >> 16) & 0x0003_0000) |
                                        ((bits >> 18) & 0x000c_0000) |
                                        ((bits >> 20) & 0x0030_0000) |
                                        ((bits >> 22) & 0x00c0_0000) |
                                        ((bits >> 24) & 0x0300_0000) |
                                        ((bits >> 26) & 0x0c00_0000) |
                                        ((bits >> 28) & 0x3000_0000) |
                                        ((bits >> 30) & 0xc000_0000));
                                }
                            }
                        }
                    }

                    break;

                case 4:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            uint* ptrdst = (uint*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride32dst = dst.Stride * 2;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stridesrc, ptrdst += stride32dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 8, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (uint)(
                                        (bits & 0x0000_000f) |
                                        ((bits >> 4) & 0x0000_00f0) |
                                        ((bits >> 8) & 0x0000_0f00) |
                                        ((bits >> 12) & 0x0000_f000) |
                                        ((bits >> 16) & 0x000f_0000) |
                                        ((bits >> 20) & 0x00f0_0000) |
                                        ((bits >> 24) & 0x0f00_0000) |
                                        ((bits >> 28) & 0xf000_0000));
                                }
                            }
                        }
                    }

                    break;

                case 8:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            byte* ptrsrc = (byte*)bitssrc;
                            byte* ptrdst = (byte*)bitsdst;
                            int stride8src = this.Stride8;
                            int stride8dst = dst.Stride8;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stride8src, ptrdst += stride8dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 2)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                case 16:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ushort* ptrsrc = (ushort*)bitssrc;
                            ushort* ptrdst = (ushort*)bitsdst;
                            int stride16src = this.Stride * 4;
                            int stride16dst = dst.Stride * 4;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stride16src, ptrdst += stride16dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 2)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                case 24:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            byte* ptrsrc = (byte*)bitssrc;
                            byte* ptrdst = (byte*)bitsdst;
                            int stride8src = this.Stride8;
                            int stride8dst = dst.Stride8;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stride8src, ptrdst += stride8dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < 3 * dstwidth; xdst += 3, xsrc += 2 * 3)
                                {
                                    ptrdst[xdst + 0] = ptrsrc[xsrc + 0];
                                    ptrdst[xdst + 1] = ptrsrc[xsrc + 1];
                                    ptrdst[xdst + 2] = ptrsrc[xsrc + 2];
                                }
                            }
                        }
                    }

                    break;

                case 32:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            uint* ptrsrc = (uint*)bitssrc;
                            uint* ptrdst = (uint*)bitsdst;
                            int stride32src = this.Stride * 2;
                            int stride32dst = dst.Stride * 2;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 2 * stride32src, ptrdst += stride32dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 2)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        bitsPerPixel));
            }

            dst.SetResolution(this.HorizontalResolution / 2, this.VerticalResolution / 2);
            dst.AppendTransform(new MatrixTransform(0.5, 0.5));

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Reduces the size and resolution of this <see cref="Image"/> by a factor of 4 by downsampling.
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
        public Image ScaleByDownsampling4(Image dst)
        {
            int width = this.Width;
            int height = this.Height;
            int bitsPerPixel = this.BitsPerPixel;
            int dstwidth = width / 4;
            int dstheight = height / 4;

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, dstwidth, dstheight, bitsPerPixel);

            switch (bitsPerPixel)
            {
                case 1:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            ushort* ptrdst = (ushort*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride16dst = dst.Stride * 4;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stridesrc, ptrdst += stride16dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 16, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (ushort)(
                                        (bits & 0x0001) |
                                        ((bits >> 3) & 0x0002) |
                                        ((bits >> 6) & 0x0004) |
                                        ((bits >> 9) & 0x0008) |
                                        ((bits >> 12) & 0x0010) |
                                        ((bits >> 15) & 0x0020) |
                                        ((bits >> 18) & 0x0040) |
                                        ((bits >> 21) & 0x0080) |
                                        ((bits >> 24) & 0x0100) |
                                        ((bits >> 27) & 0x0200) |
                                        ((bits >> 30) & 0x0400) |
                                        ((bits >> 33) & 0x0800) |
                                        ((bits >> 36) & 0x1000) |
                                        ((bits >> 39) & 0x2000) |
                                        ((bits >> 42) & 0x4000) |
                                        ((bits >> 45) & 0x8000));
                                }
                            }
                        }
                    }

                    break;

                case 2:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            ushort* ptrdst = (ushort*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride16dst = dst.Stride * 4;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stridesrc, ptrdst += stride16dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 8, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (ushort)(
                                        (bits & 0x0003) |
                                        ((bits >> 6) & 0x000c) |
                                        ((bits >> 12) & 0x0030) |
                                        ((bits >> 18) & 0x00c0) |
                                        ((bits >> 24) & 0x0300) |
                                        ((bits >> 30) & 0x0c00) |
                                        ((bits >> 36) & 0x3000) |
                                        ((bits >> 42) & 0xc000));
                                }
                            }
                        }
                    }

                    break;

                case 4:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ulong* ptrsrc = (ulong*)bitssrc;
                            ushort* ptrdst = (ushort*)bitsdst;
                            int stridesrc = this.Stride;
                            int stride16dst = dst.Stride * 4;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stridesrc, ptrdst += stride16dst)
                            {
                                for (int x = 0, xdst = 0, xsrc = 0; x < dstwidth; x += 4, xdst++, xsrc++)
                                {
                                    ulong bits = ptrsrc[xsrc];
                                    ptrdst[xdst] = (ushort)(
                                        (bits & 0x000f) |
                                        ((bits >> 12) & 0x00f0) |
                                        ((bits >> 24) & 0x0f00) |
                                        ((bits >> 36) & 0xf000));
                                }
                            }
                        }
                    }

                    break;

                case 8:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            byte* ptrsrc = (byte*)bitssrc;
                            byte* ptrdst = (byte*)bitsdst;
                            int stride8src = this.Stride8;
                            int stride8dst = dst.Stride8;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stride8src, ptrdst += stride8dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 4)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                case 16:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            ushort* ptrsrc = (ushort*)bitssrc;
                            ushort* ptrdst = (ushort*)bitsdst;
                            int stride16src = this.Stride * 4;
                            int stride16dst = dst.Stride * 4;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stride16src, ptrdst += stride16dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 4)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                case 24:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            byte* ptrsrc = (byte*)bitssrc;
                            byte* ptrdst = (byte*)bitsdst;
                            int stride8src = this.Stride8;
                            int stride8dst = dst.Stride8;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stride8src, ptrdst += stride8dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < 3 * dstwidth; xdst += 3, xsrc += 4 * 3)
                                {
                                    ptrdst[xdst + 0] = ptrsrc[xsrc + 0];
                                    ptrdst[xdst + 1] = ptrsrc[xsrc + 1];
                                    ptrdst[xdst + 2] = ptrsrc[xsrc + 2];
                                }
                            }
                        }
                    }

                    break;

                case 32:
                    unsafe
                    {
                        fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                        {
                            uint* ptrsrc = (uint*)bitssrc;
                            uint* ptrdst = (uint*)bitsdst;
                            int stride32src = this.Stride * 2;
                            int stride32dst = dst.Stride * 2;

                            for (int ydst = 0; ydst < dstheight; ydst++, ptrsrc += 4 * stride32src, ptrdst += stride32dst)
                            {
                                for (int xdst = 0, xsrc = 0; xdst < dstwidth; xdst++, xsrc += 4)
                                {
                                    ptrdst[xdst] = ptrsrc[xsrc];
                                }
                            }
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        bitsPerPixel));
            }

            dst.SetResolution(this.HorizontalResolution / 4, this.VerticalResolution / 4);
            dst.AppendTransform(new MatrixTransform(0.25, 0.25));

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern int resize(
                int bitsPerPixel,
                int widthsrc,
                int heightsrc,
                [In] ulong[] src,
                int stridesrc,
                int widthdst,
                int heightdst,
                [Out] ulong[] dst,
                int stridedst,
                InterpolationType interpolationType,
                [MarshalAs(UnmanagedType.Bool)] bool antializing,
                float valueB,
                float valueC,
                uint numLobes,
                BorderType borderType,
                uint borderValue);
        }
    }
}
