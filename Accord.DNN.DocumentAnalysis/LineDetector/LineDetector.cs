// -----------------------------------------------------------------------
// <copyright file="LineDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.DocumentAnalysis
{
    using Accord.DNN.Imaging;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public static class LineDetector
    {
        public static void DetectVerticalLines(Imaging.Image image)
        {
            int[] histy = image.HistogramY();
        }
    }
}