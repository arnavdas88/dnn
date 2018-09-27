// -----------------------------------------------------------------------
// <copyright file="RectangleLTRBComparer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares two <see cref="Rectangle"/> objects,
    /// first by <see cref="Rectangle.Left"/> then by <see cref="Rectangle.Top"/>, <see cref="Rectangle.Right"/> and <see cref="Rectangle.Bottom"/>.
    /// </summary>
    public class RectangleLTRBComparer : IComparer<Rectangle>
    {
        /// <summary>
        /// Gets a default <see cref="RectangleLTRBComparer"/> comparer.
        /// </summary>
        /// <value>
        /// The <see cref="RectangleLTRBComparer"/> object.
        /// </value>
        public static RectangleLTRBComparer Default { get; } = new RectangleLTRBComparer();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(Rectangle x, Rectangle y)
        {
            int res = x.Left - y.Left;
            if (res == 0)
            {
                res = x.Top - y.Top;
                if (res == 0)
                {
                    res = x.Right - y.Right;
                    if (res == 0)
                    {
                        res = x.Bottom - y.Bottom;
                    }
                }
            }

            return res;
        }
    }
}
