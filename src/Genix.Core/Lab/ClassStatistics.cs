// -----------------------------------------------------------------------
// <copyright file="ClassStatistics.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    /// <summary>
    /// Represents the aggregated classification statistics for a single class.
    /// </summary>
    public class ClassStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassStatistics"/> class.
        /// </summary>
        public ClassStatistics()
        {
        }

        /// <summary>
        /// Gets the <see cref="ClassificationStatistics"/> that contains data for all items in the set.
        /// </summary>
        /// <value>
        /// A <see cref="ClassificationStatistics"/> object that contains data for all items in the set.
        /// </value>
        public ClassificationStatistics All { get; } = new ClassificationStatistics();

        /// <summary>
        /// Gets the <see cref="ClassificationStatistics"/> that contains data only for those items in the set that have truth.
        /// </summary>
        /// <value>
        /// A <see cref="ClassificationStatistics"/> object that contains data only for those items in the set that have truth.
        /// </value>
        public ClassificationStatistics WithTruth { get; } = new ClassificationStatistics();

        /// <summary>
        /// Gets the <see cref="RejectCurve"/> that contains a reject curve data for all items in the set.
        /// </summary>
        /// <value>
        /// A <see cref="ClassificationStatistics"/> object that contains a reject curve data for all items in the set.
        /// </value>
        public RejectCurve RejectCurveAll { get; } = new RejectCurve();

        /// <summary>
        /// Gets the <see cref="RejectCurve"/> that contains a reject curve data only for items that have truth.
        /// </summary>
        /// <value>
        /// A <see cref="ClassificationStatistics"/> object that contains a reject curve data only for items that have truth.
        /// </value>
        public RejectCurve RejectCurveTruth { get; } = new RejectCurve();

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

            ClassStatistics other = obj as ClassStatistics;
            if (other == null)
            {
                return false;
            }

            return object.Equals(this.All, other.All) &&
                   object.Equals(this.WithTruth, other.WithTruth) &&
                   object.Equals(this.RejectCurveAll, other.RejectCurveAll) &&
                   object.Equals(this.RejectCurveTruth, other.RejectCurveTruth);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.All.GetHashCode() ^
                   this.WithTruth.GetHashCode() ^
                   this.RejectCurveAll.GetHashCode() ^
                   this.RejectCurveTruth.GetHashCode();
        }

        /// <summary>
        /// Adds a classification result to the statistics.
        /// </summary>
        /// <param name="confidence">The classification confidence.</param>
        /// <param name="isAccepted">Determines whether the classification's confidence is greater than or equal to cut-off value.</param>
        /// <param name="isValid">Determines whether the classification's result matches the truth data.</param>
        public void Add(float confidence, bool isAccepted, bool? isValid)
        {
            this.All.Add(isAccepted, isValid.GetValueOrDefault(false));
            this.RejectCurveAll.Add(confidence, isValid.GetValueOrDefault(true));

            if (isValid.HasValue)
            {
                this.WithTruth.Add(isAccepted, isValid.Value);
                this.RejectCurveTruth.Add(confidence, isValid.Value);
            }
        }

        /// <summary>
        /// Removes all data from the current <see cref="ClassStatistics"/>.
        /// </summary>
        public void Clear()
        {
            this.All.Clear();
            this.WithTruth.Clear();
            this.RejectCurveAll.Clear();
            this.RejectCurveTruth.Clear();
        }
    }
}
