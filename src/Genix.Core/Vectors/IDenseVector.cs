// -----------------------------------------------------------------------
// <copyright file="IDenseVector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Defines a contract for a dense vector.
    /// </summary>
    /// <typeparam name="T">The type of vector elements.</typeparam>
    public interface IDenseVector<T> : IVector<T>
    {
        /// <summary>
        /// Gets the vector elements.
        /// </summary>
        /// <value>
        /// The array that contains vector elements.
        /// </value>
        float[] X { get; }

        /// <summary>
        /// Gets the staring position of vector elements in <see cref="X"/>.
        /// </summary>
        /// <value>
        /// The zero-based staring position of vector elements in <see cref="X"/>.
        /// </value>
        int Offset { get; }
    }
}
