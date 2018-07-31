// -----------------------------------------------------------------------
// <copyright file="OCR.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR
{
    using Genix.Imaging;

    /// <summary>
    /// Defines the contract for the Optical-Character-Recognition (OCR) engines.
    /// </summary>
    public abstract class OCR
    {
        /// <summary>
        /// Recognizes the specified image.
        /// </summary>
        /// <param name="image">The image to recognize.</param>
        /// <returns>The <see cref="PageShape"/> object that contains the result of recognition.</returns>
        public abstract PageShape Recognize(Image image);
    }
}
