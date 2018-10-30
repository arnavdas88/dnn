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
            ISet<Line> hlinesResult = null;
            ISet<Line> vlinesResult = null;

            // close up small holes
            int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
            Image closedImage = image.MorphClose(null, StructuringElement.Square(maxLineWidth / 3), 1, BorderType.BorderConst, image.WhiteColor);

            // open up to detect big solid areas
            Image openedImage = closedImage.MorphOpen(null, StructuringElement.Square(maxLineWidth), 1, BorderType.BorderConst, image.WhiteColor);
            Image hollowImage = closedImage.Sub(null, openedImage, 0);

            if (this.FindCheckboxes)
            {
                FindAndRemoveCheckboxes();
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
                itersections = vlines?.And(null, hlines);

                // re-filter lines
                hlinesResult = FilterLines(hlines, nonHLines, false);

                if (this.RemoveLines)
                {
                    RemoveLines(hlines, nonHLines);
                }
            }

            RemoveIntersections();

            return CreateAnswer();

            void FindLines()
            {
                // open up in both directions to find lines
                int minLineLength = LineDetector.MinLineLength.MulDiv(image.HorizontalResolution, 200);
                hlines = hollowImage.MorphOpen(null, StructuringElement.Brick(minLineLength, 1), 1, BorderType.BorderConst, image.WhiteColor);
                if (hlines.IsAllWhite())
                {
                    hlines = null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                vlines = hollowImage.MorphOpen(null, StructuringElement.Brick(1, minLineLength), 1, BorderType.BorderConst, image.WhiteColor);
                if (vlines.IsAllWhite())
                {
                    vlines = null;
                }

                cancellationToken.ThrowIfCancellationRequested();

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

                    cancellationToken.ThrowIfCancellationRequested();
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

                    cancellationToken.ThrowIfCancellationRequested();
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

                    if (bounds.Width.Between(minThickLineWidth, minThickLineLength - 1) &&
                        bounds.Height.Between(minThickLineWidth, minThickLineLength - 1) &&
                        maxWidth > minThickLineLength)
                    {
                        // too thick for the length
                        isBad = true;
                    }

                    // Test density near the line if there are not enough intersections
                    if (!isBad && CountIntersections(bounds) < 2)
                    {
                        ulong count = CountAdjacentPixels(maxWidth, bounds);
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

                cancellationToken.ThrowIfCancellationRequested();

                return answer;

                ulong CountAdjacentPixels(int lineWidth, Rectangle bounds)
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

            ISet<Shape> CreateAnswer()
            {
                HashSet<Shape> answer = new HashSet<Shape>();

                if (this.LineTypes.HasFlag(LineTypes.Horizontal) && hlinesResult != null)
                {
                    int minLineLength = (this.MinHorizontalLineLength * image.HorizontalResolution).Round();

                    foreach (Line line in hlinesResult)
                    {
                        if (line.Component.Bounds.Width >= minLineLength)
                        {
                            answer.Add(new LineShape(line.Component.Bounds, line.Width, LineTypes.Horizontal));
                        }
                    }
                }

                if (this.LineTypes.HasFlag(LineTypes.Vertical) && vlinesResult != null)
                {
                    int minLineLength = (this.MinVerticalLineLength * image.VerticalResolution).Round();

                    foreach (Line line in vlinesResult)
                    {
                        if (line.Component.Bounds.Height >= minLineLength)
                        {
                            answer.Add(new LineShape(line.Component.Bounds, line.Width, LineTypes.Vertical));
                        }
                    }
                }

                if (checkboxResult != null)
                {
                    answer.UnionWith(checkboxResult);
                }

                return answer;
            }

            void FindAndRemoveCheckboxes()
            {
                checkboxResult = new HashSet<CheckboxShape>(32);

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
                                                        checkboxResult.Add(new CheckboxShape(Rectangle.Union(hbounds, vunion)));
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

                BoundedObjectGrid<ConnectedComponent> FindHorizontalLines()
                {
                    Image lines = hollowImage.MorphOpen(null, StructuringElement.Brick(minBoxSizeH, 1), 1, BorderType.BorderConst, hollowImage.WhiteColor);
                    if (lines.IsAllWhite())
                    {
                        return null;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);
                    ////comps.Re

                    cancellationToken.ThrowIfCancellationRequested();

                    BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                        lines.Bounds,
                        (lines.Width / 10).Clip(1, lines.Width),
                        (lines.Height / 20).Clip(1, lines.Height));
                    grid.Add(comps.Where(x => x.Bounds.Width <= maxBoxSizeH), true, true);

                    cancellationToken.ThrowIfCancellationRequested();

                    return grid;
                }

                BoundedObjectGrid<ConnectedComponent> FindVerticalLines()
                {
                    Image lines = hollowImage.MorphOpen(null, StructuringElement.Brick(1, minBoxSizeV), 1, BorderType.BorderConst, hollowImage.WhiteColor);
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
                    grid.Add(comps.Where(x => x.Bounds.Height <= maxBoxSizeV), true, true);

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
        }

        private class Line
        {
            public ConnectedComponent Component { get; set; }

            public int Width { get; set; }

            public bool IsVertical { get; set; }
        }
    }
}