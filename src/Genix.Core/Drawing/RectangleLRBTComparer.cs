// -----------------------------------------------------------------------
// <copyright file="RectangleLRBTComparer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares two <see cref="Rectangle"/> objects,
    /// first by <see cref="Rectangle.Left"/> then by <see cref="Rectangle.Right"/>, <see cref="Rectangle.Bottom"/> and <see cref="Rectangle.Top"/>.
    /// </summary>
    public class RectangleLRBTComparer : IComparer<Rectangle>
    {
        /// <summary>
        /// Gets a default <see cref="RectangleLRBTComparer"/> comparer.
        /// </summary>
        /// <value>
        /// The <see cref="RectangleLRBTComparer"/> object.
        /// </value>
        public static RectangleLRBTComparer Default { get; } = new RectangleLRBTComparer();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(Rectangle x, Rectangle y)
        {
            int res = x.Left - y.Left;
            if (res == 0)
            {
                res = x.Right - y.Right;
                if (res == 0)
                {
                    res = x.Bottom - y.Bottom;
                    if (res == 0)
                    {
                        res = x.Top - y.Top;
                    }
                }
            }

            return res;
        }
    }
}
