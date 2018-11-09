﻿// -----------------------------------------------------------------------
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
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Finds machine-printed text on the <see cref="Image"/>.
    /// </summary>
    public class TextDetector
    {
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
        public ISet<TextShape> FindText(Image image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image closing = image.MorphClose(null, StructuringElement.Brick(5, 1), 1, BorderType.BorderConst, 0);

            AlignedObjectGrid<ConnectedComponent> componentgrid = new AlignedObjectGrid<ConnectedComponent>(image.Bounds, 10, 20);
            componentgrid.AddRange(
                closing.FindConnectedComponents(4).Where(x => /*x.Power > 10 &&*/ x.Bounds.Height <= 100),
                true,
                true);
            componentgrid.Compact();

            AlignedObjectGrid<TextShape> shapegrid = new AlignedObjectGrid<TextShape>(image.Bounds, 10, 20);
            foreach (ConnectedComponent component in componentgrid.EnumObjects())
            {
                if (component.VerticalAlignment == VerticalAlignment.None)
                {
                    IList<ConnectedComponent> alignedComponents = FindTextShapes(componentgrid, component, 50);
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

            Image draft = image.ConvertTo(null, 24);
            foreach (TextShape shape in shapes)
            {
                draft.DrawRectangle(shape.Bounds, Color.Red);
            }

            int count = shapes.Count();

            return shapes;

            IList<ConnectedComponent> FindTextShapes(AlignedObjectGrid<ConnectedComponent> grid, ConnectedComponent obj, int maxGap)
            {
                ////Rectangle bounds = obj.Bounds;
                SortedList<Rectangle, ConnectedComponent> result = new SortedList<Rectangle, ConnectedComponent>(RectangleLTRBComparer.Default);

                Rectangle obounds = obj.Bounds;

                // calculate initial pivot points
                Line baseline = new Line(obounds.Left, obounds.Bottom, obounds.Right, obounds.Bottom);

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

                    // new objects found must extend beyond current box
                    int xstart = searchForward ? box.Right : box.Left;
                    int xend = searchForward ? box.Right + maxGap : box.Left - maxGap;
                    int ystart = box.Bottom;

                    // Compute skew tolerance
                    int skewTolerance = maxGap / MaxSkewFactor;

                    // Expand the box
                    int ymin = box.Top/*ystart - (box.Height / 2)*/ - skewTolerance;
                    int ymax = box.Bottom/*ystart + (box.Height / 5)*/ + skewTolerance;

                    // Search the grid
                    Rectangle searchArea = searchForward ?
                        Rectangle.FromLTRB(xstart, ymin, xend, ymax) :
                        Rectangle.FromLTRB(xend, ymin, xstart, ymax);

                    ConnectedComponent bestCandidate = null;
                    int bestDistance = int.MaxValue;
                    foreach (ConnectedComponent candidate in grid.EnumObjects(searchArea))
                    {
                        if (candidate.VerticalAlignment == VerticalAlignment.None)
                        {
                            Rectangle cbounds = candidate.Bounds;

                            // verify candidate position against baseline

                            int nearestx = searchForward ? cbounds.Left : cbounds.Right;
                            if (baseline.IsBelow(nearestx, cbounds.Top))
                            {
                                // candidate is below baseline
                                continue;
                            }

                            if (Math.Abs(baseline.Y(nearestx) - cbounds.Bottom) > 10)
                            {
                                // candidate's bottom is far from baseline
                                continue;
                            }

                            if ((cbounds.Height > box.Height / 2 && cbounds.Height > 2 * box.Height) ||
                                (box.Height > cbounds.Height / 2 && box.Height > 2 * cbounds.Height))
                            {
                                // box sizes too different
                                continue;
                            }

                            // find nearest element based on Eucledian distance
                            int distance = box.DistanceToSquared(cbounds);
                            if (distance < bestDistance)
                            {
                                bestCandidate = candidate;
                                bestDistance = distance;
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
                                baseline.X2 = cbounds.Right;
                                baseline.Y2 = cbounds.Bottom;
                            }
                            else
                            {
                                baseline.X1 = cbounds.Left;
                                baseline.Y1 = cbounds.Bottom;
                            }
                        }
                    }

                    return bestCandidate;
                }
            }
        }
    }
}