// -----------------------------------------------------------------------
// <copyright file="ISparseKernel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    /// <summary>
    /// Defines a contract for a kernel function that uses sparse vectors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Machine Learning and statistics,
    /// a Kernel is a function that calculates a dot product between the two arguments.
    /// </para>
    /// </remarks>
    public interface ISparseKernel
    {
        /// <summary>
        /// The kernel function.
        /// </summary>
        /// <param name="xidx">The indexes of sparse vector elements.</param>
        /// <param name="x">The values of sparse vector elements.</param>
        /// <param name="y">The input vector <paramref name="y"/>.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        /// <returns>
        /// The output tensor that contains the product of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        float Execute(int[] xidx, float[] x, float[] y, int offy);
    }
}
