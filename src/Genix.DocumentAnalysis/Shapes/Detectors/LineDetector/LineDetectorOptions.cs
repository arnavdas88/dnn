// -----------------------------------------------------------------------
// <copyright file="LineDetectorOptions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    /// <summary>
    /// Represents the options for the line detection.
    /// </summary>
    public class LineDetectorOptions
    {
        /// <summary>
        /// Gets or sets the value indicating the types of lines to locate.
        /// </summary>
        /// <value>
        /// The <see cref="LineTypes"/> enumeration.
        /// The default value is <see cref="LineTypes.All"/>.
        /// </value>
        public LineTypes Types { get; set; } = LineTypes.All;

        /// <summary>
        /// Gets or sets the minimum line length to look for.
        /// </summary>
        /// <value>
        /// The minimum line length, in inches.
        /// The default value is 1.
        /// </value>
        public float MinLineLength { get; set; } = 1.0f;
    }
}
