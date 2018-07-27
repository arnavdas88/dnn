// -----------------------------------------------------------------------
// <copyright file="EXIFMeteringMode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the <see cref="EXIFField.MeteringMode"/> field.
    /// </summary>
    public enum EXIFMeteringMode
    {
        /// <summary>
        /// The unknown metering mode.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The average metering mode.
        /// </summary>
        Average = 1,

        /// <summary>
        /// The center-weighted average metering mode.
        /// </summary>
        CenterWeightedAverage = 2,

        /// <summary>
        /// The spot metering mode.
        /// </summary>
        Spot = 3,

        /// <summary>
        /// The multi-spot metering mode.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "TIFF specifications.")]
        MultiSpot = 4,

        /// <summary>
        /// The pattern metering mode.
        /// </summary>
        Pattern = 5,

        /// <summary>
        /// The partial metering mode.
        /// </summary>
        Partial = 6,

        /// <summary>
        /// The other metering mode.
        /// </summary>
        Other = 255,
    }
}
