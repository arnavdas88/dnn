// -----------------------------------------------------------------------
// <copyright file="ImageOpen.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Imaging.Decoders;

    /// <content>
    /// Provides file opening for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Creates an <see cref="Image"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Image"/>.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        /// <remarks>
        /// <para>The <see cref="Image"/> class supports the following file types:</para>
        /// <list type="bullet">
        /// <item><description>TIFF</description></item>
        /// <item><description>BMP</description></item>
        /// <item><description>JPEG</description></item>
        /// <item><description>PNG</description></item>
        /// <item><description>GIF</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="fileName"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="fileName"/> is an empty string (""), contains only white space, or contains one or more invalid characters.</para>
        /// <para>-or-</para>
        /// <para><paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</para>
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// <para>The file specified by <paramref name="fileName"/> does not exist.</para>
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <para>The specified path is invalid, such as being on an unmapped drive.</para>
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// <para>The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</para>
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// <para>The caller does not have the required permission.</para>
        /// </exception>
        /// <exception cref="FileLoadException">
        /// <para><paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</para>
        /// <para>-or-</para>
        /// <para>The file does not have a valid image format.</para>
        /// <para>-or-</para>
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromFile(string fileName)
        {
            return Image.FromFile(fileName, 0, -1);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Image"/>.</param>
        /// <param name="startingFrame">A zero-based index of the frame to start loading.</param>
        /// <param name="frameCount">The number of frames to load.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        /// <remarks>
        /// <para>The <see cref="Image"/> class supports the following file types:</para>
        /// <list type="bullet">
        /// <item><description>TIFF</description></item>
        /// <item><description>BMP</description></item>
        /// <item><description>JPEG</description></item>
        /// <item><description>PNG</description></item>
        /// <item><description>GIF</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="fileName"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="fileName"/> is an empty string (""), contains only white space, or contains one or more invalid characters.</para>
        /// <para>-or-</para>
        /// <para><paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startingFrame"/> is negative.</para>
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// <para>The file specified by <paramref name="fileName"/> does not exist.</para>
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <para>The specified path is invalid, such as being on an unmapped drive.</para>
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// <para>The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</para>
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// <para>The caller does not have the required permission.</para>
        /// </exception>
        /// <exception cref="FileLoadException">
        /// <para><paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</para>
        /// <para>-or-</para>
        /// <para>The file does not have a valid image format.</para>
        /// <para>-or-</para>
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromFile(string fileName, int startingFrame, int frameCount)
        {
            return ImageDecoder.FromFile(fileName, startingFrame, frameCount);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Image"/> from.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromMemory(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return Image.FromMemory(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified count bytes from the byte array with index as the starting point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Image"/> from.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the <see cref="Image"/>.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromMemory(byte[] buffer, int index, int count)
        {
            return ImageDecoder.FromMemory(buffer, index, count, 0, -1);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the data for this <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The stream does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The stream has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromStream(Stream stream)
        {
            return Image.FromStream(stream, 0, -1);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the data for this <see cref="Image"/>.</param>
        /// <param name="startingFrame">A zero-based index of the frame to start loading.</param>
        /// <param name="frameCount">The number of frames to load.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startingFrame"/> is negative.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The stream does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The stream has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromStream(Stream stream, int startingFrame, int frameCount)
        {
            return ImageDecoder.FromStream(stream, startingFrame, frameCount);
        }

        internal static Image OnLoaded(Image image, ImageMetadata metadata, IList<Color> palette)
        {
            // try to fix invalid image resolution
            FixResolution();

            // apply orientation
            ApplyOrientation();

            // determine whether bitmap should be inverted
            bool invert = false;

            if (image.BitsPerPixel <= 8 && palette != null)
            {
                // convert color palette
                if (!BitmapExtensions.IsGrayScalePalette(image.BitsPerPixel, palette, out bool invertedPalette))
                {
                    return image.ApplyPalette(palette);
                }

                invert = invertedPalette;

                // remove palette
                metadata?.RemovePropertyItems(x => x.Id == (int)TIFFField.PhotometricInterpretation ||
                                                   x.Id == (int)TIFFField.ColorMap);
            }
            else
            {
                invert |= ApplyPhotometricInterpretation();
            }

            if (invert)
            {
                image.Not(image);
            }

            return image;

            // private methods
            void FixResolution()
            {
                (float w, float h)[] standardResolutions = new (float, float)[]
                {
                    (8.5f, 11.0f),  // letter
                    (8.5f, 14.0f),  // legal
                    (7.75f, 10.5f),  // W-2
                };

                // some scanners do not set correct resolution tags and leave them to 72 dpi
                if (image.HorizontalResolution == 72 && image.VerticalResolution == 72)
                {
                    // calculate width to height ratio
                    float[] ratios = new float[standardResolutions.Length];
                    for (int i = 0, ii = standardResolutions.Length; i < ii; i++)
                    {
                        ratios[i] = Math.Abs(1.0f - (((float)image.Width / standardResolutions[i].w) / ((float)image.Height / standardResolutions[i].h)));
                    }

                    int argmin = Vectors.ArgMin(ratios.Length, ratios, 0);
                    if (ratios[argmin] < 0.1f)
                    {
                        int res = (int)((((float)image.Width / standardResolutions[argmin].w) + ((float)image.Height / standardResolutions[argmin].h)) / 2);
                        image.SetResolution(res, res);
                    }
                }
            }

            void ApplyOrientation()
            {
                // change image orientation to top-left
                object itemOrientation = metadata?.GetPropertyItem((int)TIFFField.Orientation);
                if (itemOrientation != null)
                {
                    if (Enum.TryParse<TIFFOrientation>(itemOrientation.ToString(), out TIFFOrientation orientation))
                    {
                        switch (orientation)
                        {
                            case TIFFOrientation.TopRight:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate180FlipY);
                                break;
                            case TIFFOrientation.BottomRight:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate180FlipNone);
                                break;
                            case TIFFOrientation.BottomLeft:
                                image.RotateFlip(image, Imaging.RotateFlip.RotateNoneFlipX);
                                break;
                            case TIFFOrientation.LeftTop:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate270FlipY);
                                break;
                            case TIFFOrientation.RightTop:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate90FlipNone);
                                break;
                            case TIFFOrientation.RightBottom:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate90FlipY);
                                break;
                            case TIFFOrientation.LeftBottom:
                                image.RotateFlip(image, Imaging.RotateFlip.Rotate270FlipNone);
                                break;

                            case TIFFOrientation.TopLeft:
                            default:
                                break;
                        }

                        metadata.RemovePropertyItem((int)TIFFField.Orientation);
                    }
                }
            }

            bool ApplyPhotometricInterpretation()
            {
                // determine whether the image must be inverted using photometric interpretation
                object itemPhotometric = metadata?.GetPropertyItem((int)TIFFField.PhotometricInterpretation);
                if (itemPhotometric != null)
                {
                    if (Enum.TryParse<TIFFPhotometricInterpretation>(itemPhotometric.ToString(), out TIFFPhotometricInterpretation photometric))
                    {
                        if (photometric == TIFFPhotometricInterpretation.BlackIsZero)
                        {
                            metadata.RemovePropertyItem((int)TIFFField.PhotometricInterpretation);

                            // we keep 1bpp images in WhiteIsZero format
                            return image.BitsPerPixel == 1;
                        }
                        else if (photometric == TIFFPhotometricInterpretation.YCbCr)
                        {
                            metadata.SetPropertyItem((int)TIFFField.PhotometricInterpretation, TIFFPhotometricInterpretation.RGB);
                        }
                    }
                }

                return false;
            }
        }
    }
}
