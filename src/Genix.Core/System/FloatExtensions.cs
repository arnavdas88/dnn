// -----------------------------------------------------------------------
// <copyright file="FloatExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for the <see cref="float"/> class.
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Determines whether this instance falls within the range of specified single-precision floating point numbers.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// <b>true</b> if this instance is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this float value, float lowerBound, float upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }

        /// <summary>
        /// Clips this instance to a specified minimum and maximum value.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// A value that is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clip(this float value, float lowerBound, float upperBound)
        {
            if (value < lowerBound)
            {
                return lowerBound;
            }
            else if (value > upperBound)
            {
                return upperBound;
            }
            else
            {
                return value;
            }
        }
    }
}
