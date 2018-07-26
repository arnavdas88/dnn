﻿// -----------------------------------------------------------------------
// <copyright file="ImageDistortion.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Genix.Core;
    using Genix.Imaging;

    /// <summary>
    /// Randomly distorts images.
    /// </summary>
    public class ImageDistortion
    {
        /// <summary>
        /// The random numbers generator.
        /// </summary>
        private readonly GaussianGenerator normalDistribution = new GaussianGenerator(0.0, 0.33);

        /// <summary>
        /// Randomly distorts the specified image that contains a single character.
        /// </summary>
        /// <param name="image">The image to distort.</param>
        /// <param name="width">The sample width, in pixels.</param>
        /// <param name="height">The sample height, in pixels.</param>
        /// <param name="shift">The value that indicates whether the bitmap should be shifted randomly.</param>
        /// <param name="rotate">The value that indicates whether the bitmap should be rotated randomly.</param>
        /// <param name="scale">The value that indicates whether the bitmap should be scale randomly.</param>
        /// <param name="crop">The value that indicates whether the bitmap should be cropped randomly.</param>
        /// <returns>
        /// The sequence of <see cref="Imaging.Image"/> objects that contains the distorted images.
        /// </returns>
        public IEnumerable<Genix.Imaging.Image> Distort(
            Genix.Imaging.Image image,
            int width,
            int height,
            bool shift,
            bool rotate,
            bool scale,
            bool crop)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel == 8)
            {
                image = image.Binarize();
            }

            // rotate image randomly
            if (rotate)
            {
                double angle = (width > 0 ? 18 : 3) * this.normalDistribution.Generate();
                image = image.Rotate(angle);
            }

            // reduce image size to black area
            image = image.CropBlackArea(1, 1);

            // scale down if bigger than needed
            if (width > 0)
            {
                if (image.Width > width || image.Height > height)
                {
                    double horizontalFactor = (double)width / image.Width;
                    double verticalFactor = (double)height / image.Height;
                    double scaleFactor = Math.Min(horizontalFactor, verticalFactor);

                    image = image.Scale(scaleFactor, ScalingOptions.None);
                }
            }
            else
            {
                if (image.Height > height)
                {
                    double scaleFactor = (double)height / image.Height;
                    image = image.Scale(scaleFactor, ScalingOptions.None);
                }
            }

            // scale down image randomly
            if (scale)
            {
                if (width > 0)
                {
                    int newWidth = image.Width - Math.Abs(this.Random(-width / 4, width / 4));
                    int newHeight = image.Height - Math.Abs(this.Random(-height / 4, height / 4));
                    image = image.ScaleToSize(newWidth, newHeight, ScalingOptions.None);
                }
                else
                {
                    int newWidth = image.Width + this.Random(-image.Width / 10, image.Width / 10);
                    int newHeight = image.Height - Math.Abs(this.Random(-height / 4, height / 4));
                    image = image.ScaleToSize(newWidth, newHeight, ScalingOptions.None);

                    double angle = 0.5 * this.normalDistribution.Generate();
                    image = image.Inflate(image.Height, 0, image.Height, 0);
                    image = image.Shear(angle);

                    Rectangle blackArea = image.BlackArea();
                    int left = Math.Max(blackArea.Left - 1, 0);
                    int right = Math.Min(blackArea.Right + 1, image.Width);
                    image = image.Crop(left, 0, right - left, image.Height);
                }
            }

            // increase image size to fit the requested size
            if (width > 0)
            {
                if (image.Width < width || image.Height < height)
                {
                    int dx = width - image.Width;
                    int dy = height - image.Height;

                    int offsetx = dx / 2;
                    int offsety = dy / 2;
                    if (shift)
                    {
                        offsetx = this.Random(0, dx);
                        offsety = this.Random(0, dy);
                    }

                    image = image.Inflate(offsetx, offsety, dx - offsetx, dy - offsety);
                }
            }
            else
            {
                // pad with empty space on the right to reduce width granularity
                int newWidth = ((image.Width + 15) / 16) * 16;

                // increase image size to fit the requested size
                if (image.Width < newWidth || image.Height < height)
                {
                    int dx = newWidth - image.Width;
                    int dy = height - image.Height;

                    int offsetx = dx / 2;
                    int offsety = dy / 2;
                    if (shift)
                    {
                        offsetx = this.Random(0, dx);
                        offsety = this.Random(0, dy);
                    }

                    image = image.Inflate(offsetx, offsety, dx - offsetx, dy - offsety);
                }
            }

            yield return image;

            if (crop)
            {
                int oldWidth = image.Width;
                int oldHeight = image.Height;
                int inflatex = image.Width / 8;
                int inflatey = image.Height / 8;
                image = image.Inflate(inflatex, inflatey, inflatex, inflatey);

                yield return image.Crop(0, 0, oldWidth, oldHeight);
                yield return image.Crop(2 * inflatex, 0, oldWidth, oldHeight);
                yield return image.Crop(0, 2 * inflatey, oldWidth, oldHeight);
                yield return image.Crop(2 * inflatex, 2 * inflatey, oldWidth, oldHeight);
            }
        }

        private int Random(int min, int max)
        {
            int result = (int)(((this.normalDistribution.Generate() + 1.0f) / 2.0f * (max - min)) + 0.5f) + min;

            if (result < min)
            {
                return min;
            }

            if (result > max)
            {
                return max;
            }

            return result;
        }
    }
}