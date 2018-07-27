// -----------------------------------------------------------------------
// <copyright file="Int32Extensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for the <see cref="Int32"/> class.
    /// </summary>
    public static class Int32Extensions
    {
        /// <summary>
        /// Determines whether this instance falls within the range of specified 32-bit signed integers.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// <b>true</b> if this instance is greater than or equal to <c>lowerBound</c> and less than or equal to <c>upperBound</c>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this int value, int lowerBound, int upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }
    }
}
