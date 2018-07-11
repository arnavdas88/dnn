// -----------------------------------------------------------------------
// <copyright file="ScaleResize.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Drawing;
    using Leptonica;

    /// <summary>
    /// Provides scaling extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class ScaleResize
    {
        /// <summary>
        /// Scales this <see cref="Image"/> proportionally in both dimensions without changing its resolution.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to scale.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        public static Image Scale(this Image image, double scaleFactor, ScalingOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int newWidth = (int)((image.Width * scaleFactor) + 0.5);
            int newHeight = (int)((image.Height * scaleFactor) + 0.5);

            return image.ScaleToSize(newWidth, newHeight, options);
        }

        /// <summary>
        /// Scales this <see cref="Image"/> vertically and horizontally without changing its resolution.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to scale.</param>
        /// <param name="width">The desired width of the image, in pixels.</param>
        /// <param name="height">The desired height of the image, in pixels.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        public static Image ScaleToSize(this Image image, int width, int height, ScalingOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (width == image.Width && height == image.Height)
            {
                return image.Copy();
            }

            if (image.BitsPerPixel == 1 && width > 16 && height > 16 && options.HasFlag(ScalingOptions.Upscale1Bpp))
            {
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
            }

            using (Pix pixs = image.CreatePix())
            {
                using (Pix pixd = pixs.pixScaleToSize(width, height))
                {
                    return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                }
            }
        }

        /// <summary>
        /// Fits the <see cref="Image"/> into the area of specified dimensions.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to resize.</param>
        /// <param name="width">The width, in pixels, of the required image.</param>
        /// <param name="height">The height, in pixels, of the required image.</param>
        /// <param name="options">The scaling options.</param>
        /// <returns>
        /// A new scaled <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The operation cannot be performed on the image with current color depth.
        /// </exception>
        /// <remarks>
        /// This method modifies the current <see cref="Image"/> by resizing and inflating it when necessary.
        /// The resulting image should be <c>width</c> x <c>height</c> in size.
        /// </remarks>
        public static Image FitToSize(this Image image, int width, int height, ScalingOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

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

            return image;
        }

        /// <summary>
        /// Changes the size of this <see cref="Image"/> to the specified dimensions without changing its scale.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to resize.</param>
        /// <param name="width">The desired width of the image, in pixels.</param>
        /// <param name="height">The desired height of the image, in pixels.</param>
        /// <returns>
        /// A new resized <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        public static Image Resize(this Image image, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (width <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidWidth);
            }

            if (height <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHeight);
            }

            return image.Inflate(0, 0, width - image.Width, height - image.Height);
        }

        /// <summary>
        /// Creates and returns an enlarged copy of the specified <see cref="Image"/>.
        /// Positive input parameters mean inflating, negative mean cropping.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to inflate.</param>
        /// <param name="left">The amount by which to expand or shrink the left side of the <see cref="Image"/>.</param>
        /// <param name="top">The amount by which to expand or shrink the top side of the <see cref="Image"/>.</param>
        /// <param name="right">The amount by which to expand or shrink the right side of the <see cref="Image"/>. </param>
        /// <param name="bottom">The amount by which to expand or shrink the bottom side of the <see cref="Image"/>.</param>
        /// <returns>
        /// A new inflated <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>Result width is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para>Result height is less than or equal to zero.</para>
        /// </exception>
        public static Image Inflate(this Image image, int left, int top, int right, int bottom)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // calculate and verify target area in source coordinates
            System.Drawing.Rectangle bounds = System.Drawing.Rectangle.FromLTRB(
                -left,
                -top,
                image.Width + right,
                image.Height + bottom);

            if (bounds.Width <= 0)
            {
                throw new ArgumentException("The new image width is invalid.");
            }

            if (bounds.Height <= 0)
            {
                throw new ArgumentException("The new image height is invalid.");
            }

            Image dst = new Image(
                bounds.Width,
                bounds.Height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution);

            // calculate source area to copy from
            Rectangle area = Rectangle.Intersect(bounds, image.Bounds);

            // calculate destination area to copy to
            int dstx = area.X - bounds.X;
            int dsty = area.Y - bounds.Y;

            CopyCrop.CopyArea(image, area.X, area.Y, area.Width, area.Height, dst, dstx, dsty);

            if (image.BitsPerPixel > 1)
            {
                // set frame to white
                dst.SetWhiteBorderIP(dstx, dsty, area.Width, area.Height);
            }

            return dst;
        }
    }
}
