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
    using System.Threading;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;
    using Genix.Imaging.Leptonica;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public class LineDetector
    {
        /// <summary>
        /// The maximum line width, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MaxLineWidth = 10;

        /// <summary>
        /// The minimum horizontal line length, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinHorLineLength = 50;

        /// <summary>
        /// The minimum vertical line length, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinVerLineLength = 20;

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
        /// The maximum length of the gap within the line, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MaxLineGap = 40;

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
        /// Initializes a new instance of the <see cref="LineDetector"/> class.
        /// </summary>
        public LineDetector()
        {
        }

        /// <summary>
        /// Gets or sets the value indicating the types of lines to locate.
        /// </summary>
        /// <value>
        /// The <see cref="DocumentAnalysis.LineTypes"/> enumeration.
        /// The default value is <see cref="LineTypes.All"/>.
        /// </value>
        public LineTypes LineTypes { get; set; } = LineTypes.All;

        /// <summary>
        /// Gets or sets the minimum vertical line length to look for.
        /// </summary>
        /// <value>
        /// The minimum vertical line length, in inches.
        /// The default value is 0.5.
        /// </value>
        public float MinVerticalLineLength { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the minimum horizontal line length to look for.
        /// </summary>
        /// <value>
        /// The minimum horizontal line length, in inches.
        /// The default value is 1.
        /// </value>
        public float MinHorizontalLineLength { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets a value indicating whether the found lines should be removed from the image.
        /// </summary>
        /// <value>
        /// <b>true</b> to remove found lines from the image; otherwise, <b>false</b>.
        /// </value>
        private bool RemoveLines { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="LineDetector"/> should locate check boxes.
        /// </summary>
        /// <value>
        /// <b>true</b> to locate check boxes; otherwise, <b>false</b>.
        /// </value>
        private bool FindCheckboxes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the found check boxes should be removed from the image.
        /// </summary>
        /// <value>
        /// <b>true</b> to remove found check boxes from the image; otherwise, <b>false</b>.
        /// </value>
        private bool RemoveCheckboxes { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum check box size.
        /// </summary>
        /// <value>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </value>
        private int MinBoxSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the minimum check box size.
        /// </summary>
        /// <value>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </value>
        private int MaxBoxSize { get; set; } = 50;

        /// <summary>
        /// Finds the horizontal and vertical lines on the <see cref="Image"/>.
        /// The type of lines to find is determined by the class parameters.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="LineDetector"/> that operation should be canceled.</param>
        /// <returns>
        /// The detected lines.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not one.
        /// </exception>
        /// <remarks>
        /// <para>This method works with binary (1bpp) images only.</para>
        /// </remarks>
        public ISet<Shape> FindLines(Image image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ISet<CheckboxShape> checkboxResult = null;
            ISet<LineShape> hlinesResult = null;
            ISet<LineShape> vlinesResult = null;

            // close up small holes
            int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
            Image closedImage = image.MorphClose(null, StructuringElement.Square(maxLineWidth / 3), 1, BorderType.BorderConst, image.WhiteColor);

            // open up to detect big solid areas
            Image openedImage = closedImage.MorphOpen(null, StructuringElement.Square(maxLineWidth), 1, BorderType.BorderConst, image.WhiteColor);
            Image hollowImage = closedImage.Sub(null, openedImage, 0);

            if (this.FindCheckboxes)
            {
                checkboxResult = this.FindAndRemoveCheckboxes(hollowImage, cancellationToken);
            }

            // masks we would like to find
            Image hlines = null;
            Image vlines = null;
            Image nonVLines = null;
            Image nonHLines = null;
            Image itersections = null;
            FindLines();

            // remove the lines
            if (vlines != null && this.RemoveLines)
            {
                RemoveLines(vlines, nonVLines);
            }

            if (hlines != null)
            {
                // recompute intersections
                itersections = vlines?.And(null, hlines);

                // re-filter lines
                hlinesResult = this.FilterHorizontalLines(hlinesResult, hlines, nonHLines, itersections);

                if (this.RemoveLines)
                {
                    RemoveLines(hlines, nonHLines);
                }
            }

            RemoveIntersections();

            // create answer
            HashSet<Shape> answer = new HashSet<Shape>();
            answer.UnionWith(hlinesResult);
            answer.UnionWith(vlinesResult);
            if (checkboxResult != null)
            {
                answer.UnionWith(checkboxResult);
            }

            return answer;

            void FindLines()
            {
                // open up in both directions to find lines
                int minHorLineLength = LineDetector.MinHorLineLength.MulDiv(image.HorizontalResolution, 200);
                hlines = hollowImage.MorphOpen(null, StructuringElement.Brick(minHorLineLength, 1), 1, BorderType.BorderConst, image.WhiteColor);
                if (hlines.IsAllWhite())
                {
                    hlines = null;
                }
                else
                {
                    hlinesResult = this.FilterHorizontalLines(hlines);
                    if (hlinesResult.Count == 0)
                    {
                        hlines = null;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                int minVerLineLength = LineDetector.MinVerLineLength.MulDiv(image.HorizontalResolution, 200);
                vlines = hollowImage.MorphOpen(null, StructuringElement.Brick(1, minVerLineLength), 1, BorderType.BorderConst, image.WhiteColor);
                if (vlines.IsAllWhite())
                {
                    vlines = null;
                }
                else
                {
                    vlinesResult = this.FilterVerticalLines(vlines);
                    if (vlinesResult.Count == 0)
                    {
                        vlines = null;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                Image nonLines = null;

                // vertical lines
                if (vlines != null)
                {
                    nonLines = image.Sub(nonLines, vlines, 0);
                    if (hlines != null)
                    {
                        nonLines -= hlines;
                        itersections = hlines.And(itersections, vlines);
                    }

                    int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonVLines = nonLines.Erode(null, StructuringElement.Brick(maxLineResidue, 1), 1, BorderType.BorderConst, image.WhiteColor);
                    nonVLines.FloodFill(nonVLines, 8, nonLines);

                    if (hlines != null)
                    {
                        nonVLines += hlines;
                        nonVLines -= itersections;
                    }

                    vlinesResult = this.FilterVerticalLines(vlinesResult, vlines, nonVLines, itersections);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                // horizontal lines
                if (hlines != null)
                {
                    nonLines = image.Sub(nonLines, hlines, 0);
                    if (vlines != null)
                    {
                        nonLines -= vlines;
                        itersections = hlines.And(itersections, vlines);
                    }

                    int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    nonHLines = nonLines.Erode(null, StructuringElement.Brick(1, maxLineResidue), 1, BorderType.BorderConst, image.WhiteColor);
                    nonHLines.FloodFill(nonHLines, 8, nonLines);

                    if (vlines != null)
                    {
                        nonHLines += vlines;
                        nonHLines -= itersections;
                    }

                    hlinesResult = this.FilterHorizontalLines(hlinesResult, hlines, nonHLines, itersections);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            void RemoveLines(Image lines, Image nonLines)
            {
                if (this.RemoveLines)
                {
                    // remove the lines
                    image.Xand(image, lines);

                    // dilate the lines so they touch the residue
                    // then flood fill then to get all the residue (image less non-lines)
                    Image fatLines = lines.Dilate3x3(null, BorderType.BorderConst, image.WhiteColor);

                    fatLines.FloodFill(fatLines, 8, image.Xand(null, nonLines));

                    // remove the residue
                    image.Xand(image, fatLines);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            void RemoveIntersections()
            {
                if (this.RemoveLines && hlines != null && vlines != null)
                {
                    // get the intersection residue
                    Image residue = hlines
                        .And(null, vlines)
                        .Dilate(null, StructuringElement.Square(5), 1, BorderType.BorderConst, image.WhiteColor);

                    residue.FloodFill(residue, 8, image);

                    // remove the residue
                    image.Xand(image, residue);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        private ISet<CheckboxShape> FindAndRemoveCheckboxes(Image image, CancellationToken cancellationToken)
        {
            HashSet<CheckboxShape> result = new HashSet<CheckboxShape>(32);

            // compute image-dependent parameters
            int minBoxSizeH = this.MinBoxSize.MulDiv(image.HorizontalResolution, 200);
            int maxBoxSizeH = this.MaxBoxSize.MulDiv(image.HorizontalResolution, 200);
            int minBoxSizeV = this.MinBoxSize.MulDiv(image.VerticalResolution, 200);
            int maxBoxSizeV = this.MaxBoxSize.MulDiv(image.VerticalResolution, 200);

            // create a draft that would show found checkboxes
#if DEBUG
            Image draft = image.Clone(false);
#endif

            // keep track of tested horizontal components that did not yield results
            HashSet<Rectangle> testedHBounds = new HashSet<Rectangle>();

            // the algorith proceeds in two steps
            // first, we find a pair of parallel horizontal lines that have similar length and horizontal position
            // second, we find a pair of parallel vertical lines that would connect horizontal lines on both sides to form a box
            BoundedObjectGrid<ConnectedComponent> hgrid = FindHorizontalLines();
            if (hgrid != null)
            {
                BoundedObjectGrid<ConnectedComponent> vgrid = FindVerticalLines();
                if (vgrid != null)
                {
                    foreach (ConnectedComponent hcomp1 in hgrid.EnumObjects())
                    {
                        if (hcomp1.HorizontalAlignment == HorizontalAlignment.None)
                        {
                            Rectangle hbounds1 = hcomp1.Bounds;
                            int hdelta = hbounds1.Width / 5;

                            foreach (ConnectedComponent hcomp2 in hgrid.EnumObjects(Rectangle.Inflate(hbounds1, 0, hbounds1.Width.MulDiv(3, 2))))
                            {
                                if (hcomp2 != hcomp1)
                                {
                                    if (hcomp2.HorizontalAlignment == HorizontalAlignment.None)
                                    {
                                        Rectangle hbounds2 = hcomp2.Bounds;
                                        Rectangle hbounds = Rectangle.Union(hbounds1, hbounds2);
                                        hdelta = hbounds.Width / 5;

                                        if (!testedHBounds.Contains(hbounds))
                                        {
                                            testedHBounds.Add(hbounds);

                                            if (TestHorizontalComponents(hbounds1, hbounds2, hdelta))
                                            {
                                                // after we found a pair of matching horizontal lines
                                                // start looking for a pair of vertical lines that connect them
                                                ConnectedComponent vcomp1 = null;
                                                ConnectedComponent vcomp2 = null;
                                                foreach (ConnectedComponent vcomp in vgrid.EnumObjects(Rectangle.Inflate(hbounds, hdelta, 0)))
                                                {
                                                    if (vcomp.VerticalAlignment == VerticalAlignment.None)
                                                    {
                                                        Rectangle vbounds = vcomp.Bounds;

                                                        if (TestVerticalComponent(hbounds, vbounds, hdelta))
                                                        {
                                                            if (vbounds.Left.AreEqual(hbounds.Left, hdelta))
                                                            {
                                                                vcomp1 = vcomp;
                                                            }
                                                            else if (vbounds.Right.AreEqual(hbounds.Right, hdelta))
                                                            {
                                                                vcomp2 = vcomp;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (vcomp1 != null && vcomp2 != null)
                                                {
                                                    Rectangle vunion = Rectangle.Union(vcomp1.Bounds, vcomp2.Bounds);
                                                    result.Add(new CheckboxShape(Rectangle.Union(hbounds, vunion)));
#if DEBUG
                                                    draft.AddConnectedComponent(hcomp1);
                                                    draft.AddConnectedComponent(hcomp2);
                                                    draft.AddConnectedComponent(vcomp1);
                                                    draft.AddConnectedComponent(vcomp2);
#endif

                                                    // mark used components, so we do not test them twice
                                                    hcomp1.HorizontalAlignment = HorizontalAlignment.Left;
                                                    hcomp2.HorizontalAlignment = HorizontalAlignment.Left;
                                                    vcomp1.VerticalAlignment = VerticalAlignment.Top;
                                                    vcomp2.VerticalAlignment = VerticalAlignment.Top;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // delete check boxes from the image
            if (this.RemoveCheckboxes)
            {
                /*foreach (CheckboxShape shape in result)
                {
                    image.SetWhite(Rectangle.Inflate(shape.Bounds, 1, 1));
                }*/
#if DEBUG
                image.Sub(image, draft, 0);
#endif
            }

            return result;

            BoundedObjectGrid<ConnectedComponent> FindHorizontalLines()
            {
                Image lines = image.MorphOpen(null, StructuringElement.Brick(minBoxSizeH, 1), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);

                cancellationToken.ThrowIfCancellationRequested();

                BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                    lines.Bounds,
                    (lines.Width / 10).Clip(1, lines.Width),
                    (lines.Height / 20).Clip(1, lines.Height));
                grid.AddRange(comps.Where(x => x.Bounds.Width <= maxBoxSizeH), true, true);

                cancellationToken.ThrowIfCancellationRequested();

                return grid;
            }

            BoundedObjectGrid<ConnectedComponent> FindVerticalLines()
            {
                Image lines = image.MorphOpen(null, StructuringElement.Brick(1, minBoxSizeV), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);

                cancellationToken.ThrowIfCancellationRequested();

                BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                    lines.Bounds,
                    (lines.Width / 10).Clip(1, lines.Width),
                    (lines.Height / 20).Clip(1, lines.Height));
                grid.AddRange(comps.Where(x => x.Bounds.Height <= maxBoxSizeV), true, true);

                return grid;
            }

            bool TestHorizontalComponents(Rectangle bounds1, Rectangle bounds2, int delta)
            {
                int dist = Math.Abs(bounds1.CenterY - bounds2.CenterY);
                return dist.Between(minBoxSizeV, maxBoxSizeV) &&
                    (dist.AreEqual(bounds1.Width, delta) || dist.AreEqual(bounds2.Width, delta)) &&
                    bounds2.X.AreEqual(bounds1.X, delta) &&
                    bounds2.Right.AreEqual(bounds1.Right, delta);
            }

            bool TestVerticalComponent(Rectangle hbounds, Rectangle vbounds, int delta)
            {
                return vbounds.Height.AreEqual(hbounds.Height, delta) &&
                    vbounds.Top.AreEqual(hbounds.Top, delta) &&
                    vbounds.Bottom.AreEqual(hbounds.Bottom, delta);
            }
        }

        private ISet<LineShape> FilterVerticalLines(Image vlines)
        {
            HashSet<LineShape> result = new HashSet<LineShape>();

            int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(vlines.HorizontalResolution, 200);
            int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(vlines.VerticalResolution, 200);

            foreach (ConnectedComponent component in vlines.FindConnectedComponents(8))
            {
                Rectangle bounds = component.Bounds;
                int maxWidth = component.MaxWidth();

                if (bounds.Width.Between(minThickLineWidth, minThickLineLength - 1) &&
                   bounds.Height.Between(minThickLineWidth, minThickLineLength - 1) &&
                   maxWidth > minThickLineLength)
                {
                    // too thick for the length
                    vlines.SetWhite(bounds);
                    continue;
                }

                Point begin = new Point(component.GetLine(bounds.Y).Center, bounds.Y);
                Point end = new Point(component.GetLine(bounds.Bottom - 1).Center, bounds.Bottom - 1);
                LineShape line = new LineShape(bounds, begin, end, maxWidth, LineTypes.Vertical);
                result.Add(line);
            }

            return result;
        }

        private ISet<LineShape> FilterVerticalLines(ISet<LineShape> lines, Image vlines, Image nonVLines, Image itersections)
        {
            HashSet<LineShape> result = new HashSet<LineShape>();

            // find connected components and do preliminary filtering
            AlignedObjectGrid<LineShape> grid = new AlignedObjectGrid<LineShape>(
                vlines.Bounds,
                200.MulDiv(vlines.HorizontalResolution, 200),
                100.MulDiv(vlines.VerticalResolution, 200),
                RectangleTBLRComparer.Default);
            grid.AddRange(lines, true, true);

            // find line vectors
            int maxGap = LineDetector.MaxLineGap.MulDiv(vlines.VerticalResolution, 200);
            foreach (LineShape line in lines)
            {
                if (line.HorizontalAlignment == HorizontalAlignment.None)
                {
                    IList<LineShape> alignedLines = grid.FindHorizontalAlignment(line, HorizontalAlignment.Center, Math.Min(maxGap, line.Bounds.Height), 3);
                    if (alignedLines.Count > 0)
                    {
                        LineShape newline = new LineShape(
                            Rectangle.Union(alignedLines.Select(x => x.Bounds)),
                            alignedLines[0].Begin,
                            alignedLines[alignedLines.Count - 1].End,
                            alignedLines.Max(x => x.Width),
                            LineTypes.Vertical);

                        if (IsLineGood(newline))
                        {
                            result.Add(newline);
                        }
                        else
                        {
                            for (int i = 0, ii = alignedLines.Count; i < ii; i++)
                            {
                                vlines.SetWhite(alignedLines[i].Bounds);
                            }
                        }
                    }
                }
            }

            // process remaining lines
            foreach (LineShape line in lines)
            {
                if (line.HorizontalAlignment == HorizontalAlignment.None)
                {
                    if (IsLineGood(line))
                    {
                        result.Add(line);
                    }
                    else
                    {
                        vlines.SetWhite(line.Bounds);
                    }
                }
            }

            Image draft = vlines.Clone(false);
            foreach (LineShape line in result)
            {
                draft.SetBlack(line.Bounds);
            }

            return result;

            ulong CountAdjacentPixels(int lineWidth, Rectangle bounds)
            {
                bounds.Inflate(lineWidth, 0);
                bounds.Intersect(nonVLines.Bounds);

                return nonVLines.Power(bounds);
            }

            int CountIntersections(Rectangle bounds)
            {
                return itersections?.FindConnectedComponents(8, bounds)?.Count ?? 0;
            }

            bool IsLineGood(LineShape line)
            {
                Rectangle bounds = line.Bounds;

                // check length
                int minLineLength = (this.MinVerticalLineLength * vlines.VerticalResolution).Round();
                if (bounds.Height < minLineLength && !VerticalLineHasIntersectionsOnBothEnds(bounds))
                {
                    return false;
                }

                // Test density near the line if there are not enough intersections
                if (CountIntersections(line.Bounds) < 2)
                {
                    ulong count = CountAdjacentPixels(line.Width, bounds);
                    if (count > bounds.Area * LineDetector.MaxNonLineDensity)
                    {
                        return false;
                    }
                }

                return true;
            }

            bool VerticalLineHasIntersectionsOnBothEnds(Rectangle bounds)
            {
                int boxheight = Math.Min(bounds.Width, bounds.Height);
                return itersections != null &&
                    !itersections.IsAllWhite(new Rectangle(bounds.X, bounds.Y, bounds.Width, boxheight)) &&
                    !itersections.IsAllWhite(new Rectangle(bounds.X, bounds.Bottom - boxheight, bounds.Width, boxheight));
            }
        }

        private ISet<LineShape> FilterHorizontalLines(Image hlines)
        {
            HashSet<LineShape> result = new HashSet<LineShape>();

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

            int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(hlines.VerticalResolution, 200);
            int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(hlines.HorizontalResolution, 200);

            foreach (ConnectedComponent component in hlines.FindConnectedComponents(8))
            {
                Rectangle bounds = component.Bounds;

                // calculate maximum component width as twice the maximum distance between its points and the background
                int maxWidth = (int)component.ToImage().DistanceToBackground(4, 8).Max() * 2;
                maxWidth = Math.Min(maxWidth, bounds.Height);

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

                if (bounds.Width.Between(minThickLineWidth, minThickLineLength - 1) &&
                   bounds.Height.Between(minThickLineWidth, minThickLineLength - 1) &&
                   maxWidth > minThickLineLength)
                {
                    // too thick for the length
                    hlines.SetWhite(bounds);
                    continue;
                }

                LineShape newline = new LineShape(bounds, maxWidth, LineTypes.Horizontal);
                result.Add(newline);
            }

            return result;
        }

        private ISet<LineShape> FilterHorizontalLines(ISet<LineShape> lines, Image hlines, Image nonHLines, Image itersections)
        {
            HashSet<LineShape> result = new HashSet<LineShape>();

            // find connected components and do preliminary filtering
            AlignedObjectGrid<LineShape> grid = new AlignedObjectGrid<LineShape>(
                hlines.Bounds,
                200.MulDiv(hlines.HorizontalResolution, 200),
                100.MulDiv(hlines.VerticalResolution, 200),
                RectangleLTRBComparer.Default);
            grid.AddRange(lines, true, true);

            int maxGap = LineDetector.MaxLineGap.MulDiv(hlines.HorizontalResolution, 200);
            foreach (LineShape line in lines)
            {
                if (line.VerticalAlignment == VerticalAlignment.None)
                {
                    IList<LineShape> alignedLines = grid.FindVerticalAlignment(line, VerticalAlignment.Center, Math.Min(maxGap, line.Bounds.Width), 3);
                    if (alignedLines.Count > 0)
                    {
                        LineShape newline = new LineShape(
                            Rectangle.Union(alignedLines.Select(x => x.Bounds)),
                            alignedLines[0].Begin,
                            alignedLines[alignedLines.Count - 1].End,
                            alignedLines.Max(x => x.Width),
                            LineTypes.Horizontal);

                        if (IsLineGood(newline))
                        {
                            result.Add(newline);
                        }
                        else
                        {
                            for (int i = 0, ii = alignedLines.Count; i < ii; i++)
                            {
                                hlines.SetWhite(alignedLines[i].Bounds);
                            }
                        }
                    }
                }
            }

            // process remaining lines
            foreach (LineShape line in lines)
            {
                if (line.VerticalAlignment == VerticalAlignment.None)
                {
                    if (IsLineGood(line))
                    {
                        result.Add(line);
                    }
                    else
                    {
                        hlines.SetWhite(line.Bounds);
                    }
                }
            }

            Image draft = hlines.Clone(false);
            foreach (LineShape line in result)
            {
                draft.SetBlack(line.Bounds);
            }

            return result;

            ulong CountAdjacentPixels(int lineWidth, Rectangle bounds)
            {
                bounds.Inflate(0, lineWidth);
                bounds.Intersect(nonHLines.Bounds);

                return nonHLines.Power(bounds);
            }

            int CountIntersections(Rectangle bounds)
            {
                return itersections?.FindConnectedComponents(8, bounds)?.Count ?? 0;
            }

            bool IsLineGood(LineShape line)
            {
                Rectangle bounds = line.Bounds;

                // check length
                int minLineLength = (this.MinHorizontalLineLength * hlines.HorizontalResolution).Round();
                if (bounds.Width < minLineLength)
                {
                    return false;
                }

                // Test density near the line if there are not enough intersections
                if (CountIntersections(line.Bounds) < 2)
                {
                    ulong count = CountAdjacentPixels(line.Width, bounds);
                    if (count > bounds.Area * LineDetector.MaxNonLineDensity)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class Line : IAlignedBoundedObject
        {
            private readonly List<ConnectedComponent> components = new List<ConnectedComponent>();

            public IList<ConnectedComponent> Components => this.components;

            public int Width { get; set; }

            public bool IsVertical { get; set; }

            public HorizontalAlignment HorizontalAlignment { get; set; }

            public VerticalAlignment VerticalAlignment { get; set; }

            public Rectangle Bounds => Rectangle.Union(this.components.Select(x => x.Bounds));
        }
    }
}