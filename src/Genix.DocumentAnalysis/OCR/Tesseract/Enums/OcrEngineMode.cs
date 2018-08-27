// -----------------------------------------------------------------------
// <copyright file="OcrEngineMode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;

    /// <summary>
    /// Defines the Tesseract engine mode.
    /// </summary>
    public enum OcrEngineMode
    {
        /// <summary>
        /// Run Tesseract only - fastest; deprecated.
        /// </summary>
        [Obsolete("TesseractOnly is deprecated, please use LstmOnly or Default instead.")]
        TesseractOnly = 0,

        /// <summary>
        /// Run just the LSTM line recognizer.
        /// </summary>
        LstmOnly,

        /// <summary>
        /// Run the LSTM recognizer,
        /// but allow fallback to Tesseract when things get difficult; deprecated.
        /// </summary>
        [Obsolete("OEM_TESSERACT_LSTM_COMBINED is deprecated, please use LstmOnly or Default instead.")]
        TesseractLstmCombined,

        /// <summary>
        /// Specify this mode to indicate that any of the above modes should be automatically inferred from the
        /// variables in the language-specific config, command-line configs, or if not specified
        /// in any of the above should be set to the default <see cref="OcrEngineMode.TesseractOnly"/>.
        /// </summary>
        Default,
    }
}
