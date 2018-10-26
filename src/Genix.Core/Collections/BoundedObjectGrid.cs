// -----------------------------------------------------------------------
// <copyright file="BoundedObjectGrid.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using Genix.Drawing;

    /// <summary>
    /// Represents a grid that holds a collection of <see cref="IBoundedObject"/> objects and provides a fast access to those objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the grid.</typeparam>
    public class BoundedObjectGrid<T>
        where T : class, IBoundedObject
    {
        private readonly List<T>[][] cells;
        private int version = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedObjectGrid{T}"/> class.
        /// </summary>
        /// <param name="bounds">The grid bounding box.</param>
        /// <param name="cellWidth">The width of each cell, in pixels.</param>
        /// <param name="cellHeight">The height of each cell, in pixels.</param>
        public BoundedObjectGrid(Rectangle bounds, int cellWidth, int cellHeight)
        {
            this.Bounds = bounds;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;

            this.Width = (bounds.Width + cellWidth - 1) / cellWidth;
            this.Height = (bounds.Height + cellHeight - 1) / cellHeight;
            this.NumberOfCells = this.Width * this.Height;

            this.cells = JaggedArray.Create<List<T>>(this.Height, this.Width);
        }

        /// <summary>
        /// Gets the grid bounding box.
        /// </summary>
        /// <value>
        /// The grid bounding box.
        /// </value>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Gets the width of each cell, in pixels.
        /// </summary>
        /// <value>
        /// The width of each cell, in pixels.
        /// </value>
        public int CellWidth { get; }

        /// <summary>
        /// Gets the height of each cell, in pixels.
        /// </summary>
        /// <value>
        /// The height of each cell, in pixels.
        /// </value>
        public int CellHeight { get; }

        /// <summary>
        /// Gets the width of the grid, in cells.
        /// </summary>
        /// <value>
        /// The width of the grid, in cells.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the grid, in cells.
        /// </summary>
        /// <value>
        /// The height of the grid, in cells.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the total number of cells in the grid.
        /// </summary>
        /// <value>
        /// <see cref="Width"/> * <see cref="Height"/>.
        /// </value>
        public int NumberOfCells { get; }

        /// <summary>
        /// Removes all objects from the grid.
        /// </summary>
        public void Clear()
        {
            for (int y = 0, w = this.Width, h = this.Height; y < h; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = 0; x < w; x++)
                {
                    lists[x] = null;
                }
            }

            this.version++;
        }

        /// <summary>
        /// Determines whether the specified area does not contain any objects.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// <b>true</b> if the specified area does not intersect with any object in the grid; otherwise, <b>false</b>.
        /// </returns>
        public bool IsEmpty(Rectangle bounds)
        {
            this.FindCells(bounds, out int startx, out int starty, out int endx, out int endy);

            for (int y = starty; y <= endy; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    List<T> list = lists[x];
                    if (list != null)
                    {
                        foreach (T o in list)
                        {
                            if (o.Bounds.IntersectsWith(bounds))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the bounded object to the grid.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <param name="spreadHorizontally">If <b>true</b> all the cells covered horizontally by the object are used; otherwise, the object is added to the top-left cell.</param>
        /// <param name="spreadVertically">If <b>true</b> all the cells covered vertically by the object are used; otherwise, the object is added to the top-left cell.</param>
        public void Add(T obj, bool spreadHorizontally, bool spreadVertically)
        {
            this.FindCells(obj.Bounds, out int startx, out int starty, out int endx, out int endy);

            if (!spreadHorizontally)
            {
                endx = startx;
            }

            if (!spreadVertically)
            {
                endy = starty;
            }

            for (int y = starty; y <= endy; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    List<T> list = lists[x];
                    if (list == null)
                    {
                        lists[x] = list = new List<T>();
                    }

                    list.Add(obj);
                }
            }

            this.version++;
        }

        /// <summary>
        /// Removes the bounded object from the grid.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
        public void Remove(T obj)
        {
            this.FindCells(obj.Bounds, out int startx, out int starty, out int endx, out int endy);

            for (int y = starty; y <= endy; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    bool removed = false;

                    List<T> list = lists[x];
                    if (list != null)
                    {
                        removed = list.Remove(obj);

                        if (removed)
                        {
                            this.version++;

                            if (list.Count == 0)
                            {
                                // remove collection if cell does not contain any more elements
                                lists[x] = null;
                            }
                        }
                    }

                    // stop searching if the object was not spread across multiple cells
                    if (!removed && (y > starty || x > startx))
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the collection of bounded object to the grid.
        /// </summary>
        /// <param name="obj">The objects to add.</param>
        /// <param name="spreadHorizontally">If <b>true</b> all the cells covered horizontally by the object are used; otherwise, the object is added to the top-left cell.</param>
        /// <param name="spreadVertically">If <b>true</b> all the cells covered vertically by the object are used; otherwise, the object is added to the top-left cell.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <b>null</b>.
        /// </exception>
        public void Add(IEnumerable<T> obj, bool spreadHorizontally, bool spreadVertically)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            foreach (T o in obj)
            {
                this.Add(o, spreadHorizontally, spreadVertically);
            }
        }

        /// <summary>
        /// Returns all object in this <see cref="BoundedObjectGrid{T}"/>.
        /// </summary>
        /// <returns>
        /// The sequence of objects.
        /// </returns>
        public IEnumerable<T> EnumObjects()
        {
            int oldversion = this.version;

            for (int y = 0, w = this.Width, h = this.Height; y < h; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = 0; x < w; x++)
                {
                    List<T> list = lists[x];
                    if (list != null)
                    {
                        foreach (T o in list)
                        {
                            if (oldversion != this.version)
                            {
                                throw new InvalidOperationException("The grid was modified during enumeration.");
                            }

                            // validate that we are in object's starting cell
                            // otherwise the object that spread across multiple cells has already been returned from other cell
                            Rectangle bounds = o.Bounds;
                            this.FindCell(bounds.X, bounds.Y, out int startx, out int starty);
                            if (x == startx && y == starty)
                            {
                                yield return o;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all object in this <see cref="BoundedObjectGrid{T}"/> that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// The sequence of objects that intersect the specified bounds.
        /// </returns>
        public IEnumerable<T> EnumObjects(Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                yield break;
            }

            this.FindCells(bounds, out int startx, out int starty, out int endx, out int endy);

            int oldversion = this.version;
            for (int y = starty; y <= endy; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    List<T> list = lists[x];
                    if (list != null)
                    {
                        foreach (T o in list)
                        {
                            if (oldversion != this.version)
                            {
                                throw new InvalidOperationException("The grid was modified during enumeration.");
                            }

                            Rectangle obounds = o.Bounds;
                            if (obounds.IntersectsWith(bounds))
                            {
                                // validate that we are in object's starting cell
                                // otherwise the object that spread across multiple cells has already been returned from other cell
                                this.FindCell(obounds.X, obounds.Y, out int firstx, out int firsty);
                                if (x == MinMax.Max(firstx, startx) && y == MinMax.Max(firsty, starty))
                                {
                                    yield return o;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds an object in this <see cref="BoundedObjectGrid{T}"/> that contains the specified area.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// The object that contains the specified area; <b>null</b>, if no such object exists.
        /// </returns>
        public T FindContainer(Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                return null;
            }

            this.FindCells(bounds, out int startx, out int starty, out int endx, out int endy);

            int oldversion = this.version;
            for (int y = starty; y <= endy; y++)
            {
                List<T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    List<T> list = lists[x];
                    if (list != null)
                    {
                        foreach (T o in list)
                        {
                            if (oldversion != this.version)
                            {
                                throw new InvalidOperationException("The grid was modified during enumeration.");
                            }

                            if (o.Bounds.Contains(bounds))
                            {
                                return o;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Removes objects from the grid that are completely contained by other objects.
        /// </summary>
        /// <returns>
        /// The number of objects removed.
        /// </returns>
        public int Compact()
        {
            int count = 0;

            for (int y0 = 0, w = this.Width, h = this.Height; y0 < h; y0++)
            {
                List<T>[] lists0 = this.cells[y0];
                for (int x0 = 0; x0 < w; x0++)
                {
                    List<T> list0 = lists0[x0];
                    if (list0 != null)
                    {
                        for (int i0 = 0; i0 < list0.Count; i0++)
                        {
                            T o0 = list0[i0];
                            Rectangle o0bounds = o0.Bounds;
                            this.FindCells(o0bounds, out int startx, out int starty, out int endx, out int endy);

                            if (FindContainer())
                            {
                                // remove from this cell
                                list0.RemoveAt(i0);
                                i0--;
                                count++;

                                // remove from other cells
                                if (endx > startx || endy > starty)
                                {
                                    Remove();
                                }
                            }

                            bool FindContainer()
                            {
                                for (int y1 = starty; y1 <= endy; y1++)
                                {
                                    List<T>[] lists1 = this.cells[y1];
                                    for (int x1 = startx; x1 <= endx; x1++)
                                    {
                                        List<T> list1 = lists1[x1];
                                        if (list1 != null)
                                        {
                                            for (int i1 = 0; i1 < list1.Count; i1++)
                                            {
                                                T o1 = list1[i1];
                                                if (o1 != o0 && o1.Bounds.Contains(o0.Bounds))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            void Remove()
                            {
                                for (int y1 = starty; y1 <= endy; y1++)
                                {
                                    List<T>[] lists1 = this.cells[y1];
                                    for (int x1 = startx; x1 <= endx; x1++)
                                    {
                                        bool removed = false;

                                        List<T> list1 = lists1[x1];
                                        if (list1 != null)
                                        {
                                            removed = list1.Remove(o0);

                                            if (removed)
                                            {
                                                count++;

                                                if (list1.Count == 0)
                                                {
                                                    // remove collection if cell does not contain any more elements
                                                    lists1[x1] = null;
                                                }
                                            }
                                        }

                                        // stop searching if the object was not spread across multiple cells
                                        if (!removed && (y1 > starty || x1 > startx))
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Counts objects in each cell.
        /// </summary>
        /// <returns>
        /// The two-dimensional array; each cells contains the number of objects in the corresponding cell of the grid.
        /// </returns>
        public int[][] CountObjects()
        {
            int[][] counts = JaggedArray.Create<int>(this.Height, this.Width);

            for (int y = 0, w = this.Width, h = this.Height; y < h; y++)
            {
                List<T>[] lists = this.cells[y];
                int[] linecounts = counts[y];
                for (int x = 0; x < w; x++)
                {
                    List<T> list = lists[x];
                    linecounts[x] = list != null ? list.Count : 0;
                }
            }

            return counts;
        }

        /// <summary>
        /// Finds the cell that contains the given point.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <param name="xgrid">The x-coordinate of the found cell.</param>
        /// <param name="ygrid">The y-coordinate of the found cell.</param>
        private void FindCell(int x, int y, out int xgrid, out int ygrid)
        {
            xgrid = ((x - this.Bounds.X) / this.CellWidth).Clip(0, this.Width - 1);
            ygrid = ((y - this.Bounds.Y) / this.CellHeight).Clip(0, this.Height - 1);
        }

        /// <summary>
        /// Finds the range of cells that contains the given area.
        /// </summary>
        /// <param name="area">The area to test.</param>
        /// <param name="startx">The x-coordinate of the found top-left cell.</param>
        /// <param name="starty">The y-coordinate of the found top-left cell.</param>
        /// <param name="endx">The x-coordinate of the found bottom-right cell.</param>
        /// <param name="endy">The y-coordinate of the found bottom-right cell.</param>
        private void FindCells(Rectangle area, out int startx, out int starty, out int endx, out int endy)
        {
            startx = ((area.X - this.Bounds.X) / this.CellWidth).Clip(0, this.Width - 1);
            starty = ((area.Y - this.Bounds.Y) / this.CellHeight).Clip(0, this.Height - 1);

            endx = ((area.Right - 1 - this.Bounds.X) / this.CellWidth).Clip(0, this.Width - 1);
            endy = ((area.Bottom - 1 - this.Bounds.Y) / this.CellHeight).Clip(0, this.Height - 1);
        }
    }
}
