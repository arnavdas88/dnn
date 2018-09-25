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
    using System.Windows.Threading;
    using Genix.Core;

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
            return new LoadedImages(fileName, 0, -1);
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
            return new LoadedImages(fileName, startingFrame, frameCount);
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

            return new LoadedImages(buffer, 0, buffer.Length, 0, -1);
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
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return new LoadedImages(buffer, index, count, 0, -1);
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
        /// <para>The file does not have a valid image format.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>The file has zero frames.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new LoadedImages(stream, 0, -1);
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
                invert |= ApplyPhotometricInterpretation(false);
            }

            if (invert)
            {
                image.NotIP();
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
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate180FlipY);
                                break;
                            case TIFFOrientation.BottomRight:
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate180FlipNone);
                                break;
                            case TIFFOrientation.BottomLeft:
                                image = image.RotateFlip(Imaging.RotateFlip.RotateNoneFlipX);
                                break;
                            case TIFFOrientation.LeftTop:
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate270FlipY);
                                break;
                            case TIFFOrientation.RightTop:
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate90FlipNone);
                                break;
                            case TIFFOrientation.RightBottom:
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate90FlipY);
                                break;
                            case TIFFOrientation.LeftBottom:
                                image = image.RotateFlip(Imaging.RotateFlip.Rotate270FlipNone);
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

            private readonly int startingFrame;
            private readonly int frameCount;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LoadedImages(string fileName, int startingFrame, int frameCount)
            {
                this.fileName = fileName;

                this.startingFrame = startingFrame;
                this.frameCount = frameCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LoadedImages(byte[] buffer, int index, int count, int startingFrame, int frameCount)
            {
                this.buffer = buffer;
                this.index = index;
                this.count = count;

                this.startingFrame = startingFrame;
                this.frameCount = frameCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LoadedImages(Stream stream, int startingFrame, int frameCount)
            {
                this.stream = stream;

                this.startingFrame = startingFrame;
                this.frameCount = frameCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<(Image, int?, ImageMetadata)> IEnumerable<(Image, int?, ImageMetadata)>.GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this);
            }

            private class Enumerator : IEnumerator<(Image, int?, ImageMetadata)>, IEnumerator
            {
                private readonly object sync = new object();

                private readonly bool ownStream;
                private readonly Stream stream;
                private readonly long streamPosition;
                private readonly BitmapDecoder decoder;

                private readonly int frameCount;
                private readonly int firstFrame;    // The index of first frame to load
                private readonly int lastFrame;     // The index of last frame to load

                // Enumerators are positioned before the first element until the first MoveNext() call.
                private int currentFrame = -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Enumerator(LoadedImages parent)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(parent.fileName))
                        {
                            this.ownStream = true;
                            this.stream = new FileStream(parent.fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        else if (parent.buffer != null)
                        {
                            this.ownStream = true;
                            this.stream = new MemoryStream(parent.buffer, parent.index, parent.count, false);
                        }
                        else
                        {
                            this.stream = parent.stream;
                        }

                        this.streamPosition = this.stream.Position;
                        this.decoder = BitmapDecoder.Create(
                            this.stream,
                            BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.DelayCreation,
                            BitmapCacheOption.None);

                        this.frameCount = this.decoder.Frames.Count;
                        this.firstFrame = parent.startingFrame;
                        this.lastFrame = 0; //// parent.frameCount > 0 ?
                                            ////MinMax.Min(this.frameCount, parent.startingFrame + parent.frameCount) - 1 :
                                            ////this.frameCount - 1;

                        this.currentFrame = this.firstFrame - 1;
                    }
                    catch (Exception e)
                    {
                        this.Dispose();

                        if (!string.IsNullOrEmpty(parent.fileName))
                        {
                            throw new FileLoadException(
                                string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_CannotLoadImage, parent.fileName),
                                e);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                public (Image, int?, ImageMetadata) Current
                {
                    get
                    {
                        lock (this.sync)
                        {
                            BitmapFrame bitmapFrame = this.decoder.Frames[this.currentFrame];
                            (Image image, ImageMetadata metadata) = bitmapFrame.FromBitmapFrame();
                            return (image, this.frameCount == 1 ? null : (int?)this.currentFrame, metadata);
                        }
                    }
                }

                object IEnumerator.Current => this.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool IEnumerator.MoveNext()
                {
                    lock (this.sync)
                    {
                        return ++this.currentFrame <= this.lastFrame;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void IEnumerator.Reset()
                {
                    lock (this.sync)
                    {
                        if (this.currentFrame != this.firstFrame - 1)
                        {
                            if (!this.stream.CanSeek)
                            {
                                throw new InvalidOperationException();
                            }

                            this.currentFrame = this.firstFrame - 1;
                            this.stream.Position = this.streamPosition;
                        }
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
            }
        }
    }
}
