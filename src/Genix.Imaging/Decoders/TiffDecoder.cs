// -----------------------------------------------------------------------
// <copyright file="TiffDecoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Decoders
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using BitMiracle.LibTiff.Classic;

    /// <summary>
    /// Represents an decoder used to decode Tagged Image File Format (TIFF) format images.
    /// </summary>
    public class TiffDecoder : ImageDecoder
    {
        /// <inheritdoc />
        public override ImageFormat SupportedFormats => ImageFormat.Tiff;

        /// <inheritdoc />
        public override IEnumerable<(Image image, int? frameIndex, ImageMetadata metadata)> Decode(string fileName, Stream stream, bool ownStream, int startingFrame, int frameCount)
        {
            return new Loader(fileName, stream, ownStream, startingFrame, frameCount);
        }

        private class Loader : IEnumerable<(Image, int?, ImageMetadata)>
        {
            private readonly int startingFrame;
            private readonly int frameCount;

            private readonly string fileName;
            private readonly Stream stream;
            private readonly bool ownStream;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Loader(string fileName, Stream stream, bool ownStream, int startingFrame, int frameCount)
            {
                if (startingFrame < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(startingFrame), "The starting frame must be a non-negative integer.");
                }

                this.startingFrame = startingFrame;
                this.frameCount = frameCount;

                this.fileName = fileName;
                this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
                this.ownStream = ownStream;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<(Image, int?, ImageMetadata)> IEnumerable<(Image, int?, ImageMetadata)>.GetEnumerator()
            {
                return new Enumerator(this.fileName, this.stream, this.ownStream, this.startingFrame, this.frameCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this.fileName, this.stream, this.ownStream, this.startingFrame, this.frameCount);
            }

            private class Enumerator : IEnumerator<(Image, int?, ImageMetadata)>, IEnumerator
            {
                private readonly object sync = new object();

                private readonly string fileName;
                private readonly Stream stream;
                private readonly bool ownStream;

                private readonly Tiff tiff;

                private readonly int frameCount;
                private readonly int firstFrame;    // The index of first frame to load
                private readonly int lastFrame;     // The index of last frame to load

                // Enumerators are positioned before the first element until the first MoveNext() call.
                private int currentFrame = -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Enumerator(string fileName, Stream stream, bool ownStream, int startingFrame, int frameCount)
                {
                    try
                    {
                        this.fileName = fileName;
                        this.stream = stream;
                        this.ownStream = ownStream;

                        this.tiff = Libtiff.ClientOpen(stream);

                        this.frameCount = (int)this.tiff.NumberOfDirectories();
                        this.firstFrame = startingFrame;
                        this.lastFrame = Math.Min(this.frameCount, startingFrame + (frameCount > 0 ? frameCount : this.frameCount)) - 1;

                        this.currentFrame = this.firstFrame - 1;
                    }
                    catch (Exception e)
                    {
                        this.Dispose();

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

                public (Image, int?, ImageMetadata) Current
                {
                    get
                    {
                        lock (this.sync)
                        {
                            (Image image, ImageMetadata metadata) = Libtiff.ExtractCurrentFrame(this.tiff);
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
                        bool isReset = this.currentFrame == this.firstFrame - 1;
                        bool moved = ++this.currentFrame <= this.lastFrame;
                        if (moved)
                        {
                            if (isReset)
                            {
                                moved = this.tiff.SetDirectory((short)this.currentFrame);
                            }
                            else
                            {
                                moved = this.tiff.ReadDirectory();
                            }
                        }

                        return moved;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void IEnumerator.Reset()
                {
                    lock (this.sync)
                    {
                        this.currentFrame = this.firstFrame - 1;
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

                    this.tiff?.Dispose();
                }
            }
        }
    }
}
