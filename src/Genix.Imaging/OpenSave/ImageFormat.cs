// -----------------------------------------------------------------------
// <copyright file="ImageFormat.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the format for the image file.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// A unknown image format.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A TIFF image format.
        /// </summary>
        Tiff,

        /// <summary>
        /// A BMP image format.
        /// </summary>
        Bmp,

        /// <summary>
        /// A JPEG image format.
        /// </summary>
        Jpeg,

        /// <summary>
        /// A PNG image format.
        /// </summary>
        Png,

        /// <summary>
        /// A GIF image format.
        /// </summary>
        Gif,
    }
}
