// -----------------------------------------------------------------------
// <copyright file="BoundedObjectGrid.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;

    /// <summary>
    /// Represents a grid that holds a collection of <see cref="IBoundedObject"/> objects and provides a fast access to those objects.
    /// </summary>
    public class BoundedObjectGrid
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedObjectGrid"/> class.
        /// </summary>
        /// <param name="gridSize">The size of each cell, in pixels.</param>
        /// <param name="bounds">The grid bounding box.</param>
        public BoundedObjectGrid(int gridSize, Rectangle bounds)
        {
            this.GridSize = gridSize;
            this.Bounds = bounds;

            this.GridWidth = (bounds.Width + gridSize - 1) / gridSize;
            this.GridHeight = (bounds.Height + gridSize - 1) / gridSize;
            this.NumberOfCells = this.GridWidth * this.GridHeight;
        }

        /// <summary>
        /// Gets the size of each cell, in pixels.
        /// </summary>
        /// <value>
        /// The size of each cell, in pixels.
        /// </value>
        public int GridSize { get; }

        /// <summary>
        /// Gets the grid bounding box.
        /// </summary>
        /// <value>
        /// The grid bounding box.
        /// </value>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Gets the width of the grid, in cells.
        /// </summary>
        /// <value>
        /// The width of the grid, in cells.
        /// </value>
        public int GridWidth { get; }

        /// <summary>
        /// Gets the height of the grid, in cells.
        /// </summary>
        /// <value>
        /// The height of the grid, in cells.
        /// </value>
        public int GridHeight { get; }

        /// <summary>
        /// Gets the total number of cells in the grid.
        /// </summary>
        /// <value>
        /// <see cref="GridWidth"/> * <see cref="GridHeight"/>.
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
            xgrid = ((x - this.Bounds.X) / this.GridSize).Clip(0, this.GridWidth - 1);
            ygrid = ((y - this.Bounds.Y) / this.GridSize).Clip(0, this.GridHeight - 1);
        }

        /// <summary>
        /// Adds the bounded object to the grid.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <param name="spreadHorizontally">If <b>true</b> all the cells covered horizontally by the object are used; otherwise, the object is added to the top-left cell.</param>
        /// <param name="spreadVertically">If <b>true</b> all the cells covered vertically by the object are used; otherwise, the object is added to the top-left cell.</param>
        public void Add(IBoundedObject obj, bool spreadHorizontally, bool spreadVertically)
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

            for (int y = starty, index = starty * this.GridWidth; y <= endy; y++, index += this.GridWidth)
            {
                for (int x = startx; x <= endx; x++)
                {
                    ////grid_[grid_index + x].add_sorted(SortByBoxLeft<BBC>, true, bbox);
                }
            }
        }
    }
}
