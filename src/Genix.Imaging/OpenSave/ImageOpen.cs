// -----------------------------------------------------------------------
// <copyright file="ImageOpen.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Windows.Media.Imaging;

    /// <content>
    /// Provides file opening for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Creates an <see cref="Image"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <remarks>
        /// <para>The <see cref="Image"/> class supports the following file types:</para>
        /// <list type="bullet">
        /// <item><description>TIFF</description></item>
        /// <item><description>BMP</description></item>
        /// <item><description>JPEG</description></item>
        /// <item><description>PNG</description></item>
        /// <item><description>GIF</description></item>
        /// <item><description>PDF</description></item>
        /// </list>
        /// <list type="table">
        /// <listheader>
        /// <item><description>Note</description></item>
        /// </listheader>
        /// <item>
        /// <description>
        /// <para>The <see cref="Image"/> class converts PDF documents to 32 bits per pixel images and sets their resolution to 300 dpi.</para>
        /// <para>Note that you run out of memory if you try to open PDF document that has a large number of pages.</para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><c>fileName</c> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><c>fileName</c> is an empty string (""), contains only white space, or contains one or more invalid characters.</para>
        /// <para>-or-</para>
        /// <para><c>fileName</c> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</para>
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// <para>The file specified by <c>fileName</c> does not exist.</para>
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
        /// <para><c>fileName</c> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</para>
        /// <para>-or-</para>
        /// <para>The file does not have a valid image format.</para>
        /// <para>-or-</para>
        /// <para>The file has zero frames.</para>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromFile(string fileName)
        {
            return new LoadedImages(fileName);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Image"/> from.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>buffer</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromMemory(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return new LoadedImages(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified count bytes from the byte array with index as the starting point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Image"/> from.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the <see cref="Image"/>.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>buffer</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromMemory(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return new LoadedImages(buffer, index, count);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the data for this <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>stream</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new LoadedImages(stream);
        }

        internal static Image OnLoaded(Image image, ImageMetadata metadata, IList<System.Drawing.Color> palette)
        {
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
                invert |= ApplyPhotometricInterpretation(false);
            }

            if (invert)
            {
                image.NOTIP();
            }

            return image;

            // internal methods
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
                                image = image.RotateFlip(RotateFlip.Rotate180FlipY);
                                break;
                            case TIFFOrientation.BottomRight:
                                image = image.RotateFlip(RotateFlip.Rotate180FlipNone);
                                break;
                            case TIFFOrientation.BottomLeft:
                                image = image.RotateFlip(RotateFlip.RotateNoneFlipX);
                                break;
                            case TIFFOrientation.LeftTop:
                                image = image.RotateFlip(RotateFlip.Rotate270FlipY);
                                break;
                            case TIFFOrientation.RightTop:
                                image = image.RotateFlip(RotateFlip.Rotate90FlipNone);
                                break;
                            case TIFFOrientation.RightBottom:
                                image = image.RotateFlip(RotateFlip.Rotate90FlipY);
                                break;
                            case TIFFOrientation.LeftBottom:
                                image = image.RotateFlip(RotateFlip.Rotate270FlipNone);
                                break;

                            case TIFFOrientation.TopLeft:
                            default:
                                break;
                        }

                        metadata.RemovePropertyItem((int)TIFFField.Orientation);
                    }
                }
            }

            bool ApplyPhotometricInterpretation(bool replaceYCbCr)
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
                        else if (photometric == TIFFPhotometricInterpretation.YCbCr && replaceYCbCr)
                        {
                            metadata.SetPropertyItem((int)TIFFField.PhotometricInterpretation, TIFFPhotometricInterpretation.RGB);
                        }
                    }
                }

                return false;
            }
        }

