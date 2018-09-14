// -----------------------------------------------------------------------
// <copyright file="LineDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
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

            HashSet<ConnectedComponent> result = new HashSet<ConnectedComponent>();

            // close up small holes
            int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
            Image closedImage = Image.MorphClose(image, StructuringElement.Square(maxLineWidth / 3), 1);

            // open up to detect big solid areas
            Image openedImage = Image.MorphOpen(image, StructuringElement.Square(maxLineWidth), 1);
            Image hollowImage = Image.Sub(closedImage, openedImage, 0);

            // open up in both directions to find lines
            int minLineLength = LineDetector.MinLineLength.MulDiv(image.HorizontalResolution, 200);
            Image hlines = Image.MorphOpen(hollowImage, StructuringElement.Rectangle(minLineLength, 1), 1);
            Image vlines = Image.MorphOpen(hollowImage, StructuringElement.Rectangle(1, minLineLength), 1);

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
            Image itersections = null;
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
                Image nonVLines = Image.Erode(nonLines, StructuringElement.Rectangle(maxLineResidue, 1), 1);

                nonVLines.FloodFill(nonLines);

                if (hlines != null)
                {
                    nonVLines.Add(hlines, 0);
                    nonVLines.Sub(itersections, 0);
                }

                result.UnionWith(FilterLines(vlines, nonVLines, true));
            }

            // horizontal lines
            if (hlines != null)
            {
                if (nonLines == null)
                {
                    nonLines = Image.Sub(image, hlines, 0);
                }

                int maxLineResidue = LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                Image nonHLines = Image.Erode(nonLines, StructuringElement.Rectangle(1, maxLineResidue), 1);

                nonHLines.FloodFill(nonLines);

                if (nonHLinesExtra != null)
                {
                    nonHLines.Or(nonHLinesExtra);
                }

                result.UnionWith(FilterLines(hlines, nonHLines, false));
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

            return result;

            IEnumerable<ConnectedComponent> FilterLines(Image linesImage, Image nonLinesImage, bool vertical)
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

                ISet<ConnectedComponent> components = linesImage.FindConnectedComponents();
                int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(image.HorizontalResolution, 200);
                int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(image.HorizontalResolution, 200);

                foreach (ConnectedComponent component in components)
                {
                    Rectangle bounds = component.Bounds;
                    Image comp = component.ToImage();

                    // calculate maximum component width as twice the maximum distance between its points and the background
                    Image compDist = comp.DistanceFunction(4, 8);
                    compDist.MinMax(out byte min1, out byte max1);

                    int maxWidth;
                    using (Pix pixComp = Pix.FromImage(comp))
                    {
                        using (Pix pixDist = pixComp.DistanceFunction(4, 8, 1))
                        {
                            pixDist.ToImage().MinMax(out byte min, out byte max);
                            maxWidth = max * 2;
                        }
                    }

                    bool isBad = false;

                    if (bounds.Width.InRange(minThickLineWidth, minThickLineLength) &&
                        bounds.Height.InRange(minThickLineWidth, minThickLineLength) &&
                        maxWidth > minThickLineLength)
                    {
                        // too thick for the length
                        isBad = true;
                    }

                    // Test density near the line
                    if (!isBad)
                    {
                        long count = CountAdjacentPixels(maxWidth, bounds);
                        if (count > bounds.Area * LineDetector.MaxNonLineDensity)
                        {
                            isBad = true;
                        }
                    }

                    if (isBad)
                    {
                        linesImage.SetWhite(bounds);
                    }
                    else
                    {
                        yield return component;
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
                }

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

                    Image temp = nonLinesImage.Crop(bounds);

                    return nonLinesImage.Power(bounds);
                }
            }
        }
    }
}