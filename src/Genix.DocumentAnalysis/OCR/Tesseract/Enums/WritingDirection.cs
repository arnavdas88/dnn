// -----------------------------------------------------------------------
// <copyright file="WritingDirection.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    /// <summary>
    /// Defines the text writing direction.
    /// </summary>
    /// <remarks>
    /// The grapheme clusters within a line of text are laid out logically in this direction,
    /// judged when looking at the text line rotated so that its <see cref="Orientation"/> is <see cref="Orientation.PageUp"/>.
    /// </remarks>
    public enum WritingDirection
    {
        /// <summary>
        /// The left-to-right writing direction (English).
        /// </summary>
        LeftToRight = 0,

        /// <summary>
        /// The right-to-left writing direction.
        /// </summary>
        RightToLeft = 1,

        /// <summary>
        /// The top-to-bottom writing direction (Chinese).
        /// </summary>
        TopToBottom = 2,
    }
}
