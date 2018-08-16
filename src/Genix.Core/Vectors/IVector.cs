// -----------------------------------------------------------------------
// <copyright file="IVector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Defines a contract for a vector.
    /// </summary>
    /// <typeparam name="T">The type of vector elements.</typeparam>
    public interface IVector<T>
    {
        /// <summary>
        /// Gets the vector length.
        /// </summary>
        /// <value>
        /// The number of elements in vector.
        /// </value>
        int Length { get; }

        /// <summary>
        /// Copies vector values to an array starting at the specified destination index.
        /// </summary>
        /// <param name="y">The destination dense vector.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        void Copy(T[] y, int offy);

        /// <summary>
        /// Determines whether this vector contains the same data as a dense vector.
        /// </summary>
        /// <param name="y">The dense vector to compare.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// <b>true</b> if two vectors contain same data; otherwise, <b>false</b>.
        /// </returns>
        bool Equals(float[] y, int offy);

        /// <summary>
        /// Adds a constant value to each element of the vector.
        /// </summary>
        /// <param name="alpha">The scalar to add.</param>
        void AddC(T alpha);

        /// <summary>
        /// Subtracts a constant value from each element of the vector.
        /// </summary>
        /// <param name="alpha">The scalar to subtract.</param>
        void SubC(T alpha);

        /// <summary>
        /// Multiplies each element of the vector by a constant value.
        /// </summary>
        /// <param name="alpha">The scalar to multiply.</param>
        void MulC(T alpha);

        /// <summary>
        /// Divides each element of the vector by a constant value.
        /// </summary>
        /// <param name="alpha">The scalar to divide.</param>
        void DivC(T alpha);

        /// <summary>
        /// Adds a product of each element of the vector and a constant to the elements of dense vector.
        /// </summary>
        /// <param name="alpha">The scalar to multiply.</param>
        /// <param name="y">The destination dense vector.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>y[i] += this[i] * alpha</c>.
        /// </remarks>
        void AddProductC(float alpha, float[] y, int offy);

        /// <summary>
        /// Computes the sum of elements of the vector.
        /// </summary>
        /// <returns>
        /// The sum of elements of the vector.
        /// </returns>
        float Sum();

        /// <summary>
        /// Computes the Manhattan distance between elements of this vector and a dense vector.
        /// </summary>
        /// <param name="y">The dense vector <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Manhattan distance between elements of two vectors.
        /// </returns>
        float ManhattanDistance(T[] y, int offy);

        /// <summary>
        /// Computes the Euclidean distance between elements of this vector and a dense vector.
        /// </summary>
        /// <param name="y">The dense vector <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The Euclidean distance between elements of two vectors.
        /// </returns>
        float EuclideanDistance(T[] y, int offy);
    }
}
