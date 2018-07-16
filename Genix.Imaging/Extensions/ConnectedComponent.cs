// -----------------------------------------------------------------------
// <copyright file="ConnectedComponent.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    public class ConnectedComponent
    {
        private static readonly int[][] EmptyStrokes = new int[0][];

        private readonly List<int> intervals = new List<int>();
        ////private readonly Rectangle position = new Rectangle();
        private int[][] strokes = ConnectedComponent.EmptyStrokes;
        private int power;

        internal ConnectedComponent()
        {
        }

        internal ConnectedComponent(int y, int x, int length)
        {
            this.AddStroke(y, x, length);
        }

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The x-coordinate of the upper-left corner of this <see cref="ConnectedComponent"/>.
        /// </value>
        public int X { get; private set; }

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The y-coordinate of the upper-left corner of this <see cref="ConnectedComponent"/>.
        /// </value>
        public int Y { get; private set; }

        /// <summary>
        /// Gets the width of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The width of this <see cref="ConnectedComponent"/>.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The height of this <see cref="ConnectedComponent"/>.
        /// </value>
        public int Height { get; private set; }

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

                    List<int> values = this.intervals;
                    for (int i = 0, ii = values.Count; i < ii; i += 3)
                    {
                        sum += values[i + 2];
                    }

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
        public Rectangle Bounds
        {
            get
            {
                this.GetBounds(out int x, out int y, out int width, out int height);
                return new Rectangle(x, y, width, height);
            }
        }

        internal IList<int> Intervals => this.intervals;

        /// <summary>
        /// Adds a stroke to this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="y">The y-coordinate of the stroke.</param>
        /// <param name="x">The x-coordinate of the stroke.</param>
        /// <param name="length">The length of the stroke.</param>
        public void AddStroke(int y, int x, int length)
        {
            // find insertion point
            int pivot = this.FindInsertionPoint(y, x, 0);

            // insert
            this.AddSegment(pivot, y, x, length);

            //// NEW
            // insert new horizontal line
            if (this.strokes.Length == 0)
            {
                this.strokes = new int[1][];

                this.Y = y;
                this.Height = 1;
            }
            else if (y < this.Y)
            {
                // resize the array
                int diff = this.Y - y;
                int[][] newstrokes = new int[this.Height + diff][];
                Array.Copy(this.strokes, 0, newstrokes, diff, this.Height);
                this.strokes = newstrokes;

                this.Y = y;
                this.Height += diff;
            }
            else if (y >= this.Y + this.Height)
            {
                // resize the array
                int diff = y - (this.Y + this.Height) + 1;
                int[][] newstrokes = new int[this.Height + diff][];
                Array.Copy(this.strokes, 0, newstrokes, 0, this.Height);
                this.strokes = newstrokes;

                this.Height += diff;
            }

            // insert stroke into the line
            insertStrokeIntoLine(ref this.strokes[y - this.Y]);

            //// update position

            void insertStrokeIntoLine(ref int[] line)
            {
                if (line == null || line.Length == 0)
                {
                    line = new int[] { x, length };
                }
                else
                {
                    int right = x + length;
                    for (int i = 0, ii = line.Length; i < ii; i += 2)
                    {
                        int linex = line[i];
                        int lineright = linex + line[i + 1];

                        if (right < linex)
                        {
                            // insert before the stroke
                            expandLine(ref line, i);
                            break;
                        }
                        else if (ConnectedComponent.StrokesIntersect(x, right, linex, lineright))
                        {
                            // merge with the stroke
                            line[i] = Maximum.Min(x, linex);
                            line[i + 1] = Maximum.Max(right, lineright) - line[i];

                            // check whether following strokes intersect with a new expanded one
                            if (right > lineright)
                            {
                                linex = line[i];
                                lineright = linex + line[i + 1];

                                int lastj = -1;
                                for (int j = i + 2; j < ii; j += 2)
                                {
                                    if (!ConnectedComponent.StrokesIntersect(linex, lineright, line[j], line[j + 1]))
                                    {
                                        break;
                                    }

                                    lastj = j;
                                }

                                if (lastj != -1)
                                {
                                    line[i + 1] = Maximum.Max(lineright, line[lastj + 1]) - linex;
                                    shrinkLine(ref line, i + 2, lastj - i);
                                }
                            }

                            break;
                        }
                        else if (i + 2 == ii)
                        {
                            // insert after last stroke
                            expandLine(ref line, ii);
                            break;
                        }
                    }
                }

                this.X = line[0];
                this.Width = line[line.Length - 2] + line[line.Length - 1] - this.X;
            }

            void expandLine(ref int[] line, int position)
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

                line[position] = x;
                line[position + 1] = length;
            }

            void shrinkLine(ref int[] line, int position, int count)
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
        }

        /// <summary>
        /// Merges this <see cref="ConnectedComponent"/> with the specified <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to merge with.</param>
        public void MergeWith(ConnectedComponent component)
        {
            int pivot = 0;
            List<int> values = component.intervals;
            for (int i = 0, ii = values.Count; i < ii; i += 3)
            {
                int y = values[i];
                int x = values[i + 1];
                int length = values[i + 2];

                // find insertion point
                pivot = this.FindInsertionPoint(y, x, pivot);

                // insert
                this.AddSegment(pivot, y, x, length);
            }
        }

        internal bool SegmentTouchesBottom(int y, int x, int length)
        {
            List<int> values = this.intervals;
            for (int i = values.Count - 3; i >= 0; i -= 3)
            {
                int baseY = values[i];

                if (baseY == y - 1)
                {
                    if (ConnectedComponent.StrokesIntersect(values[i + 1], values[i + 2], x, length))
                    {
                        return true;
                    }
                }
                else if (baseY < y - 1)
                {
                    break;
                }
            }

            return false;
        }

        internal void GetBounds(out int xdst, out int ydst, out int widthdst, out int heightdst)
        {
            xdst = ydst = widthdst = heightdst = 0;

            List<int> values = this.intervals;
            int count = values.Count;
            if (count > 0)
            {
                int y = values[0];
                int x = values[1];
                int right = x + values[2];
                int bottom = y;

                for (int i = 3; i < count; i += 3)
                {
                    bottom = values[i];

                    int x1 = values[i + 1];
                    if (x1 < x)
                    {
                        x = x1;
                    }

                    int right1 = x1 + values[i + 2];
                    if (right1 > right)
                    {
                        right = right1;
                    }
                }

                xdst = x;
                ydst = y;
                widthdst = right - x;
                heightdst = bottom - y + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StrokesIntersect(int x1, int length1, int x2, int length2)
        {
            return (x2 >= x1 && x2 < x1 + length1) || (x1 >= x2 && x1 < x2 + length2);
        }

        private int FindInsertionPoint(int y, int x, int startPosition)
        {
            List<int> values = this.intervals;
            int low = startPosition;
            int high = values.Count - 3;

            while (high - low > 4 * 3)
            {
                int mid = (((high + low) / 2) / 3) * 3;
                if (y < values[mid] || (y == values[mid] && x < values[mid + 1]))
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
                if (y < values[pivot] || (y == values[pivot] && x < values[pivot + 1]))
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
    }
}
