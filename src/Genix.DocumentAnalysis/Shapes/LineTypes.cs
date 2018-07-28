// -----------------------------------------------------------------------
// <copyright file="LineTypes.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;

    /// <summary>
    /// Defines the line types.
    /// </summary>
    [Flags]
    public enum LineTypes
    {
        /// <summary>
        /// None of the lines.
        /// </summary>
        None = 0,

        /// <summary>
        /// The horizontal line.
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// The vertical line.
        /// </summary>
        Vertical = 2,

        /// <summary>
        /// All the lines.
        /// </summary>
        All = Horizontal | Vertical,
    }
}
