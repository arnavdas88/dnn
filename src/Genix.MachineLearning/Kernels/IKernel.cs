// -----------------------------------------------------------------------
// <copyright file="IKernel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    /// <summary>
    /// Defines a contrect for a kernel function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Machine Learning and statistics,
    /// a Kernel is a function that calculates a dot product between the two arguments.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of function arguments.</typeparam>
    public interface IKernel<in T>
    {
        /// <summary>
        /// The kernel function.
        /// </summary>
        /// <param name="x">The inpunt vector <paramref name="x"/>.</param>
        /// <param name="y">The inpunt vector <paramref name="y"/>.</param>
        /// <returns>
        /// The dot product of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        float Execute(T x, T y);
    }
}
