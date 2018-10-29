// -----------------------------------------------------------------------
// <copyright file="RejectCurveTarget.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents the target error rate and the point on the reject curve that corresponds to the target error rate.
    /// </summary>
    public struct RejectCurveTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RejectCurveTarget"/> struct, using the specified target error rate and the <see cref="RejectCurvePoint"/>.
        /// </summary>
        /// <param name="target">The target error rate.</param>
        /// <param name="point">The <see cref="RejectCurvePoint"/> associated with the specified target error rate.</param>
        public RejectCurveTarget(float target, RejectCurvePoint? point)
            : this()
        {
            this.Target = target;
            this.Point = point;
        }

        /// <summary>
        /// Gets the target error rate.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> structure that represents the error rate.
        /// </value>
        public float Target { get; }

        /// <summary>
        /// Gets the point on the reject curve associated with the target error rate.
        /// </summary>
        /// <value>
        /// A point on the reject curve associated with the target error rate; <b>null</b> if such point does not exist and the target error rate cannot be achieved.
        /// </value>
        public RejectCurvePoint? Point { get; }

        /// <summary>
        /// Compares two <see cref="RejectCurveTarget"/> objects. The result specifies whether the properties specified by the two <see cref="RejectCurveTarget"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="RejectCurveTarget"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="RejectCurveTarget"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the two <see cref="RejectCurveTarget"/> structures have equal properties; otherwise, <b>false</b>.</returns>
        public static bool operator ==(RejectCurveTarget left, RejectCurveTarget right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="RejectCurveTarget"/> objects. The result specifies whether the properties specified by the two <see cref="RejectCurveTarget"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="RejectCurveTarget"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="RejectCurveTarget"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the two <see cref="RejectCurveTarget"/> structures have unequal properties; otherwise, <b>false</b>.</returns>
        public static bool operator !=(RejectCurveTarget left, RejectCurveTarget right) => !left.Equals(right);

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

            if (!(obj is RejectCurveTarget))
            {
                return false;
            }

            RejectCurveTarget other = (RejectCurveTarget)obj;
            return this.Target == other.Target &&
                   this.Point == other.Point;
        }

        /// <inheritdoc />
        public override int GetHashCode() => (this.Target * 100.0f).Round();

        /// <inheritdoc />
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0:F4} - {1}",
                this.Target,
                this.Point.GetValueOrDefault());
    }
}
