// -----------------------------------------------------------------------
// <copyright file="ParagraphJustification.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;

    /// <summary>
    /// Defines the paragraph justification.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NOTA BENE: Fully justified paragraphs (text aligned to both left and right margins)
    /// are marked by Tesseract with <see cref="ParagraphJustification.Left"/> if their text is written with a left-to-right script
    /// and with <see cref="ParagraphJustification.Right"/> if their text is written in a right-to-left script.
    /// </para>
    /// <para>
    /// Interpretation for text read in vertical lines: "Left" is wherever the starting reading position is.
    /// </para>
    /// </remarks>
    public enum ParagraphJustification
    {
        /// <summary>
        /// The alignment is not clearly one of the other options.
        /// This could happen for example if there are only one or two lines of text or the text looks like source code or poetry.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Each line, except possibly the first, is flush to the same left tab stop.
        /// </summary>
        Left,

        /// <summary>
        /// The text lines of the paragraph are centered about a line going down through their middle of the text lines.
        /// </summary>
        Center,

        /// <summary>
        /// Each line, except possibly the first, is flush to the same right tab stop.
        /// </summary>
        Right,
    }
}
