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
    /// <typeparam name="TPoint">The type of the point.</typeparam>
    /// <typeparam name="TDistance">The type of the measured distance.</typeparam>
    public interface IDistance<in TPoint, out TDistance>
    {
        /// <summary>
        /// Computes the distance between points <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The first point <paramref name="x"/>.</param>
        /// <param name="y">The second point <paramref name="y"/>.</param>
        /// <returns>
        /// A value that represents the distance between <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        TDistance Distance(TPoint x, TPoint y);
    }
}
