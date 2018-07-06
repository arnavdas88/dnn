﻿// -----------------------------------------------------------------------
// <copyright file="TIFFNewSubfileTypes.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the NewSubfileType field.
    /// </summary>
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is the spelling by TIFF specs.")]
    public enum TIFFNewSubfileTypes
    {
        /// <summary>
        /// Bit 0 is 1 if the image is a reduced-resolution version of another image in this TIFF file; else the bit is 0.
        /// </summary>
        ReducedResolution = 1,

        /// <summary>
        /// Bit 1 is 1 if the image is a single page of a multi-page image (see the PageNumber field description); else the bit is 0.
        /// </summary>
        PageOfMultipage = 2,

        /// <summary>
        /// Bit 2 is 1 if the image defines a transparency mask for another image in this TIFF file.
        /// The PhotometricInterpretation value must be 4, designating a transparency mask.
        /// </summary>
        TransparencyMask = 4
    }
}
