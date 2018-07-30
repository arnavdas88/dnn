// -----------------------------------------------------------------------
// <copyright file="Scaling.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <summary>
    /// Provides scaling extension methods for the <see cref="Image"/> class.
    /// </summary>
    public partial class Image
    {
        /// <summary>
        /// Scales this <see cref="Image"/> proportionally in both dimensions without changing its resolution.
        /// </summary>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        public Image Scale(double scaleFactor, ScalingOptions options)
        {
            int newWidth = (int)((this.Width * scaleFactor) + 0.5);
            int newHeight = (int)((this.Height * scaleFactor) + 0.5);

            return this.ScaleToSize(newWidth, newHeight, options);
        }

        /// <summary>
        /// Scales this <see cref="Image"/> vertically and horizontally without changing its resolution.
        /// </summary>
        /// <param name="width">The desired width of the image, in pixels.</param>
        /// <param name="height">The desired height of the image, in pixels.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        public Image ScaleToSize(int width, int height, ScalingOptions options)
        {
            if (width == this.Width && height == this.Height)
            {
                return this.Copy();
            }

            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;
            matrix.Scale((double)width / this.Width, (double)height / this.Height);

            Image dst = this.Affine(matrix);
            Debug.Assert(width == dst.Width && height == dst.Height, "Image dimensions are wrong.");
            return dst;
#if false

            if (image.BitsPerPixel == 1 &&
                width > 16 &&
                height > 16 &&
                options.HasFlag(ScalingOptions.Upscale1Bpp))
            {
#if true
                Image image8bpp = image.Convert1To8(255, 0);
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
                            return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                        }
                    }
                }
#else
                using (Pix pixs = image.Convert1To8(255, 0).CreatePix())
                {
                    using (Pix pixd1 = pixs.pixScaleToSize(width, height))
                    {
                        using (Pix pixd = pixd1.pixOtsu(false))
                        {
                            if (pixd != null)
                            {
                                return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                            }
                        }
                    }
                }
#endif
            }

            using (Pix pixs = image.CreatePix())
            {
                using (Pix pixd = pixs.pixScaleToSize(width, height))
                {
                    return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
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
            Image image = this;

            if (image.Width > width || image.Height > height)
            {
                Rectangle blackArea = image.BlackArea();
                if (!blackArea.IsEmpty && blackArea != image.Bounds)
                {
                    image = image.Crop(blackArea);
                }
            }

            if (image.Width > width || image.Height > height)
            {
                double horizontalFactor = (double)width / image.Width;
                double verticalFactor = (double)height / image.Height;
                double scaleFactor = Math.Min(horizontalFactor, verticalFactor);

                int newWidth = (int)((image.Width * scaleFactor) + 0.5f);
                int newHeight = (int)((image.Height * scaleFactor) + 0.5f);

                if (image.Width != newWidth || image.Height != newHeight)
                {
                    image = image.ScaleToSize(newWidth, newHeight, options);
                }
            }

            if (image.Width < width || image.Height < height)
            {
                int dx = width - image.Width;
                int dy = height - image.Height;
                image = image.Inflate(dx / 2, dy / 2, dx - (dx / 2), dy - (dy / 2));
            }

            return image == this ? image.Copy() : image;
        }

        /// <summary>
        /// Changes the size of this <see cref="Image"/> to the specified dimensions without changing its scale.
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

            return this.Inflate(0, 0, width - this.Width, height - this.Height);
        }

        /// <summary>
        /// Creates and returns an enlarged copy of the specified <see cref="Image"/>.
        /// Positive input parameters mean inflating, negative mean cropping.
        /// </summary>
        /// <param name="left">The amount by which to expand or shrink the left side of the <see cref="Image"/>.</param>
        /// <param name="top">The amount by which to expand or shrink the top side of the <see cref="Image"/>.</param>
        /// <param name="right">The amount by which to expand or shrink the right side of the <see cref="Image"/>. </param>
        /// <param name="bottom">The amount by which to expand or shrink the bottom side of the <see cref="Image"/>.</param>
        /// <returns>
        /// A new inflated <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>Result width is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para>Result height is less than or equal to zero.</para>
        /// </exception>
        public Image Inflate(int left, int top, int right, int bottom)
        {
            // calculate and verify target area in source coordinates
            System.Drawing.Rectangle bounds = System.Drawing.Rectangle.FromLTRB(
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

            Image dst = new Image(bounds.Width, bounds.Height, this);

            // calculate source area to copy from
            Rectangle area = Rectangle.Intersect(bounds, this.Bounds);

            // calculate destination area to copy to
            int dstx = area.X - bounds.X;
            int dsty = area.Y - bounds.Y;

            Image.CopyArea(this, area.X, area.Y, area.Width, area.Height, dst, dstx, dsty);

            if (this.BitsPerPixel > 1)
            {
                // set frame to white
                dst.SetWhiteBorderIP(dstx, dsty, area.Width, area.Height);
            }

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
                Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
            }

            if ((this.Height & 1) != 0)
            {
                Arrays.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
            }

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
                Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitsdst, offdst);
            }

            switch (this.Height % 3)
            {
                case 1:
                    Arrays.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
                    break;

                case 2:
                    Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
                    break;
            }

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
                Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitssrc, offsrc + (3 * stride), bitsdst, offdst);
            }

            switch (this.Height % 4)
            {
                case 1:
                    Arrays.Copy(stride, bitssrc, offsrc, bitsdst, offdst);
                    break;

                case 2:
                    Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitsdst, offdst);
                    break;

                case 3:
                    Arrays.OR(stride, bitssrc, offsrc, bitssrc, offsrc + stride, bitssrc, offsrc + (2 * stride), bitsdst, offdst);
                    break;
            }

            return dst;
        }

        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int scale8(
               int widthsrc,
               int heightsrc,
               int stridesrc,
               [In] ulong[] src,
               int widthdst,
               int heightdst,
               int stridedst,
               [Out] ulong[] dst);
        }
    }
}
