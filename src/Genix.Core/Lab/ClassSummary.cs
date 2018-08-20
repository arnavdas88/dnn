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
    /// <typeparam name="T">The type of the classification answer.</typeparam>
    public class ClassSummary<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassSummary{T}"/> class.
        /// </summary>
        /// <param name="label">The label this summary is for.</param>
        public ClassSummary(T label)
        {
            this.Label = label;
        }

        /// <summary>
        /// Gets a name of the label this summary is for.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the label.
        /// </value>
        public T Label { get; }

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
        public IList<ClassificationResult<T>> Errors { get; } = new List<ClassificationResult<T>>();

        /// <summary>
        /// Adds the classification result to the class summary.
        /// </summary>
        /// <param name="resut">The classification result.</param>
        public void Add(ClassificationResult<T> resut)
        {
            if (resut == null)
            {
                throw new ArgumentNullException(nameof(resut));
            }

            // compare result with truth
            bool? isValid = object.Equals(resut.Expected, default(T)) ? null : (bool?)object.Equals(resut.Expected, resut.Predicted);

            // add to statistics
            this.Statistics.Add(resut.Confidence, resut.IsAccepted, isValid);

            // create an error record
            if (isValid.HasValue && !isValid.Value)
            {
                this.Errors.Add(new ClassificationResult<T>(resut));
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is ClassSummary<T> other)
            {
                return object.Equals(this.Label, other.Label) &&
                       this.Statistics.Equals(other.Statistics) &&
                       this.Errors.Count == other.Errors.Count &&
                       Enumerable.SequenceEqual(this.Errors, other.Errors);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Label?.GetHashCode() ?? 0;
    }
}
