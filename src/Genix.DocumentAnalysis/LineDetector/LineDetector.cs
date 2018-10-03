// -----------------------------------------------------------------------
// <copyright file="LineDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;
    using Genix.Imaging.Leptonica;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public static class LineDetector
    {
        /// <summary>
        /// The maximum line width, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MaxLineWidth = 10;

        /// <summary>
        /// The minimum line length, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinLineLength = 50;

        /// <summary>
        /// The maximum size of line residue, in pixels, for images with resolution 200 dpi.
        /// </summary>
        /// <remarks>
        /// These are the pixels that fail the long thin opening,
        /// and therefore don't make it to the candidate line mask,
        /// but are nevertheless part of the line.
        /// </remarks>
        private const int MaxLineResidue = 6;

        /// <summary>
        /// The minimum line thickness, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinThickLineWidth = 12;

        /// <summary>
        /// The minimum length of a line segment, in pixels, for images with resolution 200 dpi,
        /// that is thicker than <see cref="MinThickLineWidth"/>.
        /// </summary>
        private const int MinThickLineLength = 150;

        /// <summary>
        /// The maximum fraction of the area adjacent to line that can be occupied by pixels.
        /// </summary>
        private const float MaxNonLineDensity = 0.25f;

        /// <summary>
        /// Finds the horizontal and vertical lines on the <see cref="Image"/>.
        /// The type of lines to find is determined by the <c>options</c> parameter.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="options">The parameters of this method.</param>
        /// <returns>
        /// The detected lines.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not one.
        /// </exception>
        /// <remarks>
        /// <para>This method works with binary (1bpp) images only.</para>
        /// </remarks>
        public static ISet<LineShape> FindLines(Image image, LineDetectionOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ISet<Line> hlinesResult = null;
            ISet<Line> vlinesResult = null;

            // masks we would like to find
            Image hlines = null;
            Image vlines = null;
            Image nonHLines = null;
            Image nonVLines = null;
            Image itersections = null;
            FindLines();

            // remove the lines
            if (vlines != null)
            {
                RemoveLines(vlines, nonVLines);
            }

            if (hlines != null)
            {
                // recompute intersections
                itersections = vlines?.And(null, hlines);

                // re-filter lines
                hlinesResult = FilterLines(hlines, nonHLines, false);

                RemoveLines(hlines, nonHLines);
            }

            RemoveIntersections();

            return CreateAnswer();

            void FindLines()
            {
                 // close up small holes
                int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
                Image closedImage = image.MorphClose(null, StructuringElement.Square(maxLineWidth / 3), 1, BorderType.BorderConst, image.WhiteColor);

                // open up to detect big solid areas
                Image openedImage = closedImage.MorphOpen(null, StructuringElement.Square(maxLineWidth), 1, BorderType.BorderConst, image.WhiteColor);
                Image hollowImage = closedImage.Sub(null, openedImage, 0);

                // open up in both directions to find lines
                int minLineLength = LineDetector.MinLineLength.MulDiv(image.HorizontalResolution, 200);
                hlines = hollowImage.MorphOpen(null, StructuringElement.Brick(minLineLength, 1), 1, BorderType.BorderConst, image.WhiteColor);
                vlines = hollowImage.MorphOpen(null, StructuringElement.Brick(1, minLineLength), 1, BorderType.BorderConst, image.WhiteColor);

                // check for line presence
                if (hlines.IsAllWhite())
                {
                    hlines = null;
                }

                if (vlines.IsAllWhite())
                {
                    vlines = null;
                }

                // various working images
                Image nonLines = null;
                Image nonHLinesExtra = null;

                // vertical lines
                if (vlines != null)
                {
                    nonLines = image.Sub(null, vlines, 0);
                    if (hlines != null)
                    {
                        nonLines.Sub(nonLines, hlines, 0);
                        itersections = hlines.And(null, vlines);
                        nonHLinesExtra = vlines.Sub(null, itersections, 0);
                    }

                    int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonVLines = nonLines.Erode(null, StructuringElement.Brick(maxLineResidue, 1), 1, BorderType.BorderConst, image.WhiteColor);

                    nonVLines.FloodFill(nonVLines, 8, nonLines);

                    if (hlines != null)
                    {
                        nonVLines.Add(nonVLines, hlines, 0);
                        nonVLines.Sub(nonVLines, itersections, 0);
                    }

                    vlinesResult = FilterLines(vlines, nonVLines, true);
                }

                // horizontal lines
                if (hlines != null)
                {
                    if (nonLines == null)
                    {
                        nonLines = image.Sub(null, hlines, 0);
                    }

                    int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonHLines = nonLines.Erode(null, StructuringElement.Brick(1, maxLineResidue), 1, BorderType.BorderConst, image.WhiteColor);

                    nonHLines.FloodFill(nonHLines, 8, nonLines);

                    if (nonHLinesExtra != null)
                    {
                        nonHLines.Or(nonHLines, nonHLinesExtra);
                    }

                    hlinesResult = FilterLines(hlines, nonHLines, false);
                }
            }

            ISet<Line> FilterLines(Image lines, Image nonLines, bool vertical)
            {
                HashSet<Line> answer = new HashSet<Line>();

                /*using (Pix pixLines = Pix.FromImage(lines))
                {
                    Pixa pixa = null;
                    try
                    {
                        using (Boxa boxa = pixLines.FindConnectedComponents(8, out pixa))
                        {
                            for (int i = 0, ii = boxa.Count; i < ii; i++)
                            {
                                using (Box box = boxa[i])
                                {
                                    Rectangle bounds = box.GetBounds();
                                    using (Pix pixComp = pixa[i])
                                    {
                                        using (Pix pixDist = pixComp.DistanceFunction(4, 8, 1))
                                        {
                                            pixDist.ToImage().MinMax(out byte min, out byte max);
                                            int maxWidth = max * 2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        pixa?.Dispose();
                    }
                }*/

                int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(
                    vertical ? image.HorizontalResolution : image.VerticalResolution,
                    200);
                int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(
                    vertical ? image.VerticalResolution : image.HorizontalResolution,
                    200);

                foreach (ConnectedComponent component in lines.FindConnectedComponents(8))
                {
                    Rectangle bounds = component.Bounds;

                    // calculate maximum component width as twice the maximum distance between its points and the background
                    int maxWidth = (int)component.ToImage().DistanceToBackground(4, 8).Max() * 2;

                    /*int maxWidth = 0;
                    using (Pix pixComp = Pix.FromImage(comp))
                    {
                        using (Pix pixDist = pixComp.DistanceFunction(4, 8, 1))
                        {
                            pixDist.ToImage().MinMax(out uint min, out uint max);
                            int maxWidth2 = (int)(max * 2);
                            Debug.Assert(maxWidth == maxWidth2);
                        }
                    }*/

                    bool isBad = false;

                    if (bounds.Width.Between(minThickLineWidth, minThickLineLength, false) &&
                        bounds.Height.Between(minThickLineWidth, minThickLineLength, false) &&
                        maxWidth > minThickLineLength)
                    {
                        // too thick for the length
                        isBad = true;
                    }

                    // Test density near the line if there are not enough intersections
                    if (!isBad && CountIntersections(bounds) < 2)
                    {
                        long count = CountAdjacentPixels(maxWidth, bounds);
                        if (count > bounds.Area * LineDetector.MaxNonLineDensity)
                        {
                            isBad = true;
                        }
                    }

                    if (isBad)
                    {
                        lines.SetWhite(bounds);
                    }
                    else
                    {
                        answer.Add(new Line()
                        {
                            Component = component,
                            Width = maxWidth,
                            IsVertical = vertical,
                        });
                    }
                }

                return answer;

                long CountAdjacentPixels(int lineWidth, Rectangle bounds)
                {
                    if (vertical)
                    {
                        bounds.Inflate(lineWidth, 0);
                    }
                    else
                    {
                        bounds.Inflate(0, lineWidth);
                    }

                    bounds.Intersect(nonLines.Bounds);
                    return nonLines.Power(bounds);
                }
            }

            int CountIntersections(Rectangle bounds)
            {
                return itersections?.FindConnectedComponents(8, bounds)?.Count ?? 0;
            }

            void RemoveLines(Image lines, Image nonLines)
            {
                // remove the lines
                image.Xand(image, lines);

                // dilate the lines so they touch the residue
                // then flood fill then to get all the residue (image less non-lines)
                Image fatLines = lines.Dilate(null, StructuringElement.Square(3), 1, BorderType.BorderConst, image.WhiteColor);

                fatLines.FloodFill(fatLines, 8, image.Xand(null, nonLines));

                // remove the residue
                image.Xand(image, fatLines);
            }

            void RemoveIntersections()
            {
                if (hlines != null && vlines != null)
                {
                    // get the intersection residue
                    Image residue = hlines
                        .And(null, vlines)
                        .Dilate(null, StructuringElement.Square(5), 1, BorderType.BorderConst, image.WhiteColor);

                    residue.FloodFill(residue, 8, image);

                    // remove the residue
                    image.Xand(image, residue);
                }
            }

            ISet<LineShape> CreateAnswer()
            {
                HashSet<LineShape> answer = new HashSet<LineShape>();

                if (hlinesResult != null)
                {
                    foreach (Line line in hlinesResult)
                    {
                        answer.Add(new LineShape(line.Component.Bounds, line.Width, LineTypes.Horizontal));
                    }
                }

                if (vlinesResult != null)
                {
                    foreach (Line line in vlinesResult)
                    {
                        answer.Add(new LineShape(line.Component.Bounds, line.Width, LineTypes.Vertical));
                    }
                }

                return answer;
            }
        }

        private class Line
        {
            public ConnectedComponent Component { get; set; }

            public int Width { get; set; }

            public bool IsVertical { get; set; }
        }
    }
}