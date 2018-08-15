// -----------------------------------------------------------------------
// <copyright file="IClassifierProgress.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;

    /// <summary>
    /// Defines the interface for the classification progress reporting.
    /// </summary>
    /// <typeparam name="T">The type of the data to classify.</typeparam>
    public interface IClassifierProgress<T>
    {
        /// <summary>
        /// Reports that the data is about to be classified.
        /// </summary>
        /// <param name="source">The data to classify.</param>
        /// <param name="index">The zero-based index of the data in the sequence.</param>
        void OnClassifying(T source, int index);

        /// <summary>
        /// Reports that the data is classified.
        /// </summary>
        /// <param name="source">The data to classify.</param>
        /// <param name="index">The zero-based index of the data in the sequence.</param>
        /// <param name="answer">The classification answer. <b>null</b> in learning mode.</param>
        /// <param name="exception">The exception that might have occurred during classification.</param>
        void OnClassified(T source, int index, Answer answer, Exception exception);
    }
}
