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
        /// <param name="width">The width of the grid, in cells.</param>
        /// <param name="height">The height of the grid, in pixels.</param>
        public AlignedObjectGrid(Rectangle bounds, int width, int height)
            : base(bounds, width, height)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignedObjectGrid{T}"/> class.
        /// </summary>
        /// <param name="bounds">The grid bounding box.</param>
        /// <param name="width">The width of the grid, in cells.</param>
        /// <param name="height">The height of the grid, in pixels.</param>
        /// <param name="comparer">The <see cref="IComparer{Rectangle}"/> implementation to use when comparing object bounding boxes.</param>
        public AlignedObjectGrid(Rectangle bounds, int width, int height, IComparer<Rectangle> comparer)
            : base(bounds, width, height, comparer)
        {
        }

        public IList<T> FindVerticalAlignment(T obj, VerticalAlignment verticalAlignment, int maxGap)
        {
            ////Rectangle bounds = obj.Bounds;
            SortedList<Rectangle, T> result = new SortedList<Rectangle, T>(RectangleLTRBComparer.Default);

            Rectangle obounds = obj.Bounds;

            // calculate initial pivot points
            Line baseline = new Line(
                obounds.Left,
                verticalAlignment == VerticalAlignment.Top ? obounds.Top : obounds.Bottom,
                obounds.Right,
                verticalAlignment == VerticalAlignment.Top ? obounds.Top : obounds.Bottom);

            // find objects to the left
            T next;
            while ((next = FindNext(obounds, false)) != null)
            {
                result.Add(next.Bounds, next);

                next.VerticalAlignment = verticalAlignment;
                obounds.Union(next.Bounds);
            }

            // find objects to the right
            while ((next = FindNext(obounds, true)) != null)
            {
                result.Add(next.Bounds, next);

                next.VerticalAlignment = verticalAlignment;
                obounds.Union(next.Bounds);
            }

            if (result.Count > 0)
            {
                obj.VerticalAlignment = verticalAlignment;
                result.Add(obj.Bounds, obj);

                // mark contained elements
                foreach (T o in this.EnumObjects(obounds))
                {
                    if (o.VerticalAlignment == VerticalAlignment.None && obounds.Contains(o.Bounds))
                    {
                        o.VerticalAlignment = verticalAlignment;
                        result.Add(o.Bounds, o);
                    }
                }
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
                int xend = searchForward ? box.Right + maxGap : box.Left - maxGap;
                int ystart = verticalAlignment == VerticalAlignment.Top ? box.Top : box.Bottom;

                // Compute skew tolerance
                int skewTolerance = maxGap / MaxSkewFactor;

                // Expand the box
                int ymin = box.Top/*ystart - (box.Height / 2)*/ - skewTolerance;
                int ymax = box.Bottom/*ystart + (box.Height / 5)*/ + skewTolerance;

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

                        // verify candidate position against baseline
                        if (verticalAlignment == VerticalAlignment.Top)
                        {
                            int nearestx = searchForward ? cbounds.Left : cbounds.Right;
                            if (baseline.IsAbove(nearestx, cbounds.Bottom))
                            {
                                // candidate is above baseline
                                continue;
                            }

                            if (Math.Abs(baseline.Y(nearestx) - cbounds.Top) > 10)
                            {
                                // candidate's top is far from baseline
                                continue;
                            }
                        }

                        if (verticalAlignment == VerticalAlignment.Bottom)
                        {
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
                            baseline.Y2 = verticalAlignment == VerticalAlignment.Top ? cbounds.Top : cbounds.Bottom;
                        }
                        else
                        {
                            baseline.X1 = cbounds.Left;
                            baseline.Y1 = verticalAlignment == VerticalAlignment.Top ? cbounds.Top : cbounds.Bottom;
                        }
                    }
                }

                return bestCandidate;
            }
        }

        public IList<T> FindVerticalAlignment(T obj, VerticalAlignment alignment, int maxGap, int minNumberOfAlignedObjects)
        {
            ////Rectangle bounds = obj.Bounds;
            SortedList<Rectangle, T> result = new SortedList<Rectangle, T>(RectangleLTRBComparer.Default);

            Rectangle obounds = obj.Bounds;

            // calculate initial pivot points
            int y = BoxBound(obounds);
            Line baseline = new Line(obounds.X, y, obounds.Right, y);

            // find objects to the left
            T next;
            while ((next = FindNext(obounds, false)) != null)
            {
                result.Add(next.Bounds, next);

                next.VerticalAlignment = alignment;
                obounds.Union(next.Bounds);
            }

            // find objects to the right
            while ((next = FindNext(obounds, true)) != null)
            {
                result.Add(next.Bounds, next);

                next.VerticalAlignment = alignment;
                obounds.Union(next.Bounds);
            }

            if (result.Count >= minNumberOfAlignedObjects - 1)
            {
                obj.VerticalAlignment = alignment;
                result.Add(obj.Bounds, obj);
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
                int ystart = baseline.Y(xstart);

                int xend = searchForward ? xstart + maxGap : xstart - maxGap;
                int yend = baseline.Y(xend);

                Rectangle searchArea = new Rectangle(new Point(xstart, ystart), new Point(xend, yend));

                // Compute skew tolerance
                int skewTolerance = maxGap / MaxSkewFactor;
                searchArea.Inflate(skewTolerance, 0);

                T bestCandidate = null;
                int bestDistance = int.MaxValue;
                foreach (T candidate in this.EnumObjects(searchArea))
                {
                    if (candidate.VerticalAlignment == VerticalAlignment.None)
                    {
                        Rectangle cbounds = candidate.Bounds;

                        // test alignment
                        int ytest = BoxBound(cbounds);
                        int ybase = baseline.Y(searchForward ? box.Left : box.Right);
                        if (Math.Abs(ybase - ytest) > 10 /* tolerance */)
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

                // update baseline and box
                if (bestCandidate != null)
                {
                    Rectangle cbounds = bestCandidate.Bounds;

                    if (searchForward)
                    {
                        baseline.X2 = cbounds.Right;
                        baseline.Y2 = BoxBound(cbounds);
                    }
                    else
                    {
                        baseline.X1 = cbounds.Left;
                        baseline.Y1 = BoxBound(cbounds);
                    }
                }

                return bestCandidate;
            }

            int BoxBound(Rectangle r)
            {
                return alignment == VerticalAlignment.Top ? r.Top : (alignment == VerticalAlignment.Bottom ? r.Bottom : r.CenterY);
            }
        }

        public IList<T> FindHorizontalAlignment(T obj, HorizontalAlignment alignment, int maxGap, int minNumberOfAlignedObjects)
        {
            ////Rectangle bounds = obj.Bounds;
            SortedList<Rectangle, T> result = new SortedList<Rectangle, T>(RectangleLTRBComparer.Default);

            Rectangle obounds = obj.Bounds;

            // calculate initial pivot points
            int x = BoxBound(obounds);
            Line baseline = new Line(x, obounds.Top, x, obounds.Bottom);

            // find objects to the left
            T next;
            while ((next = FindNext(obounds, false)) != null)
            {
                result.Add(next.Bounds, next);

                next.HorizontalAlignment = alignment;
                obounds.Union(next.Bounds);
            }

            // find objects to the right
            while ((next = FindNext(obounds, true)) != null)
            {
                result.Add(next.Bounds, next);

                next.HorizontalAlignment = alignment;
                obounds.Union(next.Bounds);
            }

            if (result.Count >= minNumberOfAlignedObjects - 1)
            {
                obj.HorizontalAlignment = alignment;
                result.Add(obj.Bounds, obj);
            }

            return result.Values;

            T FindNext(Rectangle box, bool searchForward)
            {
                // Tolerance to skew on top of current estimate of skew. Divide x or y length
                // by kMaxSkewFactor to get the y or x skew distance.
                // If the angle is small, the angle in degrees is roughly 60/kMaxSkewFactor.
                const int MaxSkewFactor = 15;

                // new objects found must extend beyond current box
                int ystart = searchForward ? box.Bottom : box.Top;
                int xstart = baseline.X(ystart);

                int yend = searchForward ? ystart + maxGap : ystart - maxGap;
                int xend = baseline.X(yend);

                Rectangle searchArea = new Rectangle(new Point(xstart, ystart), new Point(xend, yend));

                // Compute skew tolerance
                int skewTolerance = maxGap / MaxSkewFactor;
                searchArea.Inflate(skewTolerance, 0);

                T bestCandidate = null;
                int bestDistance = int.MaxValue;
                foreach (T candidate in this.EnumObjects(searchArea))
                {
                    if (candidate.HorizontalAlignment == HorizontalAlignment.None)
                    {
                        Rectangle cbounds = candidate.Bounds;

                        // test alignment
                        int xtest = BoxBound(cbounds);
                        int xbase = baseline.X(searchForward ? box.Top : box.Bottom);
                        if (Math.Abs(xbase - xtest) > 10 /* tolerance */)
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

                // update baseline and box
                if (bestCandidate != null)
                {
                    Rectangle cbounds = bestCandidate.Bounds;

                    if (searchForward)
                    {
                        baseline.X2 = BoxBound(cbounds);
                        baseline.Y2 = cbounds.Bottom;
                    }
                    else
                    {
                        baseline.X1 = BoxBound(cbounds);
                        baseline.Y1 = cbounds.Top;
                    }
                }

                return bestCandidate;
            }

            int BoxBound(Rectangle r)
            {
                return alignment == HorizontalAlignment.Left ? r.Left : (alignment == HorizontalAlignment.Right ? r.Right : r.CenterX);
            }
        }
    }
}
