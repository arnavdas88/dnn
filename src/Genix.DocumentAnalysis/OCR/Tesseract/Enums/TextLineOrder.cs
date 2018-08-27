// -----------------------------------------------------------------------
// <copyright file="TextLineOrder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    /// <summary>
    /// Defines the text lines direction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The text lines are read in the given sequence.
    /// </para>
    /// <para>
    /// In English, the order is top-to-bottom.
    /// In Chinese, vertical text lines are read right-to-left.
    /// Mongolian is written in vertical columns top to bottom like Chinese, but the lines order left-to right.
    /// </para>
    /// <para>
    /// Note that only some combinations make sense. For example, <see cref="WritingDirection.LeftToRight"/> implies <see cref="TextLineOrder.TopToBottom"/>.
    /// </para>
    /// </remarks>
    public enum TextLineOrder
    {
        /// <summary>
        /// The left-to-right line direction.
        /// </summary>
        LeftToRight = 0,

        /// <summary>
        /// The right-to-left line direction (Chinese).
        /// </summary>
        RightToLeft = 1,

        /// <summary>
        /// The top-to-bottom line direction (English).
        /// </summary>
        TopToBottom = 2,
    }
}
