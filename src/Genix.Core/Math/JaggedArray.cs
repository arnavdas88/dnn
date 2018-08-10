// -----------------------------------------------------------------------
// <copyright file="JaggedArray.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Provides methods for manipulating jagged arrays.
    /// </summary>
    public static class JaggedArray
    {
        /// <summary>
        /// Creates a jagged array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="rows">The number of rows in the array.</param>
        /// <param name="columns">The number of columns in the array.</param>
        /// <returns>
        /// The array of the requested size.
        /// </returns>
        public static T[][] Create<T>(int rows, int columns)
        {
            T[][] matrix = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new T[columns];
            }

            return matrix;
        }
    }
}
