// -----------------------------------------------------------------------
// <copyright file="IVectorDistance.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Distances
{
    /// <summary>
    /// Defines a contract for measuring distance between two vectors.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the vectors.</typeparam>
    /// <typeparam name="TDistance">The type of the measured distance.</typeparam>
    public interface IVectorDistance<in T, out TDistance>
    {
        /// <summary>
        /// Computes the distance between vectors <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The first vector.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The second vector.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// A value that represents the distance between <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        TDistance Distance(int length, T[] x, int offx, T[] y, int offy);
    }
}
