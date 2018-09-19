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
        public static ISet<ConnectedComponent> FindLines(Image image, LineDetectionOptions options)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

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
                itersections = vlines != null ? Image.And(hlines, vlines) : null;

                // re-filter lines
                FilterLines(hlines, nonHLines, false);

                RemoveLines(hlines, nonHLines);
            }

            RemoveIntersections();

            return null; //// result;

            void FindLines()
            {
                // result
                ISet<ConnectedComponent> hlinesResult = null;
                ISet<ConnectedComponent> vlinesResult = null;
                HashSet<ConnectedComponent> result = new HashSet<ConnectedComponent>();

                // close up small holes
                int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
                Image closedImage = image.MorphClose(StructuringElement.Square(maxLineWidth / 3), 1);

                // open up to detect big solid areas
                Image openedImage = image.MorphOpen(StructuringElement.Square(maxLineWidth), 1);
                Image hollowImage = Image.Sub(closedImage, openedImage, 0);

                // open up in both directions to find lines
                int minLineLength = LineDetector.MinLineLength.MulDiv(image.HorizontalResolution, 200);
                hlines = hollowImage.MorphOpen(StructuringElement.Rectangle(minLineLength, 1), 1);
                vlines = hollowImage.MorphOpen(StructuringElement.Rectangle(1, minLineLength), 1);

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
                    nonLines = Image.Sub(image, vlines, 0);
                    if (hlines != null)
                    {
                        nonLines.Sub(hlines, 0);
                        itersections = Image.And(hlines, vlines);
                        nonHLinesExtra = Image.Sub(vlines, itersections, 0);
                    }

                    int maxLineResidue = LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonVLines = nonLines.Erode(StructuringElement.Rectangle(maxLineResidue, 1), 1);

                    nonVLines.FloodFillIP(8, nonLines);

                    if (hlines != null)
                    {
                        nonVLines.Add(hlines, 0);
                        nonVLines.Sub(itersections, 0);
                    }

                    vlinesResult = FilterLines(vlines, nonVLines, true);
                }

                // horizontal lines
                if (hlines != null)
                {
                    if (nonLines == null)
                    {
                        nonLines = Image.Sub(image, hlines, 0);
                    }

                    int maxLineResidue = LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonHLines = nonLines.Erode(StructuringElement.Rectangle(1, maxLineResidue), 1);

                    nonHLines.FloodFillIP(8, nonLines);

                    if (nonHLinesExtra != null)
                    {
                        nonHLines.Or(nonHLinesExtra);
                    }

                    hlinesResult = FilterLines(hlines, nonHLines, false);
                }

                /*Image res = Image.Sub(image, hlines, 0);
                res.Sub(vlines, 0);

                // find horizontal lines
                if (options.Types.HasFlag(LineTypes.Horizontal))
                {
                    // morphology open leaves only pixels that have required number of horizontal neighbors
                    Image workImage = Image.Open(image, StructuringElement.Rectangle(30, 1), 1);

                    // dilate lines to merge small gaps and include neighboring isolated pixels
                    const int DilationSize = 2;
                    workImage.Dilate(StructuringElement.Square(1 + (2 * DilationSize)), 1);

                    // find line components and filter out non-line components
                    minLineLength = (int)((options.MinLineLength * image.HorizontalResolution) + 0.5f);
                    List<ConnectedComponent> components = workImage.FindConnectedComponents()
                                                                   .Where(x => x.Bounds.Width >= minLineLength)
                                                                   .ToList();

                    // add components to found lines
                    foreach (ConnectedComponent component in components)
                    {
                        int y = (component.Bounds.Top + component.Bounds.Bottom) / 2;
                        lineShapes.Add(new LineShape(
                            new Point(component.Bounds.Left, y),
                            new Point(component.Bounds.Right, y),
                            MinMax.Max(1, component.Bounds.Height - (2 * DilationSize)),
                            LineTypes.Horizontal));
                    }

                    // create mask image
                    Image mask = new Image(
                        workImage.Width,
                        workImage.Height,
                        1,
                        workImage.HorizontalResolution,
                        workImage.VerticalResolution);
                    mask.AddConnectedComponents(components);
                    mask.And(image);

                    // apply mask
                    cleanedImage = image ^ mask;

                    // dilate vertically to close gaps in vertical lines opened by lines removal
                    cleanedImage.Dilate(StructuringElement.Rectangle(1, 7), 1);
                    cleanedImage = cleanedImage & image;
                }*/
            }

            ISet<ConnectedComponent> FilterLines(Image lines, Image nonLines, bool vertical)
            {
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

                HashSet<ConnectedComponent> components = new HashSet<ConnectedComponent>(lines.FindConnectedComponents(8));
                components.RemoveWhere(component =>
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
                        /*if (vertical)
                        {
                            yield return new LineShape(
                                new Point(bounds.X + (bounds.Width / 2), bounds.Y),
                                new Point(bounds.X + (bounds.Width / 2), bounds.Bottom),
                                bounds.Width,
                                LineTypes.Vertical);
                        }
                        else
                        {
                            yield return new LineShape(
                                new Point(bounds.X, bounds.Y + (bounds.Height / 2)),
                                new Point(bounds.Right, bounds.Y + (bounds.Height / 2)),
                                bounds.Height,
                                LineTypes.Horizontal);
                        }*/
                    }

                    return isBad;
                });

                return components;

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
                image.Xand(lines);

                // dilate the lines so they touch the residue
                // then flood fill then to get all the residue (image less non-lines)
                Image fatLines = lines.Dilate(StructuringElement.Square(3), 1);
                fatLines.FloodFillIP(8, Image.Xand(image, nonLines));

                // remove the residue
                image.Xand(fatLines);
            }

            void RemoveIntersections()
            {
                if (hlines != null && vlines != null)
                {
                    // get the intersection residue
                    Image residue = Image.And(hlines, vlines)
                        .Dilate(StructuringElement.Square(5), 1);
                    residue.FloodFillIP(8, image);

                    // remove the residue
                    image.Xand(residue);
                }
            }
        }
    }
}