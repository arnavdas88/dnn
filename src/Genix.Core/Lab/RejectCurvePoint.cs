// -----------------------------------------------------------------------
// <copyright file="RejectCurvePoint.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a point on the reject curve.
    /// </summary>
    public struct RejectCurvePoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RejectCurvePoint"/> struct, using the specified threshold.
        /// </summary>
        /// <param name="threshold">The threshold value.</param>
        public RejectCurvePoint(int threshold)
            : this()
        {
            this.Threshold = threshold;
        }

        /// <summary>
        /// Gets the confidence threshold associated with this <see cref="RejectCurvePoint"/>.
        /// </summary>
        /// <value>
        /// An integer that represents the confidence threshold.
        /// </value>
        public int Threshold { get; private set; }

        /// <summary>
        /// Gets the number of items in the reject curve.
        /// </summary>
        /// <value>
        /// An integer that represents the number of items in the reject curve.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the number of accepted items.
        /// </summary>
        /// <value>
        /// An integer that represents the number of accepted items.
        /// </value>
        /// <remarks>
        /// Accepted items have their confidence greater than or equal than the threshold.
        /// </remarks>
        public int AcceptCount { get; private set; }

        /// <summary>
        /// Gets the number of erroneous items.
        /// </summary>
        /// <value>
        /// An integer that represents the number of erroneous items.
        /// </value>
        /// <remarks>
        /// Erroneous items do not match with their truth information.
        /// </remarks>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Gets the accept rate.
        /// </summary>
        /// <value>
        /// A <see cref="double"/> structure that represents the accept rate.
        /// </value>
        /// <remarks>
        /// The accept rate if the <see cref="AcceptCount"/> divided by the total number of items in the curve.
        /// </remarks>
        public double AcceptRate => this.Count <= 0 ? 0.0 : (double)this.AcceptCount / this.Count;

        /// <summary>
        /// Gets the error rate.
        /// </summary>
        /// <value>
        /// A <see cref="double"/> structure that represents the error rate.
        /// </value>
        /// <remarks>
        /// The error rate if the <see cref="ErrorCount"/> divided by the <see cref="AcceptCount"/>.
        /// </remarks>
        public double ErrorRate => this.AcceptCount <= 0 ? 0.0 : (double)this.ErrorCount / this.AcceptCount;

        /// <summary>
        /// Compares two <see cref="RejectCurvePoint"/> objects. The result specifies whether the properties specified by the two <see cref="RejectCurvePoint"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="RejectCurvePoint"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="RejectCurvePoint"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the two <see cref="RejectCurvePoint"/> structures have equal properties; otherwise, <b>false</b>.</returns>
        public static bool operator ==(RejectCurvePoint left, RejectCurvePoint right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="RejectCurvePoint"/> objects. The result specifies whether the properties specified by the two <see cref="RejectCurvePoint"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="RejectCurvePoint"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="RejectCurvePoint"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the two <see cref="RejectCurvePoint"/> structures have unequal properties; otherwise, <b>false</b>.</returns>
        public static bool operator !=(RejectCurvePoint left, RejectCurvePoint right) => !left.Equals(right);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is RejectCurvePoint))
            {
                return false;
            }

            RejectCurvePoint other = (RejectCurvePoint)obj;
            return this.Threshold == other.Threshold &&
                   this.Count == other.Count &&
                   this.AcceptCount == other.AcceptCount &&
                   this.ErrorCount == other.ErrorCount;
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Threshold;

        /// <inheritdoc />
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} : {1:P2} : {2:P2}",
                this.Threshold,
                this.AcceptRate,
                this.ErrorRate);

        /// <summary>
        /// Adds a result of comparison to the point on the reject curve.
        /// </summary>
        /// <param name="isAccepted"><b>true</b> if the result was accepted; otherwise, <b>false</b>.</param>
        /// <param name="isValid"><b>true</b> if the result has matched with the truth; otherwise, <b>false</b>.</param>
        internal void Add(bool isAccepted, bool isValid)
        {
            this.Count++;

            if (isAccepted)
            {
                this.AcceptCount++;
            }

            if (isAccepted && !isValid)
            {
                this.ErrorCount++;
            }
        }
    }
}
