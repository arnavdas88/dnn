// -----------------------------------------------------------------------
// <copyright file="ImageSave.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;

    /// <content>
    /// Provides file saving for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        private static readonly Dictionary<string, ImageFormat> MapFormats = new Dictionary<string, ImageFormat>()
        {
            { ".TIF", ImageFormat.Tiff },
            { ".TIFF", ImageFormat.Tiff },
            { ".JPG", ImageFormat.Jpeg },
            { ".JPEG", ImageFormat.Jpeg },
            { ".JPE", ImageFormat.Jpeg },
            { ".JFIF", ImageFormat.Jpeg },
            { ".GIF", ImageFormat.Gif },
            { ".PNG", ImageFormat.Png },
            { ".BMP", ImageFormat.Bmp },
            { ".DIB", ImageFormat.Bmp },
        };

        /// <summary>
        /// Gets an collection of supported image file extensions.
        /// </summary>
        /// <value>
        /// A collection of <see cref="string"/> objects that represents the image file extensions.
        /// </value>
        public static IReadOnlyCollection<string> SupportedFileExtensions => Image.MapFormats.Keys;

        /// <summary>
        /// Saves a range of <see cref="Image"/> objects to the specified file.
        /// </summary>
        /// <param name="images">The <see cref="Image"/> objects to save.</param>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Image"/> objects.</param>
        public static void Save(IEnumerable<Image> images, string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Image.Save(images, stream, Image.GetFormat(fileName));
            }
        }

        /// <summary>
        /// Saves a range of <see cref="Image"/> objects to the specified data stream in the specified format.
        /// </summary>
        /// <param name="images">The <see cref="Image"/> objects to save.</param>
        /// <param name="stream">The <see cref="Stream"/> where the images will be saved.</param>
        /// <param name="format">An <see cref="ImageFormat"/> that specifies the format of the saved image.</param>
        public static void Save(IEnumerable<Image> images, Stream stream, ImageFormat format)
        {
            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (format == ImageFormat.Tiff)
            {
                images.SaveToTiff(stream);
            }
            else
            {
                BitmapEncoder encoder = Image.CreateEncoder(format);
                if (!encoder.CodecInfo.SupportsMultipleFrames)
                {
                    throw new InvalidOperationException(Properties.Resources.E_UnsupportedMultipleFrames);
                }

                foreach (Image image in images)
                {
                    if (encoder is TiffBitmapEncoder tiffBitmapEncoder)
                    {
                        tiffBitmapEncoder.Compression = image.BitsPerPixel == 1 ? TiffCompressOption.Ccitt4 : TiffCompressOption.Default;
                    }

                    encoder.Frames.Add(image.ToBitmapFrame());
                }

                encoder.Save(stream);
            }
        }

        /// <summary>
        /// Saves a range of <see cref="Image"/> objects to the memory buffer in the specified format.
        /// </summary>
        /// <param name="images">The <see cref="Image"/> objects to save.</param>
        /// <param name="format">An <see cref="ImageFormat"/> that specifies the format of the saved images.</param>
        /// <returns>
        /// The buffer containing saved <see cref="Image"/> objects.
        /// </returns>
        public static byte[] Save(IEnumerable<Image> images, ImageFormat format)
        {
            using (MemoryStream stream = new MemoryStream(1000000))
            {
                Image.Save(images, stream, format);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="Image"/> can be saved in the specified format.
        /// </summary>
        /// <param name="format">The image format to check.</param>
        /// <returns>
        /// <b>true</b> if the current <see cref="Image"/> can be saved in the specified format; otherwise, <b>false</b>.
        /// </returns>
        public bool CanSave(ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.Tiff:
                    return true;

                case ImageFormat.Bmp:
                    return true;

                case ImageFormat.Jpeg:
                    // Jpeg supports gray-scale and color images only
                    return this.BitsPerPixel >= 8;

                case ImageFormat.Png:
                    return true;

                case ImageFormat.Gif:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Saves this <see cref="Image"/> to the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Image"/>.</param>
        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                this.Save(stream, Image.GetFormat(fileName));
            }
        }

        /// <summary>
        /// Saves this <see cref="Image"/> to the specified data stream in the specified format.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> where the image will be saved.</param>
        /// <param name="format">An <see cref="ImageFormat"/> that specifies the format of the saved image.</param>
        public void Save(Stream stream, ImageFormat format)
        {
            if (format == ImageFormat.Tiff)
            {
                this.SaveToTiff(stream);
            }
            else
            {
                BitmapEncoder encoder = Image.CreateEncoder(format);

                if (encoder is TiffBitmapEncoder tiffBitmapEncoder)
                {
                    tiffBitmapEncoder.Compression = this.BitsPerPixel == 1 ? TiffCompressOption.Ccitt4 : TiffCompressOption.Default;
                }

                encoder.Frames.Add(this.ToBitmapFrame());

                encoder.Save(stream);
            }
        }

        /// <summary>
        /// Saves this <see cref="Image"/> to the memory buffer in the specified format.
        /// </summary>
        /// <param name="format">An <see cref="ImageFormat"/> that specifies the format of the saved image.</param>
        /// <returns>
        /// The buffer containing saved <see cref="Image"/>.
        /// </returns>
        public byte[] Save(ImageFormat format)
        {
            using (MemoryStream stream = new MemoryStream(1000000))
            {
                this.Save(stream, format);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Creates an encoder for the specified format.
        /// </summary>
        /// <param name="format">An <see cref="ImageFormat"/> that specifies the format of the image.</param>
        /// <returns>The <see cref="BitmapEncoder"/> this method creates.</returns>
        private static BitmapEncoder CreateEncoder(ImageFormat format)
        {
            switch (format)
            {
                ////case ImageFormat.Unknown:

                case ImageFormat.Bmp:
                    return new BmpBitmapEncoder();

                case ImageFormat.Gif:
                    return new GifBitmapEncoder();

                case ImageFormat.Jpeg:
                    return new JpegBitmapEncoder()
                    {
                        QualityLevel = 100,
                    };

                case ImageFormat.Png:
                    return new PngBitmapEncoder();

                case ImageFormat.Tiff:
                    return new TiffBitmapEncoder();

                default:
                    throw new NotSupportedException(Properties.Resources.E_UnsupportedImageFormat);
            }
        }

        /// <summary>
        /// Determines the image file format using the image file name.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file.</param>
        /// <returns>
        /// The <see cref="ImageFormat"/> enumeration value.
        /// </returns>
        private static ImageFormat GetFormat(string fileName)
        {
            if (!Image.MapFormats.TryGetValue(Path.GetExtension(fileName).ToUpperInvariant(), out ImageFormat format))
            {
                format = ImageFormat.Tiff;
            }

            return format;
        }
    }
}
