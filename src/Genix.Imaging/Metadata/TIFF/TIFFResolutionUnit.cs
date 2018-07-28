// -----------------------------------------------------------------------
// <copyright file="TIFFResolutionUnit.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the ResolutionUnit field.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum TIFFResolutionUnit
    {
        /// <summary>
        /// No absolute unit of measurement.
        /// Used for images that may have a non-square aspect ratio, but no meaningful absolute dimensions.
        /// </summary>
        None = 1,

        /// <summary>
        /// The unit of measurement is inch (the default).
        /// </summary>
        Inches = 2,

        /// <summary>
        /// The unit of measurement is centimeter.
        /// </summary>
        Centimeters = 3,
    }
}
