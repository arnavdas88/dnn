// -----------------------------------------------------------------------
// <copyright file="PageIteratorLevel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    /// <summary>
    /// Defines the elements of the page hierarchy.
    /// </summary>
    /// <remarks>
    /// The enum is used in ResultIterator to provide functions that operate on each level without having to have 5x as many functions.
    /// </remarks>
    public enum PageIteratorLevel
    {
        /// <summary>
        /// Block of text/image/separator line.
        /// </summary>
        TextBlock,

        /// <summary>
        /// Paragraph within a block.
        /// </summary>
        Paragraph,

        /// <summary>
        /// Line within a paragraph.
        /// </summary>
        TextLine,

        /// <summary>
        /// Word within a textline.
        /// </summary>
        Word,

        /// <summary>
        /// Symbol/character within a word.
        /// </summary>
        Symbol,
    }
}
