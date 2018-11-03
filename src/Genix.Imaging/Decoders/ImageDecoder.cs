// -----------------------------------------------------------------------
// <copyright file="ImageDecoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Decoders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides base functionality for all derived image decoder objects. This is an abstract class.
    /// </summary>
    public abstract class ImageDecoder
    {
        private static readonly List<ImageDecoder> Decoders = new List<ImageDecoder>()
        {
            { new TiffDecoder() },
            { new WinDecoder() },
        };

        /// <summary>
        /// Gets the image formats the decoder supports.
        /// </summary>
        /// <value>
        /// The <see cref="ImageFormat"/> enumeration value.
        /// </value>
        public abstract ImageFormat SupportedFormats { get; }

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
        /// <para>The <see cref="ImageDecoder"/> class supports the following file types:</para>
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
            Stream stream = null;
            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                ImageDecoder decoder = ImageDecoder.DecoderFromStream(stream);
                var result = decoder.Decode(fileName, stream, true, startingFrame, frameCount);
                stream = null;  // The decoder will dispose

                return result;
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    throw new FileLoadException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_CannotLoadImage, fileName),
                        e);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified count bytes from the byte array with index as the starting point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Image"/> from.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the <see cref="Image"/>.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="startingFrame">A zero-based index of the frame to start loading.</param>
        /// <param name="frameCount">The number of frames to load.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startingFrame"/> is negative.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromMemory(byte[] buffer, int index, int count, int startingFrame, int frameCount)
        {
            Stream stream = null;
            try
            {
                stream = new MemoryStream(buffer, index, count, false);

                ImageDecoder decoder = ImageDecoder.DecoderFromStream(stream);
                var result = decoder.Decode(null, stream, true, startingFrame, frameCount);
                stream = null;  // The decoder will dispose

                return result;
            }
            finally
            {
                stream?.Dispose();
            }
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
            ImageDecoder decoder = ImageDecoder.DecoderFromStream(stream);
            return decoder.Decode(null, stream, false, startingFrame, frameCount);
        }

        /// <summary>
        /// Decodes the <see cref="Image"/> from the data stream.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file. Can be <b>null</b> if <paramref name="stream"/> is not a file stream.</param>
        /// <param name="stream">The <see cref="Stream"/> to decode.</param>
        /// <param name="ownStream"><b>true</b> if decoder should dispose the <paramref name="stream"/>.</param>
        /// <param name="startingFrame">A zero-based index of the frame to start loading.</param>
        /// <param name="frameCount">The number of frames to load.</param>
        /// <returns>
        /// The sequence of tuples, one for each loaded frame.
        /// Each tuple contains frame <see cref="Image"/>, a zero-based index of the frame, and the frame meta data.
        /// </returns>
        public abstract IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> Decode(string fileName, Stream stream, bool ownStream, int startingFrame, int frameCount);

        /// <summary>
        /// Creates an <see cref="ImageDecoder"/> that can decode the specified stream.
        /// </summary>
        /// <returns>
        /// The <see cref="ImageDecoder"/> object this method creates.
        /// </returns>
        private static ImageDecoder DecoderFromStream(Stream stream)
        {
            ImageFormat format = ImageFormat.None;

            if (stream.CanSeek)
            {
                byte[] buffer = new byte[4];
                int count = stream.Read(buffer, 0, 4);
                stream.Seek(-count, SeekOrigin.Current);

                if (count == 4)
                {
                    if ((buffer[0] == 0x49 && buffer[1] == 0x49 && buffer[2] == 0x2A && buffer[3] == 0x00) ||
                        (buffer[0] == 0x4D && buffer[1] == 0x4D && buffer[2] == 0x00 && buffer[3] == 0x2A))
                    {
                        format = ImageFormat.Tiff;
                    }
                }
            }

            return ImageDecoder.Decoders.FirstOrDefault(x => (x.SupportedFormats & format) != ImageFormat.None) ??
                   ImageDecoder.Decoders.Last();
        }
    }
}
