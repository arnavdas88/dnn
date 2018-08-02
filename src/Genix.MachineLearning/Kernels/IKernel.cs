// -----------------------------------------------------------------------
// <copyright file="IKernel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    /// <summary>
    /// Defines a contract for a kernel function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Machine Learning and statistics,
    /// a Kernel is a function that calculates a dot product between the two arguments.
    /// </para>
    /// </remarks>
    public interface IKernel
    {
        /// <summary>
        /// The kernel function.
        /// </summary>
        /// <param name="length">The number of elements in vectors.</param>
        /// <param name="x">The input vector <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The input vector <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The output tensor that contains the product of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        float Execute(int length, float[] x, int offx, float[] y, int offy);
    }
}
