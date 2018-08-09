// -----------------------------------------------------------------------
// <copyright file="ImagePreprocessingOptions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;

    /// <summary>
    /// Defines the image preprocessing options.
    /// </summary>
    [Flags]
    public enum ImagePreprocessingOptions
    {
        /// <summary>
        /// None of the options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Removes small isolated noise and fills small gaps.
        /// </summary>
        Despeckle = 1,

        /// <summary>
        /// Removes black over scan border.
        /// </summary>
        CleanOverscan = 2,

        /// <summary>
        /// Aligns the image horizontally.
        /// </summary>
        Deskew = 4,
    }
}
