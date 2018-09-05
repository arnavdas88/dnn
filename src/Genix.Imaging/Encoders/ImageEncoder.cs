// -----------------------------------------------------------------------
// <copyright file="ImageEncoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Encoders
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Encodes a single or a collection of <see cref="Image"/> objects to an image stream. This is an abstract class.
    /// </summary>
    public abstract class ImageEncoder
    {
        /// <summary>
        /// Gets a value indicating whether the encoder supports multiple frames.
        /// </summary>
        /// <value>
        /// <b>true</b> if the encoder supports multiple frames; otherwise, <b>false</b>.
        /// </value>
        public abstract bool SupportsMultipleFrames { get; }

        /// <summary>
        /// Gets a value that identifies the file extensions associated with the encoder.
        /// </summary>
        /// <value>
        /// The file extensions associated with the encoder.
        /// </value>
        public abstract string FileExtensions { get; }

        /// <summary>
        /// Encodes a single <see cref="Image"/> to a specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream that this <see cref="Image"/> is encoded to.</param>
        /// <param name="image">The <see cref="Image"/> to encode.</param>
        /// <param name="metadata">The meta data to encode associated with the <paramref name="image"/>.</param>
        public abstract void Save(Stream stream, Image image, ImageMetadata metadata);

        /// <summary>
        /// Encodes a collection of <see cref="Image"/> objects to a specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream that this <see cref="Image"/> is encoded to.</param>
        /// <param name="images">The <see cref="Image"/> objects to encode and associated with them meta data.</param>
        public abstract void Save(Stream stream, IEnumerable<(Image image, ImageMetadata metadata)> images);
    }
}
