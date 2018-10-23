// -----------------------------------------------------------------------
// <copyright file="BoundedObjectGrid.cs" company="Noname, Inc.">
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
    /// Represents a grid that holds a collection of <see cref="IBoundedObject"/> objects and provides a fast access to those objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the grid.</typeparam>
    public class BoundedObjectGrid<T>
        where T : class, IBoundedObject
    {
        private readonly SortedList<Rectangle, T>[][] cells;
        private readonly IComparer<Rectangle> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedObjectGrid{T}"/> class.
        /// </summary>
        /// <param name="bounds">The grid bounding box.</param>
        /// <param name="cellWidth">The width of each cell, in pixels.</param>
        /// <param name="cellHeight">The height of each cell, in pixels.</param>
        /// <param name="comparer">The comparer used to sort object withing their cells.</param>
        public BoundedObjectGrid(Rectangle bounds, int cellWidth, int cellHeight, IComparer<Rectangle> comparer)
        {
            this.Bounds = bounds;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;

            this.Width = (bounds.Width + cellWidth - 1) / cellWidth;
            this.Height = (bounds.Height + cellHeight - 1) / cellHeight;
            this.NumberOfCells = this.Width * this.Height;

            this.cells = JaggedArray.Create<SortedList<Rectangle, T>>(this.Height, this.Width);
            this.comparer = comparer;
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
        /// Finds the cell that contains the given point.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <param name="xgrid">The x-coordinate of the found cell.</param>
        /// <param name="ygrid">The y-coordinate of the found cell.</param>
        public void FindCell(int x, int y, out int xgrid, out int ygrid)
        {
            xgrid = ((x - this.Bounds.X) / this.CellWidth).Clip(0, this.Width - 1);
            ygrid = ((y - this.Bounds.Y) / this.CellHeight).Clip(0, this.Height - 1);
        }

        /// <summary>
        /// Adds the bounded object to the grid.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <param name="spreadHorizontally">If <b>true</b> all the cells covered horizontally by the object are used; otherwise, the object is added to the top-left cell.</param>
        /// <param name="spreadVertically">If <b>true</b> all the cells covered vertically by the object are used; otherwise, the object is added to the top-left cell.</param>
        public void Add(T obj, bool spreadHorizontally, bool spreadVertically)
        {
            Rectangle bounds = obj.Bounds;

            this.FindCell(bounds.X, bounds.Y, out int startx, out int starty);
            this.FindCell(bounds.Right - 1, bounds.Bottom - 1, out int endx, out int endy);
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
                SortedList<Rectangle, T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    SortedList<Rectangle, T> list = lists[x];
                    if (list == null)
                    {
                        lists[x] = list = new SortedList<Rectangle, T>(this.comparer);
                    }

                    list.Add(obj.Bounds, obj);
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
        public IEnumerable<T> GetObjects()
        {
            return this.GetObjects(0, 0, this.Width - 1, this.Height - 1);
        }

        /// <summary>
        /// Returns all object in this <see cref="BoundedObjectGrid{T}"/> that intersect the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>
        /// The sequence of objects found inside the specified bounds.
        /// </returns>
        public IEnumerable<T> GetObjects(Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                return Enumerable.Empty<T>();
            }

            this.FindCell(bounds.X, bounds.Y, out int startx, out int starty);
            this.FindCell(bounds.Right - 1, bounds.Bottom - 1, out int endx, out int endy);

            return this.GetObjects(startx, starty, endx, endy);
        }

        private IEnumerable<T> GetObjects(int startx, int starty, int endx, int endy)
        {
            for (int y = starty; y <= endy; y++)
            {
                SortedList<Rectangle, T>[] lists = this.cells[y];
                for (int x = startx; x <= endx; x++)
                {
                    SortedList<Rectangle, T> list = lists[x];
                    if (list != null)
                    {
                        foreach (KeyValuePair<Rectangle, T> kvp in list)
                        {
                            // validate that we are in object's starting cell
                            // otherwise the object that spread across multiple cells has already been returned from other cell
                            this.FindCell(kvp.Key.X, kvp.Key.Y, out int firstx, out int firsty);
                            if (x == MinMax.Max(firstx, startx) && y == MinMax.Max(firsty, starty))
                            {
                                yield return kvp.Value;
                            }
                        }
                    }
                }
            }
        }
    }
}
