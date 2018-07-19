// -----------------------------------------------------------------------
// <copyright file="ConnectedComponent.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#define NEW

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    [DebuggerDisplay("{Bounds}")]
    public class ConnectedComponent : IBoundedObject
    {
#if NEW
        private static readonly int[][] EmptyStrokes = new int[0][];
        private int[][] strokes = ConnectedComponent.EmptyStrokes;
#else
        private readonly List<int> intervals = new List<int>();
#endif

        private Rectangle bounds = Rectangle.Empty;
        private int power = -1;

        internal ConnectedComponent(int y, int x, int length)
        {
            this.AddStroke(y, x, length);
        }

        private ConnectedComponent()
        {
        }

        /// <summary>
        /// Gets the number of black pixels on this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The number of black pixels on this <see cref="ConnectedComponent"/>.
        /// </value>
        public int Power
        {
            get
            {
                if (this.power == -1)
                {
                    int sum = 0;
#if NEW
                    int[][] values = this.strokes;
                    for (int i = 0, ii = values.Length; i < ii; i++)
                    {
                        int[] intervals = values[i];
                        for (int j = 1, jj = intervals.Length; j < jj; j += 2)
                        {
                            sum += intervals[j];
                        }
                    }
#else
                    for (int i = 0; i < this.intervals.Count; i += 3)
                    {
                        sum += this.intervals[i + 2];
                    }
#endif
                    this.power = sum;
                }

                return this.power;
            }
        }

        /// <summary>
        /// Gets the bounds, in pixels, of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Drawing.Rectangle"/> structure that contains the bounds, in pixels, of this <see cref="ConnectedComponent"/>.
        /// </value>
        public Rectangle Bounds => this.bounds;

        /// <summary>
        /// Adds a stroke to this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="y">The y-coordinate of the stroke.</param>
        /// <param name="x">The x-coordinate of the stroke.</param>
        /// <param name="length">The length of the stroke.</param>
        public void AddStroke(int y, int x, int length)
        {
#if NEW
            // insert new horizontal line
            if (this.strokes.Length == 0)
            {
                this.strokes = new int[1][];
            }
            else if (y < this.bounds.Y)
            {
                ConnectedComponent.ExpandStrokes(ref this.strokes, 0, this.bounds.Y - y);
            }
            else if (y >= this.bounds.Bottom)
            {
                ConnectedComponent.ExpandStrokes(ref this.strokes, this.bounds.Height, y - this.bounds.Bottom + 1);
            }

            // update position
            this.bounds.Union(x, y, length, 1);

            // insert stroke into the line
            ConnectedComponent.InsertStroke(ref this.strokes[y - this.bounds.Y], x, length);
#else
            // find insertion point
            int pivot = this.FindInsertionPoint(y, x, 0);

            // insert
            this.AddSegment(pivot, y, x, length);
#endif
        }

        /// <summary>
        /// Returns a collection of strokes in this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <returns>
        /// The collection of strokes in this <see cref="ConnectedComponent"/>.
        /// </returns>
        public IEnumerable<(int y, int x, int length)> EnumStrokes()
        {
#if NEW
            int[][] values1 = this.strokes;
            for (int i = 0, ii = values1.Length, y = this.bounds.Y; i < ii; i++, y++)
            {
                int[] values2 = values1[i];
                for (int j = 0, jj = values2.Length; j < jj; j += 2)
                {
                    yield return (y, values2[j], values2[j + 1]);
                }
            }
#else
            for (int i = 0; i < this.intervals.Count; i += 3)
            {
                yield return (this.intervals[i], this.intervals[i + 1], this.intervals[i + 2]);
            }
#endif
        }

        /// <summary>
        /// Merges this <see cref="ConnectedComponent"/> with the specified <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to merge with.</param>
        public void MergeWith(ConnectedComponent component)
        {
#if NEW
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            // pre-allocate stroke holders
            if (component.bounds.Y < this.bounds.Y || component.bounds.Bottom > this.bounds.Bottom)
            {
                int y1 = Maximum.Min(component.bounds.Y, this.bounds.Y);
                int y2 = Maximum.Max(component.bounds.Bottom, this.bounds.Bottom);

                int[][] newstrokes = new int[y2 - y1][];
                Array.Copy(this.strokes, 0, newstrokes, this.bounds.Y - y1, this.bounds.Height);
                this.strokes = newstrokes;

                // update position
                this.bounds.Union(component.bounds);
            }

            int[][] values1 = component.strokes;
            for (int i = 0, ii = values1.Length, y = component.bounds.Y; i < ii; i++, y++)
            {
                int[] values2 = values1[i];
                for (int j = 0, jj = values2.Length; j < jj; j += 2)
                {
                    this.AddStroke(y, values2[j], values2[j + 1]);
                }
            }
#else
            int pivot = 0;
            for (int i = 0; i < component.intervals.Count; i += 3)
            {
                int y = component.intervals[i];
                int x = component.intervals[i + 1];
                int length = component.intervals[i + 2];

                // find insertion point
                pivot = this.FindInsertionPoint(y, x, pivot);

                // insert
                this.AddSegment(pivot, y, x, length);
            }
#endif
        }

        internal bool TouchesBottom(int y, int x, int length)
        {
#if NEW
            if (this.strokes.Length > 0 && this.bounds.Y < y && y <= this.bounds.Bottom)
            {
                int[] values = this.strokes[y - 1 - this.bounds.Y];
                for (int i = 0, ii = values.Length; i < ii; i += 2)
                {
                    if (ConnectedComponent.StrokesIntersect(values[i], values[i + 1], x, length))
                    {
                        return true;
                    }
                }
            }
#else
            for (int i = this.intervals.Count - 3; i >= 0; i -= 3)
            {
                int baseY = this.intervals[i];

                if (baseY == y - 1)
                {
                    int baseX = this.intervals[i + 1];
                    int baseLength = this.intervals[i + 2];
                    if ((x >= baseX && x < baseX + baseLength) || (baseX >= x && baseX < x + length))
                    {
                        return true;
                    }
                }
                else if (baseY < y - 1)
                {
                    break;
                }
            }
#endif

            return false;
        }
#if NEW
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExpandStrokes(ref int[][] strokes, int position, int count)
        {
            int[][] newstrokes = new int[strokes.Length + count][];

            if (position > 0)
            {
                Array.Copy(strokes, 0, newstrokes, 0, position);
            }

            if (position < strokes.Length)
            {
                Array.Copy(strokes, position, newstrokes, position + count, strokes.Length - position);
            }

            strokes = newstrokes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StrokesIntersect(int x1, int width1, int x2, int width2)
        {
            ////return (x2 >= x1 && x2 < x1 + width1) || (x1 >= x2 && x1 < x2 + width2);
            return x2.Between(x1, x1 + width1) || x1.Between(x2, x2 + width2);
        }

        private static void InsertStroke(ref int[] line, int x, int length)
        {
            if (line == null || line.Length == 0)
            {
                line = new int[] { x, length };
            }
            else
            {
                int x2 = x + length;
                for (int i = 0, ii = line.Length; i < ii; i += 2)
                {
                    int linex1 = line[i];
                    int linelength = line[i + 1];

                    if (x2 < linex1)
                    {
                        // insert before the stroke
                        ConnectedComponent.ExpandLine(ref line, i);
                        line[i] = x;
                        line[i + 1] = length;
                        break;
                    }
                    else if (ConnectedComponent.StrokesIntersect(x, length, linex1, linelength))
                    {
                        int linex2 = linex1 + linelength;

                        // merge with the stroke
                        line[i] = Maximum.Min(x, linex1);
                        line[i + 1] = Maximum.Max(x2, linex2) - line[i];

                        // check whether following strokes intersect with a new expanded one
                        if (x2 > linex2)
                        {
                            linex1 = line[i];
                            linelength = line[i + 1];
                            linex2 = linex1 + linelength;

                            int lastj = -1;
                            for (int j = i + 2; j < ii; j += 2)
                            {
                                if (!ConnectedComponent.StrokesIntersect(linex1, linelength, line[j], line[j + 1]))
                                {
                                    break;
                                }

                                lastj = j;
                            }

                            if (lastj != -1)
                            {
                                line[i + 1] = Maximum.Max(linex2, line[lastj + 1]) - linex1;
                                ConnectedComponent.ShrinkLine(ref line, i + 2, lastj - i);
                            }
                        }

                        break;
                    }
                    else if (i + 2 == ii)
                    {
                        // insert after last stroke
                        ConnectedComponent.ExpandLine(ref line, ii);
                        line[ii] = x;
                        line[ii + 1] = length;
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExpandLine(ref int[] line, int position)
        {
            int[] newline = new int[line.Length + 2];

            if (position > 0)
            {
                Arrays.Copy(position, line, 0, newline, 0);
            }

            if (position < line.Length)
            {
                Arrays.Copy(line.Length - position, line, position, newline, position + 2);
            }

            line = newline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShrinkLine(ref int[] line, int position, int count)
        {
            int[] newline = new int[line.Length - count];

            if (position > 0)
            {
                Arrays.Copy(position, line, 0, newline, 0);
            }

            if (position + count < line.Length)
            {
                Arrays.Copy(line.Length - (position + count), line, position + count, newline, position);
            }

            line = newline;
        }
#else
        private int FindInsertionPoint(int y, int x, int startPosition)
        {
            int low = startPosition;
            int high = this.intervals.Count - 3;

            while (high - low > 4 * 3)
            {
                int mid = (((high + low) / 2) / 3) * 3;
                if (y < this.intervals[mid] || (y == this.intervals[mid] && x < this.intervals[mid + 1]))
                {
                    high = mid;
                }
                else
                {
                    low = mid + 3;
                }
            }

            int pivot = 0;
            for (pivot = low; pivot <= high; pivot += 3)
            {
                if (y < this.intervals[pivot] || (y == this.intervals[pivot] && x < this.intervals[pivot + 1]))
                {
                    break;
                }
            }

            return pivot;
        }

        private void AddSegment(int pivot, int y, int x, int length)
        {
            this.intervals.Insert(pivot, y);
            this.intervals.Insert(pivot + 1, x);
            this.intervals.Insert(pivot + 2, length);

            // update power
            if (this.power != -1)
            {
                this.power += length;
            }
        }
#endif
    }
}
