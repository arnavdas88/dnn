// -----------------------------------------------------------------------
// <copyright file="LineDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using Genix.Core;
    using Genix.Imaging;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public static class LineDetector
    {
        public static void DetectVerticalLines(Image image)
        {
            Histogram histy = image.HistogramY();
        }
    }
}