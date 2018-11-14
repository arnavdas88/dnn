// -----------------------------------------------------------------------
// <copyright file="AlignedObjectGrid.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        /// <summary>
        /// Returns objects in the grid that are vertically aligned with the specified object.
        /// </summary>
        /// <param name="obj">The object the found objects should be aligned with.</param>
        /// <param name="alignment">The alignment.</param>
        /// <param name="maxGap">The maximum allowed distance between objects, in pixels.</param>
        /// <param name="tolerance">Determines by how many pixels the aligned objects can deviate from the baseline.</param>
        /// <param name="acceptor">The delegate this method calls to accept the results of alignment.</param>
        /// <returns>
        /// The collection that contains found objects. <paramref name="obj"/> is included in the collection.
        /// </returns>
        public IList<T> FindVerticalAlignment(
            T obj,
            VerticalAlignment alignment,
            int maxGap,
            int tolerance,
            Func<IList<T>, Rectangle, bool> acceptor)
        {
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
                obounds.Union(next.Bounds);
            }

            // find objects to the right
            while ((next = FindNext(obounds, true)) != null)
            {
                result.Add(next.Bounds, next);
                obounds.Union(next.Bounds);
            }

            if (result.Count > 0)
            {
                result.Add(obj.Bounds, obj);

                if (acceptor(result.Values, obounds))
                {
                    for (int i = 0, ii = result.Values.Count; i < ii; i++)
                    {
                        result.Values[i].VerticalAlignment = alignment;
                    }

                    return result.Values;
                }
            }

            return new T[0];

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
                searchArea.Inflate(0, skewTolerance);

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
                        if (Math.Abs(ybase - ytest) > tolerance)
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

        /// <summary>
        /// Returns objects in the grid that are horizontally aligned with the specified object.
        /// </summary>
        /// <param name="obj">The object the found objects should be aligned with.</param>
        /// <param name="alignment">The alignment.</param>
        /// <param name="maxGap">The maximum allowed distance between objects, in pixels.</param>
        /// <param name="tolerance">Determines by how many pixels the aligned objects can deviate from the baseline.</param>
        /// <param name="acceptor">The delegate this method calls to accept the results of alignment.</param>
        /// <returns>
        /// The collection that contains found objects. <paramref name="obj"/> is included in the collection.
        /// </returns>
        public IList<T> FindHorizontalAlignment(
            T obj,
            HorizontalAlignment alignment,
            int maxGap,
            int tolerance,
            Func<IList<T>, Rectangle, bool> acceptor)
        {
            SortedList<Rectangle, T> result = new SortedList<Rectangle, T>(RectangleTBLRComparer.Default);

            Rectangle obounds = obj.Bounds;

            // calculate initial pivot points
            int x = BoxBound(obounds);
            Line baseline = new Line(x, obounds.Top, x, obounds.Bottom);

            // find objects to the top
            T next;
            while ((next = FindNext(obounds, false)) != null)
            {
                result.Add(next.Bounds, next);
                obounds.Union(next.Bounds);
            }

            // find objects to the bottom
            while ((next = FindNext(obounds, true)) != null)
            {
                result.Add(next.Bounds, next);
                obounds.Union(next.Bounds);
            }

            if (result.Count > 0)
            {
                result.Add(obj.Bounds, obj);

                if (acceptor(result.Values, obounds))
                {
                    for (int i = 0, ii = result.Values.Count; i < ii; i++)
                    {
                        result.Values[i].HorizontalAlignment = alignment;
                    }

                    return result.Values;
                }
            }

            return new T[0];

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
                        if (Math.Abs(xbase - xtest) > tolerance)
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
