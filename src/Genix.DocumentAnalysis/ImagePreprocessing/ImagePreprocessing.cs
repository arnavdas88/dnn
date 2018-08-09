// -----------------------------------------------------------------------
// <copyright file="ImagePreprocessing.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Globalization;
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
                image = ConvertTo1bpp().Despeckle();
            }

            // removes border noise
            if (options.HasFlag(ImagePreprocessingOptions.CleanOverscan))
            {
                // clean 1/2 inches on all sides
                image = ConvertTo1bpp().CleanOverscan(0.5f, 0.5f);
            }

            // align
            if (options.HasFlag(ImagePreprocessingOptions.Despeckle))
            {
                image = ConvertTo1bpp().Deskew();
            }

            // convert to requested depth
            switch (bitsPerPixel)
            {
                case 1: return ConvertTo1bpp();
                case 8: return ConvertTo8bpp();
                case 24: return ConvertTo24bpp();
                case 32: return ConvertTo32bpp();

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, bitsPerPixel));
            }

            Image ConvertTo1bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return image;
                    case 8: return image.Binarize();
                    case 24: return image.Convert24To8().Binarize();
                    case 32: return image.Convert32To8().Binarize();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo8bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return image.Convert1To8();
                    case 8: return image;
                    case 24: return image.Convert24To8();
                    case 32: return image.Convert32To8();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo24bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return image.Convert1To8().Convert8To24();
                    case 8: return image.Convert8To24();
                    case 24: return image;
                    case 32: return image.Convert32To24();

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }

            Image ConvertTo32bpp()
            {
                switch (image.BitsPerPixel)
                {
                    case 1: return image.Convert1To8().Convert8To32(255);
                    case 8: return image.Convert8To32(255);
                    case 24: return image.Convert24To32();
                    case 32: return image;

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
                }
            }
        }
    }
}