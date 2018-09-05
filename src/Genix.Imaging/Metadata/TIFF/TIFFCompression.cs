// -----------------------------------------------------------------------
// <copyright file="TIFFCompression.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the values for the Compression field.
    /// </summary>
    public enum TIFFCompression
    {
        /// <summary>
        /// The encoder attempts to save the image with the best possible compression schema.
        /// </summary>
        Default = 0,

        /// <summary>
        /// No compression, but pack data into bytes as tightly as possible, leaving no unused bits (except at the end of a row).
        /// The component values are stored as an array of type BYTE.
        /// Each scan line (row) is padded to the next BYTE boundary.
        /// </summary>
        Packed = 1,

        /// <summary>
        /// CCITT Group 3 1-Dimensional Modified Huffman run length encoding.
        /// </summary>
        Huffman = 2,

        /// <summary>
        /// CCITT T.4 bi-level encoding as specified in section 4, Coding, of ITU-T Recommendation T.4 (a.k.a. CCITT Group 3 fax encoding or CCITT Group 3 2D).
        /// </summary>
        T4 = 3,

        /// <summary>
        /// CCITT T.6 bi-level encoding as specified in section 2 of ITU-T Recommendation T.6 (a.k.a. CCITT Group 4 fax encoding).
        /// </summary>
        T6 = 4,

        /// <summary>
        /// LZW (Lempel-Ziv &amp; Welch algorithm).
        /// </summary>
        LZW = 5,

        /// <summary>
        /// JPEG ('old-style' JPEG, later overridden in Technote2).
        /// </summary>
        JPEG = 6,

        /// <summary>
        /// JPEG ('new-style' JPEG).
        /// </summary>
        JPEGStream = 7,

        /// <summary>
        /// Deflate ('Adobe-style').
        /// </summary>
        Deflate = 8,

        /// <summary>
        /// JBIG, per ITU-T T.85.
        /// </summary>
        JBIG = 9,

        /// <summary>
        /// JBIG, per ITU-T T.43.
        /// </summary>
        JBIGMRC = 10,

        /// <summary>
        /// PackBits compression, a.k.a. Macintosh RLE.
        /// </summary>
        PackBits = 32773,

        /// <summary>
        /// NeXT RLE 2-bit gray scale encoding.
        /// </summary>
        NeXT = 32766,

        /// <summary>
        /// ThunderScan RLE 4-bit encoding.
        /// </summary>
        ThunderScan = 32809,

        /// <summary>
        /// RasterPadding in CT or MP (Continuous Tone or Monochrome Picture) encoding.
        /// </summary>
        RasterPadding = 32895,

        /// <summary>
        /// RLE for LW (Line Work) encoding.
        /// </summary>
        RLELW = 32896,

        /// <summary>
        /// RLE for HC (High-resolution Continuous-tone) encoding.
        /// </summary>
        RLEHC = 32897,

        /// <summary>
        /// RLE for BL (Binary Line work) encoding.
        /// </summary>
        RLEBL = 32898,

        /// <summary>
        /// Kodak DCS encoding.
        /// </summary>
        KodakDCS = 32947,

        /// <summary>
        /// JBIG encoding.
        /// </summary>
        JBIGLibTiff = 34661,

        /// <summary>
        /// JPEG2000 encoding.
        /// </summary>
        JPEG2000 = 34712,

        /// <summary>
        /// Nikon NEF Compressed.
        /// </summary>
        NikonNEF = 34713,

        /// <summary>
        /// Deflate (PKZIP-style Deflate encoding) (experimental).
        /// </summary>
        DeflateExperimental = 32946,
    }
}
