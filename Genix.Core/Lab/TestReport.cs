// -----------------------------------------------------------------------
// <copyright file="TestReport.cs" company="Noname, Inc.">
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
    public class TestReport
    {
        private readonly List<ClassificationResult> results = new List<ClassificationResult>();
        private readonly List<ClassSummary> classes = new List<ClassSummary>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReport"/> class.
        /// </summary>
        /// <param name="results">The classification results.</param>
        public TestReport(IEnumerable<ClassificationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            Dictionary<string, ClassSummary> summaries = new Dictionary<string, ClassSummary>();

            foreach (ClassificationResult result in results)
            {
                this.results.Add(result);

                string truth = result.Expected ?? string.Empty;

                if (!summaries.TryGetValue(truth, out ClassSummary summary))
                {
                    summary = new ClassSummary(truth);

                    summaries.Add(truth, summary);
                    this.classes.Add(summary);
                }

                summary.Add(result);

                this.AllClasses.Add(result);

                this.ConfusionMatrix.Add(result.Predicted, truth);
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
        public ReadOnlyCollection<ClassificationResult> Results
        {
            get
            {
                return new ReadOnlyCollection<ClassificationResult>(this.results);
            }
        }

        /// <summary>
        /// Gets a collection of summary reports for classes in the test.
        /// </summary>
        /// <value>
        /// A collection of reports for each class.
        /// </value>
        public ReadOnlyCollection<ClassSummary> Classes
        {
            get
            {
                return new ReadOnlyCollection<ClassSummary>(this.classes);
            }
        }

        /// <summary>
        /// Gets a summary report for all classes in the test.
        /// </summary>
        /// <value>
        /// A combined report for all classes.
        /// </value>
        public ClassSummary AllClasses { get; } = new ClassSummary("All");

        /// <summary>
        /// Gets a confusion matrix.
        /// </summary>
        /// <value>
        /// A <see cref="ConfusionMatrix"/> object.
        /// </value>
        public ConfusionMatrix ConfusionMatrix { get; } = new ConfusionMatrix();
    }
}
