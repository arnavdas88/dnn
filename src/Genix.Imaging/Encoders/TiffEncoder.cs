// -----------------------------------------------------------------------
// <copyright file="TiffEncoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Encoders
{
    using System.IO;
    using BitMiracle.LibTiff.Classic;

    /// <summary>
    /// Represents an encoder used to encode Tagged Image File Format (TIFF) format images.
    /// </summary>
    public class TiffEncoder : ImageEncoder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TiffEncoder"/> class.
        /// </summary>
        public TiffEncoder()
        {
        }

        /// <inheritdoc />
        public override bool SupportsMultipleFrames => true;

        /// <inheritdoc />
        public override string FileExtensions => ".tif;.*tiff";

        /// <summary>
        /// Gets or sets a value that indicates the type of compression used by this Tagged Image File Format (TIFF) encoder.
        /// </summary>
        /// <value>
        /// One of the <see cref="TIFFCompression"/> values. The default is <see cref="TIFFCompression.Default"/>.
        /// </value>
        public TIFFCompression Compression { get; set; } = TIFFCompression.Default;

        /// <inheritdoc />
        public override void Save(Stream stream, Image image, ImageMetadata metadata)
        {
            Libtiff.Save(stream, image, metadata, this.Compression);
        }

        /// <inheritdoc />
        public override void Save(Stream stream, System.Collections.Generic.IEnumerable<(Image image, ImageMetadata metadata)> images)
        {
            Libtiff.Save(stream, images, this.Compression);
        }
    }
}
