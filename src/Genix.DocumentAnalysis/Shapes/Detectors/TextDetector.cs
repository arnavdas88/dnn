// -----------------------------------------------------------------------
// <copyright file="TextDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
    using Genix.Geometry;
    using Genix.Imaging;

    /// <summary>
    /// Finds machine-printed text on the <see cref="Image"/>.
    /// </summary>
    public class TextDetector
    {
        /// <summary>
        /// The maximum text height, in inches.
        /// </summary>
        private const float MaxTextHeight = 0.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDetector"/> class.
        /// </summary>
        public TextDetector()
        {
        }

        /// <summary>
        /// Finds machine-printed text on the <see cref="Image"/>.
        /// The type of text to find is determined by the class parameters.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="lines">The lines located on the <paramref name="image"/>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="TextDetector"/> that operation should be canceled.</param>
        /// <returns>
        /// The detected text.
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
        public ISet<TextShape> FindText(Image image, IEnumerable<LineShape> lines, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image closing = image.MorphClose(null, StructuringElement.Brick(9, 2), 1, BorderType.BorderConst, 0);

            // find components
            HashSet<ConnectedComponent> components = closing.FindConnectedComponents(4);

            // filter components
            int maxTextHeight = (TextDetector.MaxTextHeight * image.VerticalResolution).Round();
            components.RemoveWhere(x => /*x.Power > 10 &&*/ x.Bounds.Height > maxTextHeight);

            // index components
            BoundedObjectGrid<ConnectedComponent> componentgrid = new BoundedObjectGrid<ConnectedComponent>(image.Bounds, 10, 20);
            componentgrid.AddRange(components, true, true);
            componentgrid.Compact();

            // index vertical lines
            BoundedObjectGrid<LineShape> vlines = new BoundedObjectGrid<LineShape>(image.Bounds, 10, 1);
            vlines.AddRange(lines.Where(x => x.Types.HasFlag(LineTypes.Vertical)), true, true);

            BoundedObjectGrid<TextShape> shapegrid = new BoundedObjectGrid<TextShape>(image.Bounds, 10, 20);
            foreach (ConnectedComponent component in componentgrid.EnumObjects())
            {
                if (component.VerticalAlignment == VerticalAlignment.None)
                {
                    IList<ConnectedComponent> alignedComponents = FindTextShapes(componentgrid, component);
                    if (alignedComponents.Count > 0)
                    {
                        shapegrid.Add(new TextShape(Rectangle.Union(alignedComponents.Select(x => x.Bounds))), true, true);
                    }
                }
            }

            // assign unassigned components
            foreach (ConnectedComponent component in componentgrid.EnumObjects())
            {
                if (component.VerticalAlignment == VerticalAlignment.None)
                {
                    if (shapegrid.FindContainer(component.Bounds) == null)
                    {
                        shapegrid.Add(new TextShape(component.Bounds), true, true);
                    }
                }
            }

            shapegrid.Compact();

            // create result
            HashSet<TextShape> shapes = new HashSet<TextShape>();
            shapes.UnionWith(shapegrid.EnumObjects().Where(x => x.Bounds.Width > 4 && x.Bounds.Height > 10));

#if DEBUG
            Image draft = image.ConvertTo(null, 32);
            foreach (TextShape shape in shapes)
            {
                draft.DrawRectangle(shape.Bounds, Color.Red);
            }
#endif

            return shapes;

            IList<ConnectedComponent> FindTextShapes(BoundedObjectGrid<ConnectedComponent> grid, ConnectedComponent obj)
            {
                ////Rectangle bounds = obj.Bounds;
                SortedList<Rectangle, ConnectedComponent> result = new SortedList<Rectangle, ConnectedComponent>(RectangleLTRBComparer.Default);

                Rectangle obounds = obj.Bounds;

                // calculate initial pivot points
                Line topline = new Line(obounds.Left, obounds.Top, obounds.Right, obounds.Top);
                Line bottomline = new Line(obounds.Left, obounds.Bottom, obounds.Right, obounds.Bottom);

                // find objects to the left
                ConnectedComponent next;
                while ((next = FindNext(obounds, false)) != null)
                {
                    result.Add(next.Bounds, next);

                    next.VerticalAlignment = VerticalAlignment.Bottom;
                    obounds.Union(next.Bounds);
                }

                // find objects to the right
                while ((next = FindNext(obounds, true)) != null)
                {
                    result.Add(next.Bounds, next);

                    next.VerticalAlignment = VerticalAlignment.Bottom;
                    obounds.Union(next.Bounds);
                }

                if (result.Count > 0)
                {
                    obj.VerticalAlignment = VerticalAlignment.Bottom;
                    result.Add(obj.Bounds, obj);

                    // mark contained elements
                    foreach (ConnectedComponent o in grid.EnumObjects(obounds))
                    {
                        if (o.VerticalAlignment == VerticalAlignment.None && obounds.Contains(o.Bounds))
                        {
                            o.VerticalAlignment = VerticalAlignment.Bottom;
                            result.Add(o.Bounds, o);
                        }
                    }
                }

                return result.Values;

                ConnectedComponent FindNext(Rectangle box, bool searchForward)
                {
                    // Tolerance to skew on top of current estimate of skew. Divide x or y length
                    // by kMaxSkewFactor to get the y or x skew distance.
                    // If the angle is small, the angle in degrees is roughly 60/kMaxSkewFactor.
                    const int MaxSkewFactor = 15;

                    // compute search area
                    int xstart = searchForward ? box.Right : box.Left;
                    int xend = searchForward ? box.Right + (box.Height * 2) : box.Left - (box.Height * 2);

                    Rectangle searchArea = searchForward ?
                        Rectangle.FromLTRB(xstart, box.Top, xend, box.Bottom) :
                        Rectangle.FromLTRB(xend, box.Top, xstart, box.Bottom);

                    // look for lines crossing the search area
                    foreach (LineShape line in vlines.EnumObjects(searchArea))
                    {
                        int x = new Line(line.Begin, line.End).X(box.Bottom);

                        if (searchForward)
                        {
                            int right = MinMax.Min(searchArea.Right, x - (line.Width / 2));
                            searchArea.InflateX(0, right - searchArea.Right);
                        }
                        else
                        {
                            int left = MinMax.Max(searchArea.Left, x + (line.Width / 2));
                            searchArea.InflateX(searchArea.Left - left, 0);
                        }
                    }

                    if (searchArea.Width == 0 || searchArea.Height == 0)
                    {
                        return null;
                    }

                    // Compute skew tolerance and expand the search box
                    int skewTolerance = searchArea.Width / MaxSkewFactor;
                    searchArea.Inflate(0, skewTolerance);

                    ConnectedComponent bestCandidate = null;
                    int bestDistance = int.MaxValue;
                    foreach (ConnectedComponent candidate in grid.EnumObjects(searchArea))
                    {
                        if (candidate.VerticalAlignment == VerticalAlignment.None)
                        {
                            Rectangle cbounds = candidate.Bounds;
                            int nearestx = searchForward ? cbounds.Left : cbounds.Right;
                            int distx = box.DistanceToX(nearestx);
                            bool skip = false;

                            // verify candidate position against baseline
                            if (!skip && bottomline.IsBelow(nearestx, cbounds.Top))
                            {
                                // candidate is below baseline
                                skip = true;
                            }

                            if (!skip && Math.Abs(bottomline.Y(nearestx) - cbounds.Bottom) > 10)
                            {
                                // candidate's bottom is far from baseline
                                skip = true;
                            }

                            if (!skip &&
                                ((cbounds.Height > box.Height / 2 && cbounds.Height > 2 * box.Height) ||
                                 (box.Height > cbounds.Height / 2 && box.Height > 2 * cbounds.Height)))
                            {
                                // box sizes too different
                                skip = true;

                                if (box.ContainsY(cbounds) && distx < box.Height)
                                {
                                    // accept small elements locates inside the box that close to it
                                    // could be some detached c.c. or punctuation
                                    skip = false;
                                }
                            }

                            // find nearest element based on Eucledian distance
                            if (!skip)
                            {
                                int distance = box.DistanceToSquared(cbounds);
                                if (distance < bestDistance)
                                {
                                    bestCandidate = candidate;
                                    bestDistance = distance;
                                }
                            }
                        }
                    }

                    // update baseline and box
                    if (bestCandidate != null)
                    {
                        Rectangle cbounds = bestCandidate.Bounds;

                        if (cbounds.Height > box.Height / 2)
                        {
                            if (searchForward)
                            {
                                bottomline.X2 = cbounds.Right;
                                bottomline.Y2 = cbounds.Bottom;
                            }
                            else
                            {
                                bottomline.X1 = cbounds.Left;
                                bottomline.Y1 = cbounds.Bottom;
                            }
                        }
                    }

                    return bestCandidate;
                }
            }
        }
    }
}