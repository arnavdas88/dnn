// -----------------------------------------------------------------------
// <copyright file="PageSegmentationMode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;

    /// <summary>
    /// Defines the Tesseract page segmentation mode.
    /// </summary>
    /// <remarks>
    /// Possible modes for page layout analysis. These *must* be kept in order
    /// of decreasing amount of layout analysis to be done, except for OSD_ONLY,
    /// so that the inequality test macros below work.
    /// </remarks>
    public enum PageSegmentationMode
    {
        /// <summary>
        /// Orientation and script detection only.
        /// </summary>
        PSM_OSD_ONLY,

        /// <summary>
        /// Automatic page segmentation with orientation and script detection. (OSD)
        /// </summary>
        PSM_AUTO_OSD,

        /// <summary>
        /// Automatic page segmentation, but no OSD, or OCR.
        /// </summary>
        PSM_AUTO_ONLY,

        /// <summary>
        /// Fully automatic page segmentation, but no OSD.
        /// </summary>
        PSM_AUTO,

        /// <summary>
        /// Assume a single column of text of variable sizes.
        /// </summary>
        PSM_SINGLE_COLUMN,

        /// <summary>
        /// Assume a single uniform block of vertically aligned text.
        /// </summary>
        PSM_SINGLE_BLOCK_VERT_TEXT,

        /// <summary>
        /// Assume a single uniform block of text. (Default.)
        /// </summary>
        PSM_SINGLE_BLOCK,

        /// <summary>
        /// Treat the image as a single text line.
        /// </summary>
        PSM_SINGLE_LINE,

        /// <summary>
        /// Treat the image as a single word.
        /// </summary>
        PSM_SINGLE_WORD,

        /// <summary>
        /// Treat the image as a single word in a circle.
        /// </summary>
        PSM_CIRCLE_WORD,

        /// <summary>
        /// Treat the image as a single character.
        /// </summary>
        PSM_SINGLE_CHAR,

        /// <summary>
        /// Find as much text as possible in no particular order.
        /// </summary>
        PSM_SPARSE_TEXT,

        /// <summary>
        /// Sparse text with orientation and script detection.
        /// </summary>
        PSM_SPARSE_TEXT_OSD,

        /// <summary>
        /// Treat the image as a single text line, bypassing hacks that are Tesseract-specific.
        /// </summary>
        PSM_RAW_LINE,
    }
}