#if false
        [HandleProcessCorruptedStateExceptions]
        private static IEnumerable<(Image, ImageMetadata)> Load(Stream stream)
        {
            long streamPosition = stream.Position;

            ////try
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                foreach (BitmapFrame bitmapFrame in decoder.Frames)
                {
                    (Image image, ImageMetadata metadata) = bitmapFrame.FromBitmapFrame();
                    yield return (image, metadata);
                }
            }
            /*catch (Exception e)
            {
                bool libTiffSucceeded = false;

                if (e is FileFormatException ||
                    e is NotSupportedException ||
                    e is OverflowException ||
                    e is ArgumentException)
                {
                    // try LibTiff
                    if (stream.CanSeek)
                    {
                        stream.Seek(streamPosition, SeekOrigin.Begin);
                        ////Image.LoadFromTiffStream(stream, libtiffframes);
                        libTiffSucceeded = true;
                        ////this.frames.AddRange(libtiffframes);
                    }
                }

                // throw original exception
                if (!libTiffSucceeded)
                {
                    throw;
                }
            }*/
        }
#endif
        private class LoadedImages : IEnumerable<(Image, int?, ImageMetadata)>
        {
            private readonly string fileName;

            private readonly byte[] buffer;
            private readonly int index;
            private readonly int count;

            private readonly Stream stream;

            public LoadedImages(string fileName)
            {
                this.fileName = fileName;
            }

            public LoadedImages(byte[] buffer, int index, int count)
            {
                this.buffer = buffer;
                this.index = index;
                this.count = count;
            }

            public LoadedImages(Stream stream)
            {
                this.stream = stream;
            }

            IEnumerator<(Image, int?, ImageMetadata)> IEnumerable<(Image, int?, ImageMetadata)>.GetEnumerator()
            {
                if (!string.IsNullOrEmpty(this.fileName))
                {
                    return new Enumerator(this.fileName);
                }
                else if (this.buffer != null)
                {
                    return new Enumerator(this.buffer, this.index, this.count);
                }
                else
                {
                    return new Enumerator(this.stream);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this.stream);
            }

            private class Enumerator : IEnumerator<(Image, int?, ImageMetadata)>, IEnumerator
            {
                private readonly bool ownStream;
                private readonly Stream stream;
                private readonly long streamPosition;
                private readonly BitmapDecoder decoder;

                // Enumerators are positioned before the first element until the first MoveNext() call.
                private int position = -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Enumerator(string fileName)
                {
                    try
                    {
                        this.ownStream = true;
                        this.stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        this.streamPosition = this.stream.Position;
                        this.decoder = Enumerator.CreateDecoder(this.stream);
                    }
                    catch (Exception e)
                    {
                        this.stream?.Dispose();

                        throw new FileLoadException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_CannotLoadImage, fileName),
                            e);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Enumerator(byte[] buffer, int index, int count)
                {
                    try
                    {
                        this.ownStream = true;
                        this.stream = new MemoryStream(buffer, index, count, false);
                        this.streamPosition = this.stream.Position;
                        this.decoder = Enumerator.CreateDecoder(this.stream);
                    }
                    catch
                    {
                        this.stream?.Dispose();
                        throw;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Enumerator(Stream stream)
                {
                    this.stream = stream;
                    this.streamPosition = this.stream.Position;
                    this.decoder = Enumerator.CreateDecoder(this.stream);
                }

                public (Image, int?, ImageMetadata) Current
                {
                    get
                    {
                        BitmapFrame bitmapFrame = this.decoder.Frames[this.position];
                        (Image image, ImageMetadata metadata) = bitmapFrame.FromBitmapFrame();
                        return (image, this.decoder.Frames.Count == 1 ? null : (int?)this.position, metadata);
                    }
                }

                object IEnumerator.Current => this.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool IEnumerator.MoveNext()
                {
                    this.position++;
                    return this.position < this.decoder.Frames.Count;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void IEnumerator.Reset()
                {
                    if (this.position != -1)
                    {
                        if (!this.stream.CanSeek)
                        {
                            throw new InvalidOperationException();
                        }

                        this.position = -1;
                        this.stream.Position = this.streamPosition;
                    }
                }

                [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "stream", Justification = "Dispose only if we own the object.")]
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose()
                {
                    if (this.ownStream)
                    {
                        this.stream?.Dispose();
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static BitmapDecoder CreateDecoder(Stream stream)
                {
                    return BitmapDecoder.Create(
                        stream,
                        BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.DelayCreation,
                        BitmapCacheOption.None);
                }
            }
        }
    }
}
