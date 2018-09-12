// -----------------------------------------------------------------------
// <copyright file="ImagePreprocessing.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using Genix.Imaging;

    /// <summary>
    /// Prepares image for classification, recognition, and other forms of analysis.
    /// </summary>
    public static class ImagePreprocessing
    {
        /// <summary>
        /// Prepares image for classification, recognition, and other forms of analysis.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="options">The image preprocessing options.</param>
        /// <param name="bitsPerPixel">The depth of resulting image.</param>
        /// <returns>The processed <see cref="Image"/>.</returns>
        /// <remarks>
        /// The resulting <see cref="Image"/> will have depth specified by <paramref name="bitsPerPixel"/> parameter.
        /// </remarks>
        public static Image Process(Image image, ImagePreprocessingOptions options, int bitsPerPixel)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // remove noise
            if (options.HasFlag(ImagePreprocessingOptions.Despeckle))
            {
                image = Image.ConvertTo(image, 1);
                image = Image.Despeckle(image);
            }

            // removes border noise
            if (options.HasFlag(ImagePreprocessingOptions.CleanOverscan))
            {
                // clean 1/2 inches on all sides
                image = Image.ConvertTo(image, 1);
                image = Image.CleanOverscan(image, 0.5f, 0.5f);
            }

            // align
            if (options.HasFlag(ImagePreprocessingOptions.Despeckle))
            {
                image = Image.ConvertTo(image, 1);
                image = Image.Deskew(image);
            }

            return Image.ConvertTo(image, bitsPerPixel);
        }
    }
}