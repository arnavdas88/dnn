// -----------------------------------------------------------------------
// <copyright file="ImageFormat.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <summary>
    /// Specifies the format for the image file.
    /// </summary>
    [Flags]
    public enum ImageFormat
    {
        /// <summary>
        /// A unknown image format.
        /// </summary>
        None = 0,

        /// <summary>
        /// A TIFF image format.
        /// </summary>
        Tiff = 1,

        /// <summary>
        /// A BMP image format.
        /// </summary>
        Bmp = 2,

        /// <summary>
        /// A JPEG image format.
        /// </summary>
        Jpeg = 4,

        /// <summary>
        /// A PNG image format.
        /// </summary>
        Png = 8,

        /// <summary>
        /// A GIF image format.
        /// </summary>
        Gif = 16,

        /// <summary>
        /// All image formats.
        /// </summary>
        All = Tiff | Bmp | Jpeg | Png | Gif,
    }
}
