// -----------------------------------------------------------------------
// <copyright file="PolyBlockType.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    /// <summary>
    /// Defines the type of page iterator block.
    /// </summary>
    public enum PolyBlockType
    {
        /// <summary>
        /// Type is not yet known. Keep as the first element.
        /// </summary>
        Unknown,

        /// <summary>
        /// Text that lives inside a column.
        /// </summary>
        FlowingText,

        /// <summary>
        /// Text that spans more than one column.
        /// </summary>
        HeadingText,

        /// <summary>
        /// Text that is in a cross-column pull-out region.
        /// </summary>
        PulloutText,

        /// <summary>
        /// Partition belonging to an equation region.
        /// </summary>
        Equation,

        /// <summary>
        /// Partition has inline equation.
        /// </summary>
        InlineEquation,

        /// <summary>
        /// Partition belonging to a table region.
        /// </summary>
        Table,

        /// <summary>
        /// Text-line runs vertically.
        /// </summary>
        VerticalText,

        /// <summary>
        /// Text that belongs to an image.
        /// </summary>
        CaptionText,

        /// <summary>
        /// Image that lives inside a column.
        /// </summary>
        FlowingImage,

        /// <summary>
        /// Image that spans more than one column.
        /// </summary>
        HeadingImage,

        /// <summary>
        /// Image that is in a cross-column pull-out region.
        /// </summary>
        PulloutImage,

        /// <summary>
        /// Horizontal Line.
        /// </summary>
        HorizontalLine,

        /// <summary>
        /// Vertical Line.
        /// </summary>
        VerticalLine,

        /// <summary>
        /// Lies outside of any column.
        /// </summary>
        Noise,
    }
}
