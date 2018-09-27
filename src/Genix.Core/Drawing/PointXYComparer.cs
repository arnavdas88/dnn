// -----------------------------------------------------------------------
// <copyright file="PointXYComparer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares two <see cref="Point"/> objects, first by <see cref="Point.X"/> then by <see cref="Point.Y"/>.
    /// </summary>
    public class PointXYComparer : IComparer<Point>
    {
        /// <summary>
        /// Gets a default <see cref="PointXYComparer"/> comparer.
        /// </summary>
        /// <value>
        /// The <see cref="PointXYComparer"/> object.
        /// </value>
        public static PointXYComparer Default { get; } = new PointXYComparer();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(Point x, Point y)
        {
            int res = x.X - y.X;
            if (res == 0)
            {
                res = x.Y - y.Y;
            }

            return res;
        }
    }
}
