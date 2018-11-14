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
        private bool FindCheckboxes { get; set; } = true;

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
        private int MinBoxSize { get; set; } = 15;

        /// <summary>
        /// Gets or sets the minimum check box size.
        /// </summary>
        /// <value>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </value>
        private int MaxBoxSize { get; set; } = 60;

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

            // find check boxes
            ISet<CheckboxShape> checkboxResult = null;
            BoundedObjectGrid<CheckboxShape> checkboxGrid = null;
            if (this.FindCheckboxes)
            {
                checkboxResult = this.FindBoxes(image, cancellationToken);

                checkboxGrid = new BoundedObjectGrid<CheckboxShape>(image.Bounds, 5, 10);
                checkboxGrid.AddRange(checkboxResult, true, true);
            }

            HashSet<LineShape> hlinesResult = null;
            HashSet<LineShape> vlinesResult = null;

            if (this.LineTypes != LineTypes.None)
            {
                // masks we would like to find
                Image hlines = null;
                Image vlines = null;
                FindLines();

                if (hlines != null || vlines != null)
                {
                    // filter out false positives
                    FilterFalsePositives(out Image nonHLines, out Image nonVLines);

                    if (this.RemoveLines)
                    {
                        if (hlines != null)
                        {
                            RemoveLines(hlines, nonHLines);
                        }

                        if (vlines != null)
                        {
                            RemoveLines(vlines, nonVLines);
                        }

                        if (hlines != null && vlines != null)
                        {
                            RemoveIntersections();
                        }
                    }
                }

                void FindLines()
                {
                    // close up small holes
                    int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
                    Image closedImage = image.MorphClose(null, StructuringElement.Square(maxLineWidth / 3), 1, BorderType.BorderConst, image.WhiteColor);

                    // open up to detect big solid areas
                    Image openedImage = closedImage.MorphOpen(null, StructuringElement.Square(maxLineWidth), 1, BorderType.BorderConst, image.WhiteColor);
                    Image hollowImage = closedImage.Sub(null, openedImage, 0);

                    // open up in both directions to find lines
                    int minHorLineLength = LineDetector.MinHorLineLength.MulDiv(image.HorizontalResolution, 200);
                    hlines = hollowImage.MorphOpen(null, StructuringElement.Brick(minHorLineLength, 1), 1, BorderType.BorderConst, image.WhiteColor);
                    if (hlines.IsAllWhite())
                    {
                        hlines = null;
                    }
                    else
                    {
                        hlinesResult = this.FilterHorizontalLines(hlines, checkboxGrid);
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
                        vlinesResult = this.FilterVerticalLines(vlines, hlines, checkboxGrid);
                        if (vlinesResult.Count == 0)
                        {
                            vlines = null;
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                void FilterFalsePositives(out Image nonHLines, out Image nonVLines)
                {
                    nonHLines = null;
                    nonVLines = null;

                    Image nonLines = null;
                    if (hlines != null)
                    {
                        nonLines = image.Sub(nonLines, hlines, 0);
                    }

                    if (vlines != null)
                    {
                        nonLines = image.Sub(nonLines, vlines, 0);
                    }

                    Image intersections = vlines != null && hlines != null ? hlines & vlines : null;

                    int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    if (hlines != null)
                    {
                        nonHLines = nonLines.Erode(null, StructuringElement.Brick(1, maxLineResidue), 1, BorderType.BorderConst, image.WhiteColor);
                        nonHLines.FloodFill(nonHLines, 8, nonLines);

                        if (vlines != null)
                        {
                            nonHLines += vlines;
                            nonHLines -= intersections;
                        }
                    }

                    ////int maxLineResidue = 6; //// LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                    if (vlines != null)
                    {
                        nonVLines = nonLines.Erode(null, StructuringElement.Brick(maxLineResidue, 1), 1, BorderType.BorderConst, image.WhiteColor);
                        nonVLines.FloodFill(nonVLines, 8, nonLines);

                        if (hlines != null)
                        {
                            nonVLines += hlines;
                            nonVLines -= intersections;
                        }
                    }

                    int removedCount = 0;
                    do
                    {
                        removedCount = 0;

                        // horizontal lines
                        if (hlines != null)
                        {
                            int count = this.RemoveHorizontalLinesFalsePositives(hlinesResult, hlines, nonHLines, intersections);
                            if (count > 0)
                            {
                                removedCount += count;
                                if (vlines != null)
                                {
                                    // recompute intersections
                                    intersections = hlines.And(intersections, vlines);
                                }

                                if (hlinesResult.Count == 0)
                                {
                                    hlines = null;
                                }
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        // vertical lines
                        if (vlines != null)
                        {
                            int count = this.RemoveVerticalLinesFalsePositives(vlinesResult, vlines, nonVLines, intersections);
                            if (count > 0)
                            {
                                removedCount += count;
                                if (hlines != null)
                                {
                                    // recompute intersections
                                    intersections = hlines.And(intersections, vlines);
                                }

                                if (vlinesResult.Count == 0)
                                {
                                    vlines = null;
                                }
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    while (removedCount != 0);
                }

                void RemoveLines(Image lines, Image nonLines)
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

                void RemoveIntersections()
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

            // delete check boxes from the image
            if (this.RemoveCheckboxes && checkboxResult != null)
            {
                foreach (CheckboxShape shape in checkboxResult)
                {
                    image.SetWhite(Rectangle.Inflate(shape.Bounds, 1, 1));
                }
            }

            // create a draft that would show found check boxes
#if DEBUG
            Image draft = image.ConvertTo(null, 32);

            if (checkboxResult != null)
            {
                foreach (CheckboxShape shape in checkboxResult)
                {
                    draft.DrawRectangle(shape.Bounds, Color.Green);
                }
            }

            if (hlinesResult != null)
            {
                foreach (LineShape shape in hlinesResult)
                {
                    draft.DrawRectangle(shape.Bounds, Color.Red);
                }
            }

            if (vlinesResult != null)
            {
                foreach (LineShape shape in vlinesResult)
                {
                    draft.DrawRectangle(shape.Bounds, Color.Red);
                }
            }
#endif

            // create answer
            HashSet<Shape> answer = new HashSet<Shape>();
            if (hlinesResult != null)
            {
                answer.UnionWith(hlinesResult);
            }

            if (vlinesResult != null)
            {
                answer.UnionWith(vlinesResult);
            }

            if (checkboxResult != null)
            {
                answer.UnionWith(checkboxResult);
            }

            return answer;
        }

        private ISet<CheckboxShape> FindBoxes(Image image, CancellationToken cancellationToken)
        {
            HashSet<CheckboxShape> result = new HashSet<CheckboxShape>(32);

            // compute image-dependent parameters
            int minBoxSizeH = this.MinBoxSize.MulDiv(image.HorizontalResolution, 200);
            int maxBoxSizeH = this.MaxBoxSize.MulDiv(image.HorizontalResolution, 200);
            int minBoxSizeV = this.MinBoxSize.MulDiv(image.VerticalResolution, 200);
            int maxBoxSizeV = this.MaxBoxSize.MulDiv(image.VerticalResolution, 200);

            // keep track of tested horizontal components that did not yield results
            ////HashSet<Rectangle> testedHBounds = new HashSet<Rectangle>();
            HashSet<Rectangle> testedVBounds = new HashSet<Rectangle>();

            // the algorithm proceeds in two steps
            // first, we find a pair of parallel horizontal lines that have similar length and horizontal position
            // second, we find a pair of parallel vertical lines that would connect horizontal lines on both sides to form a box
            IList<ConnectedComponent> hlines = FindHorizontalLines();
            if (hlines != null && hlines.Count > 0)
            {
                IList<ConnectedComponent> vlines = FindVerticalLines();
                if (vlines != null && vlines.Count > 0)
                {
                    BoundedObjectGrid<ConnectedComponent> hgrid = CreateGrid(hlines);
                    BoundedObjectGrid<ConnectedComponent> vgrid = CreateGrid(vlines);
                    foreach (ConnectedComponent vcomp1 in vlines.Where(x => x.Bounds.Height <= maxBoxSizeV))
                    {
                        if (vcomp1.HorizontalAlignment == HorizontalAlignment.None)
                        {
                            Rectangle vbounds1 = vcomp1.Bounds;
                            int vdelta = vbounds1.Height / 5;

                            foreach (ConnectedComponent vcomp2 in vgrid.EnumObjects(Rectangle.Inflate(vbounds1, vbounds1.Height.MulDiv(2, 1), 0)))
                            {
                                if (vcomp2 != vcomp1 && vcomp2.HorizontalAlignment == HorizontalAlignment.None)
                                {
                                    // test second vertical component
                                    if (!TestVerticalComponents(vbounds1, vcomp2, out Rectangle vbounds2, vdelta, out bool longVLine))
                                    {
                                        continue;
                                    }

                                    Rectangle vbounds = Rectangle.Union(vbounds1, vbounds2);
                                    vdelta = MinMax.Max(vbounds.Width, vbounds.Height) / 5;

                                    if (testedVBounds.Contains(vbounds))
                                    {
                                        continue;
                                    }

                                    testedVBounds.Add(vbounds);

                                    // after we found a pair of matching horizontal lines
                                    // start looking for a pair of vertical lines that connect them
                                    ConnectedComponent hcompTop = null;
                                    int topEst = 0;
                                    ConnectedComponent hcompBottom = null;
                                    int bottomEst = 0;
                                    Rectangle hboundsBottom = Rectangle.Empty;
                                    bool longHLine = false;
                                    foreach (ConnectedComponent hcomp in hgrid.EnumObjects(Rectangle.Inflate(vbounds, 0, vdelta)))
                                    {
                                        if (hcomp.VerticalAlignment == VerticalAlignment.None)
                                        {
                                            if (TestTopComponent(vbounds, hcomp.Bounds, vdelta, out int est))
                                            {
                                                if (hcompTop == null || est < topEst)
                                                {
                                                    hcompTop = hcomp;
                                                    topEst = est;
                                                }
                                            }

                                            if (TestBottomComponent(vbounds, hcomp, out Rectangle hbounds, vdelta, out bool longLine, out est))
                                            {
                                                if (hcompBottom == null || est < bottomEst || (longHLine && !longLine))
                                                {
                                                    hcompBottom = hcomp;
                                                    bottomEst = est;
                                                    hboundsBottom = hbounds;
                                                    longHLine = longLine;
                                                }
                                            }
                                        }
                                    }

                                    if (hcompTop != null && hcompBottom != null)
                                    {
                                        Rectangle bounds = Rectangle.FromLTRB(vbounds.X, hcompTop.Bounds.Y, vbounds.Right, hboundsBottom.Bottom);
                                        result.Add(new CheckboxShape(bounds));

                                        // mark used components, so we do not test them twice
                                        hcompTop.VerticalAlignment = VerticalAlignment.Top;
                                        if (!longHLine)
                                        {
                                            hcompBottom.VerticalAlignment = VerticalAlignment.Bottom;
                                        }

                                        vcomp1.HorizontalAlignment = vbounds1.X < vbounds2.X ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                                        if (!longVLine)
                                        {
                                            vcomp2.HorizontalAlignment = vbounds2.X < vbounds1.X ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;

            IList<ConnectedComponent> FindVerticalLines()
            {
                int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);

                Image lines = image.MorphOpen(null, StructuringElement.Brick(1, minBoxSizeV), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);

                cancellationToken.ThrowIfCancellationRequested();

                return comps.Where(x => x.MaxWidth() <= maxLineWidth).ToArray();
            }

            IList<ConnectedComponent> FindHorizontalLines()
            {
                int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.VerticalResolution, 200);

                Image lines = image.MorphOpen(null, StructuringElement.Brick(minBoxSizeH, 1), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);

                cancellationToken.ThrowIfCancellationRequested();

                return comps.Where(x => x.MaxHeight() <= maxLineWidth).ToArray();
            }

            BoundedObjectGrid<ConnectedComponent> CreateGrid(IEnumerable<ConnectedComponent> comps)
            {
                BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(image.Bounds, 10, 20);
                grid.AddRange(comps, true, true);
                return grid;
            }

            bool TestVerticalComponents(Rectangle bounds1, ConnectedComponent comp, out Rectangle bounds2, int delta, out bool longLine)
            {
                longLine = false;
                bounds2 = comp.Bounds;

                if (bounds2.Height > maxBoxSizeV)
                {
                    // if touching long line, get the part of it that intersects with the box
                    bounds2 = comp.Intersect(Rectangle.FromLTRB(bounds2.X, bounds1.Y, bounds2.Right, bounds1.Bottom));
                    longLine = true;
                }

                int dist = Math.Abs(bounds1.CenterX - bounds2.CenterX);
                if (!dist.Between(minBoxSizeH, maxBoxSizeH))
                {
                    // the distance between lines is invalid
                    return false;
                }

                // both lines are isolated - check for squareness of the box
                return dist.Between(MinMax.Min(bounds1.Height, bounds2.Height) - delta, (2 * MinMax.Max(bounds1.Height, bounds2.Height)) + delta) &&
                    bounds2.Y.AreEqual(bounds1.Y, delta) &&
                    bounds2.Bottom.AreEqual(bounds1.Bottom, delta);
            }

            bool TestTopComponent(Rectangle vbounds, Rectangle hbounds, int delta, out int est)
            {
                int est1 = Math.Abs(hbounds.Top - vbounds.Top);
                int est2 = Math.Abs(hbounds.X - vbounds.X);
                int est3 = Math.Abs(hbounds.Right - vbounds.Right);
                est = est1 + est2 + est3;

                return est1 <= delta && est2 <= delta && est3 <= delta;
            }

            bool TestBottomComponent(Rectangle vbounds, ConnectedComponent hcomp, out Rectangle hbounds, int delta, out bool longLine, out int est)
            {
                longLine = false;
                hbounds = hcomp.Bounds;

                if (hbounds.Width > maxBoxSizeH)
                {
                    // if touching long line, get the part of it that intersects with the box
                    hbounds = hcomp.Intersect(Rectangle.FromLTRB(vbounds.X, hbounds.Y, vbounds.Right, hbounds.Bottom));
                    longLine = true;
                }

                int est1 = Math.Abs(hbounds.Bottom - vbounds.Bottom);
                int est2 = Math.Abs(hbounds.X - vbounds.X);
                int est3 = Math.Abs(hbounds.Right - vbounds.Right);
                est = est1 + est2 + est3;

                return est1 <= delta && est2 <= delta && est3 <= delta;
            }
        }

        private HashSet<LineShape> FilterVerticalLines(Image vlines, Image hlines, BoundedObjectGrid<CheckboxShape> checkboxGrid)
        {
            HashSet<LineShape> lines = new HashSet<LineShape>();

            int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(vlines.HorizontalResolution, 200);
            int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(vlines.VerticalResolution, 200);
            int maxBoxSize = this.MaxBoxSize.MulDiv(vlines.VerticalResolution, 200);

            // find connected components and do preliminary filtering
            foreach (ConnectedComponent component in vlines.FindConnectedComponents(8))
            {
                Rectangle bounds = component.Bounds;
                int maxWidth = component.MaxWidth();

                bool isBad = false;

                if (bounds.Width.Between(minThickLineWidth, minThickLineLength - 1) &&
                   bounds.Height.Between(minThickLineWidth, minThickLineLength - 1) &&
                   maxWidth > minThickLineLength)
                {
                    // too thick for the length
                    isBad = true;
                }

                if (!isBad && bounds.Height <= maxBoxSize && checkboxGrid != null && checkboxGrid.EnumObjects(bounds).Any())
                {
                    // is part of the check box
                    isBad = true;
                }

                if (isBad)
                {
                    vlines.SetWhite(bounds);
                }
                else
                {
                    Point begin = new Point(component.GetLine(bounds.Y).Center, bounds.Y);
                    Point end = new Point(component.GetLine(bounds.Bottom - 1).Center, bounds.Bottom - 1);
                    LineShape line = new LineShape(bounds, begin, end, maxWidth, LineTypes.Vertical);
                    lines.Add(line);
                }
            }

            // find line vectors
            AlignedObjectGrid<LineShape> grid = new AlignedObjectGrid<LineShape>(vlines.Bounds, 10, 20, RectangleTBLRComparer.Default);
            grid.AddRange(lines, true, true);

            int minLineLength = (this.MinVerticalLineLength * vlines.VerticalResolution).Round();
            int maxGap = LineDetector.MaxLineGap.MulDiv(vlines.VerticalResolution, 200);
            int tolerance = 6.MulDiv(vlines.HorizontalResolution, 200);

            HashSet<LineShape> newlines = new HashSet<LineShape>();

            foreach (LineShape line in lines)
            {
                if (line.HorizontalAlignment == HorizontalAlignment.None)
                {
                    IList<LineShape> alignedLines = grid.FindHorizontalAlignment(
                        line,
                        HorizontalAlignment.Center,
                        Math.Min(maxGap, line.Bounds.Height),
                        tolerance,
                        (result, bounds) => (bounds.Height >= minLineLength && (result.Count >= 3 || result.Any(o => o.Bounds.Height >= minLineLength))) || LineHasIntersectionsOnBothEnds(bounds));

                    if (alignedLines.Count > 0)
                    {
                        LineShape newline = new LineShape(
                            Rectangle.Union(alignedLines.Select(x => x.Bounds)),
                            alignedLines[0].Begin,
                            alignedLines[alignedLines.Count - 1].End,
                            alignedLines.Max(x => x.Width),
                            LineTypes.Vertical);

                        newlines.Add(newline);
                    }
                }
            }

            // remove merged and short lines
            lines.RemoveWhere(x =>
            {
                // remove merged lines
                bool remove = x.HorizontalAlignment != HorizontalAlignment.None;

                // remove small not merged lines
                // also delete them from the image
                if (x.HorizontalAlignment == HorizontalAlignment.None && x.Bounds.Height < minLineLength && !LineHasIntersectionsOnBothEnds(x.Bounds))
                {
                    vlines.SetWhite(x.Bounds);
                    remove = true;
                }

                return remove;
            });

            lines.UnionWith(newlines);

            bool LineHasIntersectionsOnBothEnds(Rectangle bounds)
            {
                if (hlines != null)
                {
                    int boxheight = Math.Min(bounds.Width, bounds.Height);

                    Rectangle top = new Rectangle(bounds.X, bounds.Y, bounds.Width, boxheight);
                    top.Inflate(0, 2 * boxheight);
                    top.Intersect(hlines.Bounds);

                    Rectangle bottom = new Rectangle(bounds.X, bounds.Bottom - boxheight, bounds.Width, boxheight);
                    bottom.Inflate(0, 2 * boxheight);
                    bottom.Intersect(hlines.Bounds);

                    return !hlines.IsAllWhite(top) && !hlines.IsAllWhite(bottom);
                }
                else
                {
                    return false;
                }
            }

            return lines;
        }

        private HashSet<LineShape> FilterHorizontalLines(Image hlines, BoundedObjectGrid<CheckboxShape> checkboxGrid)
        {
            HashSet<LineShape> lines = new HashSet<LineShape>();

            int minThickLineWidth = LineDetector.MinThickLineWidth.MulDiv(hlines.VerticalResolution, 200);
            int minThickLineLength = LineDetector.MinThickLineLength.MulDiv(hlines.HorizontalResolution, 200);
            int maxBoxSize = this.MaxBoxSize.MulDiv(hlines.HorizontalResolution, 200);

            // find connected components and do preliminary filtering
            foreach (ConnectedComponent component in hlines.FindConnectedComponents(8))
            {
                Rectangle bounds = component.Bounds;
                int maxWidth = component.MaxHeight();

                bool isBad = false;

                if (bounds.Width.Between(minThickLineWidth, minThickLineLength - 1) &&
                   bounds.Height.Between(minThickLineWidth, minThickLineLength - 1) &&
                   maxWidth > minThickLineLength)
                {
                    // too thick for the length
                    isBad = true;
                }

                if (!isBad && bounds.Width <= maxBoxSize && checkboxGrid != null && checkboxGrid.EnumObjects(bounds).Any())
                {
                    // is part of the check box
                    isBad = true;
                }

                if (isBad)
                {
                    hlines.SetWhite(bounds);
                }
                else
                {
                    LineShape newline = new LineShape(bounds, maxWidth, LineTypes.Horizontal);
                    lines.Add(newline);
                }
            }

            // find line vectors
            AlignedObjectGrid<LineShape> grid = new AlignedObjectGrid<LineShape>(hlines.Bounds, 10, 20, RectangleLTRBComparer.Default);
            grid.AddRange(lines, true, true);

            int minLineLength = (this.MinHorizontalLineLength * hlines.HorizontalResolution).Round();
            int maxGap = LineDetector.MaxLineGap.MulDiv(hlines.HorizontalResolution, 200);
            int tolerance = 6.MulDiv(hlines.VerticalResolution, 200);

            HashSet<LineShape> newlines = new HashSet<LineShape>();

            foreach (LineShape line in lines)
            {
                if (line.VerticalAlignment == VerticalAlignment.None)
                {
                    IList<LineShape> alignedLines = grid.FindVerticalAlignment(
                        line,
                        VerticalAlignment.Center,
                        Math.Min(maxGap, line.Bounds.Width),
                        tolerance,
                        (result, bounds) => bounds.Width >= minLineLength && (result.Count >= 2 || result.Any(o => o.Bounds.Width >= minLineLength)));

                    if (alignedLines.Count > 0)
                    {
                        LineShape newline = new LineShape(
                            Rectangle.Union(alignedLines.Select(x => x.Bounds)),
                            alignedLines[0].Begin,
                            alignedLines[alignedLines.Count - 1].End,
                            alignedLines.Max(x => x.Width),
                            LineTypes.Horizontal);

                        newlines.Add(newline);
                    }
                }
            }

            // remove merged and short lines
            lines.RemoveWhere(x =>
            {
                // remove merged lines
                bool remove = x.VerticalAlignment != VerticalAlignment.None;

                // remove small not merged lines
                // also delete them from the image
                if (x.VerticalAlignment == VerticalAlignment.None && x.Bounds.Width < minLineLength)
                {
                    hlines.SetWhite(x.Bounds);
                    remove = true;
                }

                return remove;
            });

            lines.UnionWith(newlines);

            return lines;
        }

        private int RemoveVerticalLinesFalsePositives(HashSet<LineShape> lines, Image vlines, Image nonVLines, Image itersections)
        {
            return lines.RemoveWhere(line =>
            {
                bool isBad = false;
                Rectangle bounds = line.Bounds;

                // Test density near the line if there are not enough intersections
                if (CountIntersections(line.Bounds) < 2)
                {
                    int count = CountAdjacentPixels(line.Width, bounds);
                    if (count > (bounds.Area * LineDetector.MaxNonLineDensity).Round())
                    {
                        isBad = true;
                    }
                }

                if (isBad)
                {
                    vlines.SetWhite(line.Bounds);
                }

                return isBad;
            });

            int CountIntersections(Rectangle bounds)
            {
                return itersections?.FindConnectedComponents(8, bounds)?.Count ?? 0;
            }

            int CountAdjacentPixels(int lineWidth, Rectangle bounds)
            {
                bounds.Inflate(lineWidth, 0);
                bounds.Intersect(nonVLines.Bounds);

                return (int)nonVLines.Power(bounds);
            }
        }

        private int RemoveHorizontalLinesFalsePositives(HashSet<LineShape> lines, Image hlines, Image nonHLines, Image itersections)
        {
            return lines.RemoveWhere(line =>
            {
                bool isBad = false;
                Rectangle bounds = line.Bounds;

                // Test density near the line if there are not enough intersections
                if (CountIntersections(line.Bounds) < 2)
                {
                    int count = CountAdjacentPixels(line.Width, bounds);
                    if (count > (bounds.Area * LineDetector.MaxNonLineDensity).Round())
                    {
                        isBad = true;
                    }
                }

                if (isBad)
                {
                    hlines.SetWhite(line.Bounds);
                }

                return isBad;
            });

            int CountAdjacentPixels(int lineWidth, Rectangle bounds)
            {
                bounds.Inflate(0, lineWidth);
                bounds.Intersect(nonHLines.Bounds);

                return (int)nonHLines.Power(bounds);
            }

            int CountIntersections(Rectangle bounds)
            {
                return itersections?.FindConnectedComponents(8, bounds)?.Count ?? 0;
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