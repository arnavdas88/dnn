// -----------------------------------------------------------------------
// <copyright file="ClassificationStatistics.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Represents the classification statistics.
    /// </summary>
    public class ClassificationStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationStatistics"/> class.
        /// </summary>
        public ClassificationStatistics()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationStatistics"/> class, aggregating data from a sequence of <see cref="ClassificationStatistics"/> objects.
        /// </summary>
        /// <param name="statistics">A sequence of <see cref="ClassificationStatistics"/> objects to aggregate data from.</param>
        public ClassificationStatistics(IEnumerable<ClassificationStatistics> statistics)
        {
            this.Count = statistics.Sum(x => x.Count);
            this.Valid = statistics.Sum(x => x.Valid);
            this.Accepted = statistics.Sum(x => x.Accepted);
            this.Errors = statistics.Sum(x => x.Errors);
        }

        /// <summary>
        /// Gets the total number of items that were classified.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that contains the total number of items that were classified.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the number of items that were classified correctly.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that contains the number of items that were classified correctly.
        /// </value>
        public int Valid { get; private set; }

        /// <summary>
        /// Gets the number of items that were announced as correct.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that contains the number of items that were announced as correct.
        /// </value>
        public int Accepted { get; private set; }

        /// <summary>
        /// Gets the number of items that were announced as correct but were incorrect.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that contains the number of items that were announced as correct but were incorrect.
        /// </value>
        public int Errors { get; private set; }

        /// <summary>
        /// Gets the percentage of items that were classified correctly.
        /// </summary>
        /// <value><see cref="Valid"/> divided by <see cref="Count"/>; zero if <see cref="Count"/> is zero.</value>
        public float ValidRate
        {
            get { return this.Count > 0 ? (float)this.Valid / this.Count : 0.0f; }
        }

        /// <summary>
        /// Gets the percentage of items that were announced as correct.
        /// </summary>
        /// <value><see cref="Accepted"/> divided by <see cref="Count"/>; zero if <see cref="Count"/> is zero.</value>
        public float AcceptRate
        {
            get { return this.Count > 0 ? (float)this.Accepted / this.Count : 0.0f; }
        }

        /// <summary>
        /// Gets the percentage items that were announced as correct but were incorrect.
        /// </summary>
        /// <value><see cref="Errors"/> divided by <see cref="Accepted"/>; zero if <see cref="Accepted"/> is zero.</value>
        public float ErrorRate
        {
            get { return this.Accepted > 0 ? (float)this.Errors / this.Accepted : 0.0f; }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            ClassificationStatistics other = obj as ClassificationStatistics;
            if (other == null)
            {
                return false;
            }

            return this.Count == other.Count &&
                   this.Valid == other.Valid &&
                   this.Accepted == other.Accepted &&
                   this.Errors == other.Errors;
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Count ^ this.Valid ^ this.Accepted ^ this.Errors;

        /// <inheritdoc />
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "Count : {0}, Valid : {1}, Accepted : {2}, Errors : {3}",
                this.Count,
                this.Valid,
                this.Accepted,
                this.Errors);

        /// <summary>
        /// Adds an item result to the statistics.
        /// </summary>
        /// <param name="isAccepted">Determines whether the item was announced as correct.</param>
        /// <param name="isValid">Determines whether the item was recognized correctly.</param>
        public void Add(bool isAccepted, bool isValid)
        {
            this.Count++;

            if (isValid)
            {
                this.Valid++;
            }

            if (isAccepted)
            {
                this.Accepted++;
                if (!isValid)
                {
                    this.Errors++;
                }
            }
        }

        /// <summary>
        /// Adds item results to the statistics.
        /// </summary>
        /// <param name="isAccepted">Determines whether the item were announced as correct.</param>
        /// <param name="count">A number of items to add to the statistics.</param>
        /// <param name="validCount">A number of correct items in <c>count</c>.</param>
        public void Add(bool isAccepted, int count, int validCount)
        {
            this.Count += count;
            this.Valid += validCount;

            if (isAccepted)
            {
                this.Accepted += count;
                this.Errors += count - validCount;
            }
        }

        /// <summary>
        /// Removes all data from the current <see cref="ClassificationStatistics"/>.
        /// </summary>
        public void Clear()
        {
            this.Count = this.Valid = this.Accepted = this.Errors = 0;
        }
    }
}
