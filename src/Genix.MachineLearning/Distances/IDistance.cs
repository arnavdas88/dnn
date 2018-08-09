// -----------------------------------------------------------------------
// <copyright file="IDistance.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Distances
{
    /// <summary>
    /// Defines a contract for measuring distance between two points.
    /// </summary>
    /// <typeparam name="TPoint1">The type of the first point.</typeparam>
    /// <typeparam name="TPoint2">The type of the second point.</typeparam>
    /// <typeparam name="TDistance">The type of the measured distance.</typeparam>
    public interface IDistance<in TPoint1, in TPoint2, out TDistance>
    {
        /// <summary>
        /// Computes the distance between points <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The first point <paramref name="x"/>.</param>
        /// <param name="y">The second point <paramref name="y"/>.</param>
        /// <returns>
        /// A value that represents the distance between <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        TDistance Distance(TPoint1 x, TPoint2 y);
    }
}
