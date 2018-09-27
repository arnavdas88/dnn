// -----------------------------------------------------------------------
// <copyright file="PointYXComparer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares two <see cref="Point"/> objects, first by <see cref="Point.Y"/> then by <see cref="Point.X"/>.
    /// </summary>
    public class PointYXComparer : IComparer<Point>
    {
        /// <summary>
        /// Gets a default <see cref="PointYXComparer"/> comparer.
        /// </summary>
        /// <value>
        /// The <see cref="PointYXComparer"/> object.
        /// </value>
        public static PointYXComparer Default { get; } = new PointYXComparer();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(Point x, Point y)
        {
            int res = x.Y - y.Y;
            if (res == 0)
            {
                res = x.X - y.X;
            }

            return res;
        }
    }
}
