// -----------------------------------------------------------------------
// <copyright file="ConnectedComponent.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    public class ConnectedComponent
    {
        private int power;
        private List<int> intervals = new List<int>();

        internal ConnectedComponent()
        {
        }

        internal ConnectedComponent(int y, int x, int length)
        {
            this.AddSegment(y, x, length);
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
                    this.power = 0;
                    for (int i = 0; i < this.intervals.Count; i += 3)
                    {
                        this.power += this.intervals[i + 2];
                    }
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

        internal bool SegmentTouchesBottom(int y, int x, int length)
        {
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

            return false;
        }

        internal void AddSegment(int y, int x, int length)
        {
            // find insertion point
            int pivot = this.FindInsertionPoint(y, x, 0);

            // insert
            this.AddSegment(pivot, y, x, length);
        }

        internal void Merge(ConnectedComponent component)
        {
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
        }

        internal void GetBounds(out int xdst, out int ydst, out int widthdst, out int heightdst)
        {
            xdst = ydst = int.MaxValue;
            widthdst = heightdst = 0;

            int count = this.intervals.Count;
            if (count > 0)
            {
                int y = this.intervals[0];
                int x = this.intervals[1];
                int right = x + this.intervals[2];
                int bottom = y;

                for (int i = 3; i < count; i += 3)
                {
                    bottom = this.intervals[i];

                    int x1 = this.intervals[i + 1];
                    int right1 = x1 + this.intervals[i + 2];

                    if (x > x1)
                    {
                        x = x1;
                    }

                    if (right < right1)
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
    }
}
