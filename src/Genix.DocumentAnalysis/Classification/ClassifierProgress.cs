// -----------------------------------------------------------------------
// <copyright file="ClassifierProgress.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;

    /// <summary>
    /// Provides a basic implementation of <see cref="IClassifierProgress{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the data to classify.</typeparam>
    public class ClassifierProgress<T>
        : IClassifierProgress<T>
    {
        private readonly Action<T, int> classifyingHandler = null;
        private readonly Action<T, int, Exception> classifiedHandler = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifierProgress{T}"/> class.
        /// </summary>
        public ClassifierProgress()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifierProgress{T}"/> class,
        /// using the specified callbacks.
        /// </summary>
        /// <param name="classifyingHandler">The handler to invoke when the data is about to be classified.</param>
        /// <param name="classifiedHandler">The handler to invoke when the data is classified.</param>
        public ClassifierProgress(
            Action<T, int> classifyingHandler,
            Action<T, int, Exception> classifiedHandler)
        {
            this.classifyingHandler = classifyingHandler;
            this.classifiedHandler = classifiedHandler;
        }

        /// <summary>
        /// Reports that the data is about to be classified.
        /// </summary>
        /// <param name="source">The data to classify.</param>
        /// <param name="index">The zero-based index of the data in the sequence.</param>
        public virtual void OnClassifying(T source, int index)
        {
            this.classifyingHandler?.Invoke(source, index);
        }

        /// <summary>
        /// Reports that the data is classified.
        /// </summary>
        /// <param name="source">The data to classify.</param>
        /// <param name="index">The zero-based index of the data in the sequence.</param>
        /// <param name="exception">The exception that might have occurred during classification.</param>
        public virtual void OnClassified(T source, int index, Exception exception)
        {
            this.classifiedHandler?.Invoke(source, index, exception);
        }
    }
}