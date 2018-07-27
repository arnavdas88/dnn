// -----------------------------------------------------------------------
// <copyright file="TIFFSubfileType.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the SubfileType field.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum TIFFSubfileType
    {
        /// <summary>
        /// Full-resolution image data.
        /// </summary>
        FullResolution = 1,

        /// <summary>
        /// Reduced-resolution image data.
        /// </summary>
        ReducedResolution = 2,

        /// <summary>
        /// A single page of a multi-page image.
        /// </summary>
        PageOfMultipage = 3
    }
}
