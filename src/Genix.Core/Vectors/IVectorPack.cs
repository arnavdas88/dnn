// -----------------------------------------------------------------------
// <copyright file="IVectorPack.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Defines a contract for a packed collection of vectors.
    /// </summary>
    /// <typeparam name="T">The type of vector elements.</typeparam>
    public interface IVectorPack<T>
    {
        /// <summary>
        /// Gets the number of vectors in <see cref="X"/>.
        /// </summary>
        /// <value>
        /// The number of vectors.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the length of each vector in <see cref="X"/>.
        /// </summary>
        /// <value>
        /// The length of each vector.
        /// </value>
        int Length { get; }

        /// <summary>
        /// Gets the packed vectors.
        /// </summary>
        /// <value>
        /// The packed vectors.
        /// </value>
        /// <remarks>
        /// The total number of vectors contained in this property is <see cref="Count"/> * <see cref="Length"/>.
        /// </remarks>
        float[] X { get; }
    }
}
