// -----------------------------------------------------------------------
// <copyright file="ClassificationReport.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a classification test report.
    /// </summary>
    /// <typeparam name="T">The type of the classification answer.</typeparam>
    public class ClassificationReport<T>
    {
        private readonly List<ClassificationResult<T>> results = new List<ClassificationResult<T>>();
        private readonly List<ClassSummary<T>> classes = new List<ClassSummary<T>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationReport{T}"/> class.
        /// </summary>
        /// <param name="results">The classification results.</param>
        public ClassificationReport(IEnumerable<ClassificationResult<T>> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            Dictionary<T, ClassSummary<T>> summaries = new Dictionary<T, ClassSummary<T>>();

            foreach (ClassificationResult<T> result in results)
            {
                this.results.Add(result);

                T expected = result.Expected;
                if (!summaries.TryGetValue(expected, out ClassSummary<T> summary))
                {
                    summaries[expected] = summary = new ClassSummary<T>(expected);
                    this.classes.Add(summary);
                }

                summary.Add(result);

                this.AllClasses.Add(result);
                this.ConfusionMatrix.Add(result.Predicted, expected);
            }

            this.results.TrimExcess();
            this.classes.TrimExcess();
        }

        /// <summary>
        /// Gets the classification results.
        /// </summary>
        /// <value>
        /// A collection of classification results.
        /// </value>
        public ReadOnlyCollection<ClassificationResult<T>> Results
        {
            get
            {
                return new ReadOnlyCollection<ClassificationResult<T>>(this.results);
            }
        }

        /// <summary>
        /// Gets a collection of summary reports for classes in the test.
        /// </summary>
        /// <value>
        /// A collection of reports for each class.
        /// </value>
        public ReadOnlyCollection<ClassSummary<T>> Classes
        {
            get
            {
                return new ReadOnlyCollection<ClassSummary<T>>(this.classes);
            }
        }

        /// <summary>
        /// Gets a summary report for all classes in the test.
        /// </summary>
        /// <value>
        /// A combined report for all classes.
        /// </value>
        public ClassSummary<T> AllClasses { get; } = new ClassSummary<T>(default);

        /// <summary>
        /// Gets a confusion matrix.
        /// </summary>
        /// <value>
        /// A <see cref="ConfusionMatrix"/> object.
        /// </value>
        public ConfusionMatrix<T> ConfusionMatrix { get; } = new ConfusionMatrix<T>();
    }
}
