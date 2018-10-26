// -----------------------------------------------------------------------
// <copyright file="AlignedObjectGrid.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using Genix.Drawing;

    /// <summary>
    /// Represents a grid that holds a collection of <see cref="IAlignedBoundedObject"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the grid.</typeparam>
    public class AlignedObjectGrid<T> : BoundedObjectGrid<T>
        where T : class, IAlignedBoundedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlignedObjectGrid{T}"/> class.
        /// </summary>
        /// <param name="bounds">The grid bounding box.</param>
        /// <param name="cellWidth">The width of each cell, in pixels.</param>
        /// <param name="cellHeight">The height of each cell, in pixels.</param>
        public AlignedObjectGrid(Rectangle bounds, int cellWidth, int cellHeight)
            : base(bounds, cellWidth, cellHeight)
        {
        }

        public IList<T> FindVerticalAlignment(T obj, VerticalAlignment verticalAlignment, int maxGap)
        {
            ////Rectangle bounds = obj.Bounds;
            SortedList<Rectangle, T> result = new SortedList<Rectangle, T>(RectangleLTRBComparer.Default);

            Rectangle obounds = obj.Bounds;
            result.Add(obounds, obj);

            // calculate initial pivot points
            Line baseline = new Line(
                obounds.Left,
                verticalAlignment == VerticalAlignment.Top ? obounds.Top : obounds.Bottom,
                obounds.Right,
                verticalAlignment == VerticalAlignment.Top ? obounds.Top : obounds.Bottom);

            // find objects to the left
            T next = FindNext(obounds, false);
            while (next != null)
            {
                result.Add(next.Bounds, next);
                next = FindNext(next.Bounds, false);
            }

            // find objects to the right
            next = FindNext(obounds, true);
            while (next != null)
            {
                result.Add(next.Bounds, next);
                next = FindNext(next.Bounds, true);
            }

            return result.Values;

            T FindNext(Rectangle box, bool searchForward)
            {
                // Tolerance to skew on top of current estimate of skew. Divide x or y length
                // by kMaxSkewFactor to get the y or x skew distance.
                // If the angle is small, the angle in degrees is roughly 60/kMaxSkewFactor.
                const int MaxSkewFactor = 15;

                // new objects found must extend beyond current box
                int xstart = searchForward ? box.Right : box.Left;
                int xend = searchForward ? xstart + maxGap : xstart - maxGap;
                int ystart = verticalAlignment == VerticalAlignment.Top ? box.Top : box.Bottom;

                // Compute skew tolerance
                int skewTolerance = maxGap / MaxSkewFactor;

                // Expand the box
                int ymin = ystart - (box.Height / 2) - skewTolerance;
                int ymax = ystart + (box.Height / 5) + skewTolerance;

                // Search the grid
                Rectangle searchArea = searchForward ?
                    Rectangle.FromLTRB(xstart, ymin, xend, ymax) :
                    Rectangle.FromLTRB(xend, ymin, xstart, ymax);

                T bestCandidate = null;
                int bestDistance = int.MaxValue;
                foreach (T candidate in this.EnumObjects(searchArea))
                {
                    if (candidate.VerticalAlignment == VerticalAlignment.None)
                    {
                        Rectangle cbounds = candidate.Bounds;

                        // element was already selected
                        if (result.ContainsKey(cbounds))
                        {
                            continue;
                        }

                        // verify candidate position againts baseline
                        if (verticalAlignment == VerticalAlignment.Top && baseline.IsAbove(cbounds.CenterX, cbounds.Bottom))
                        {
                            continue;
                        }

                        if (verticalAlignment == VerticalAlignment.Bottom && baseline.IsBelow(cbounds.CenterX, cbounds.Top))
                        {
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

                return bestCandidate;
            }
        }
    }
}
