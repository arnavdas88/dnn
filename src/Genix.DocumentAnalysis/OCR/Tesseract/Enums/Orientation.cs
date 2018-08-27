// -----------------------------------------------------------------------
// <copyright file="Orientation.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    /// <summary>
    /// Defines the block orientation.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  +------------------+  Orientation Example:
    ///  | 1 Aaaa Aaaa Aaaa |  ====================
    ///  | Aaa aa aaa aa    |  To left is a diagram of some (1) English and
    ///  | aaaaaa A aa aaa. |  (2) Chinese text and a (3) photo credit.
    ///  |                2 |
    ///  |   #######  c c C |  Upright Latin characters are represented as A and a.
    ///  |   #######  c c c |  '&lt;' represents a latin character rotated
    ///  | &lt; #######  c c c |      anti-clockwise 90 degrees.
    ///  | &lt; #######  c   c |
    ///  | &lt; #######  .   c |  Upright Chinese characters are represented C and c.
    ///  | 3 #######      c |
    ///  +------------------+  NOTA BENE: enum values here should match goodoc.proto.
    /// </para>
    /// <para>
    /// If you orient your head so that "up" aligns with Orientation,
    /// then the characters will appear "right side up" and readable.
    /// </para>
    /// <para>
    /// In the example above, both the English and Chinese paragraphs are oriented
    /// so their "up" is the top of the page (page up).  The photo credit is read
    /// with one's head turned leftward ("up" is to page left).
    /// </para>
    /// <para>
    /// The values of this enum match the convention of Tesseract's osdetect.h.
    /// </para>
    /// </remarks>
    public enum Orientation
    {
        /// <summary>
        /// Page up orientation.
        /// </summary>
        PageUp = 0,

        /// <summary>
        /// Page right orientation.
        /// </summary>
        PageRight = 1,

        /// <summary>
        /// Page down orientation.
        /// </summary>
        PageDown = 2,

        /// <summary>
        /// Page left orientation.
        /// </summary>
        PageLeft = 3,
    }
}
