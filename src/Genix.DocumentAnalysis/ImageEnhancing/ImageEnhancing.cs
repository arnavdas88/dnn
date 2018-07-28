// -----------------------------------------------------------------------
// <copyright file="ImageEnhancing.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using Genix.Imaging;

    /// <summary>
    /// Improves image quality.
    /// </summary>
    public static class ImageEnhancing
    {
        /// <summary>
        /// Improves image quality.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to enhance.</param>
        /// <param name="options">The image enhancing options.</param>
        /// <returns>The enhanced <see cref="Image"/>.</returns>
        public static Image Enhance(Image image, ImageEnhancingOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // binarize gray scale image
            if (image.BitsPerPixel == 8)
            {
                image = image.Binarize();
            }

            // remove noise
            if (options.HasFlag(ImageEnhancingOptions.Despeckle))
            {
                image = image.Despeckle();
            }

            // removes border noise
            if (options.HasFlag(ImageEnhancingOptions.CleanBorderNoise))
            {
                // clean 1/2 inches on all sides
                image = image.CleanBorderNoise(0.5f, 0.5f);
            }

            // align
            if (options.HasFlag(ImageEnhancingOptions.Despeckle))
            {
                image = image.Deskew();
            }

            return image;
        }
    }
}