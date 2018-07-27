// -----------------------------------------------------------------------
// <copyright file="ClassSummary.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a classification summary report for a single class.
    /// </summary>
    public class ClassSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassSummary"/> class.
        /// </summary>
        /// <param name="className">The class name this summary is for.</param>
        public ClassSummary(string className)
        {
            this.ClassName = className;
        }

        /// <summary>
        /// Gets a name of the class this summary is for.
        /// </summary>
        /// <value>
        /// A <see cref="String"/> that contains the class name.
        /// </value>
        public string ClassName { get; }

        /// <summary>
        /// Gets a classification statistics for the class.
        /// </summary>
        /// <value>
        /// A <see cref="ClassStatistics"/> object.
        /// </value>
        public ClassStatistics Statistics { get; } = new ClassStatistics();

        /// <summary>
        /// Gets a collection of errors for the class.
        /// </summary>
        /// <value>
        /// A <see cref="IList{ClassificationResult}"/> object.
        /// </value>
        public IList<ClassificationResult> Errors { get; } = new List<ClassificationResult>();

        /// <summary>
        /// Adds the classification result to the class summary.
        /// </summary>
        /// <param name="answer">The classification result.</param>
        public void Add(ClassificationResult answer)
        {
            if (answer == null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            // compare result with truth
            bool? isValid = string.IsNullOrEmpty(answer.Expected) ? null : (bool?)(answer.Expected == answer.Predicted);

            // add to statistics
            this.Statistics.Add(answer.Confidence, answer.IsAccepted, isValid);

            // create an error record
            if (isValid.HasValue && !isValid.Value)
            {
                this.Errors.Add(new ClassificationResult(answer));
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ClassSummary other = obj as ClassSummary;
            if (other == null)
            {
                return false;
            }

            return object.Equals(this.ClassName, other.ClassName) &&
                   this.Statistics.Equals(other.Statistics) &&
                   this.Errors.Count == other.Errors.Count &&
                   Enumerable.SequenceEqual(this.Errors, other.Errors);
        }

        /// <inheritdoc />
        public override int GetHashCode() => (this.ClassName ?? string.Empty).GetHashCode();
    }
}
