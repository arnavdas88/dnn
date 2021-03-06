﻿// -----------------------------------------------------------------------
// <copyright file="RejectCurve.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The reject curve the describes the dependency of number of valid and erroneous items from the threshold.
    /// </summary>
    public sealed class RejectCurve
    {
        /// <summary>
        /// The points of the reject curve.
        /// </summary>
        private readonly RejectCurvePoint[] points = new RejectCurvePoint[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="RejectCurve"/> class.
        /// </summary>
        public RejectCurve()
            : this(1.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RejectCurve"/> class, using the specified maximum confidence value.
        /// </summary>
        /// <param name="maxConfidence">The maximum allowed confidence value.</param>
        public RejectCurve(float maxConfidence)
        {
            this.MaxConfidence = maxConfidence;

            this.points = new RejectCurvePoint[(int)Math.Ceiling(100.0f * this.MaxConfidence) + 1];
            for (int i = 0, ii = this.points.Length; i < ii; i++)
            {
                this.points[i] = new RejectCurvePoint(i);
            }
        }

        /// <summary>
        /// Gets the number of items in the reject curve.
        /// </summary>
        /// <value>
        /// An integer that represents the number of items in the reject curve.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the maximum allowed confidence value.
        /// </summary>
        /// <value>
        /// An integer that represents the maximum allowed confidence value. The default value is 1.0f.
        /// </value>
        public float MaxConfidence { get; private set; }

        /// <summary>
        /// Gets the collection of the <see cref="RejectCurvePoint"/> in the curve.
        /// </summary>
        /// <value>The collection of <see cref="RejectCurvePoint"/>.</value>
        public IReadOnlyCollection<RejectCurvePoint> Points => this.points;

        /// <summary>
        /// Gets the <see cref="RejectCurvePoint"/> associated with the specified threshold.
        /// </summary>
        /// <param name="threshold">The point on the curve to get the <see cref="RejectCurvePoint"/> for.</param>
        /// <returns>The <see cref="RejectCurvePoint"/>.</returns>
        public RejectCurvePoint this[int threshold] => this.points[threshold];

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            RejectCurve other = obj as RejectCurve;
            if (other == null)
            {
                return false;
            }

            return this.points.Length == other.points.Length &&
                Enumerable.SequenceEqual(this.points, other.points);
        }

        /// <inheritdoc />
        public override int GetHashCode() => 0;

        /// <summary>
        /// Removes all items from the reject curve.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < this.points.Length; i++)
            {
                this.points[i] = new RejectCurvePoint(i);
            }

            this.Count = 0;
        }

        /// <summary>
        /// Adds a new item to the reject curve.
        /// </summary>
        /// <param name="confidence">An item's confidence value.</param>
        /// <param name="isValid">Determines whether the item matches truth data.</param>
        public void Add(float confidence, bool isValid)
        {
            confidence = confidence.Clip(0, this.MaxConfidence);

            this.Count++;

            for (int threshold = 0; threshold < this.points.Length; threshold++)
            {
                this.points[threshold].Add(confidence >= threshold, isValid);
            }
        }

        /// <summary>
        /// Finds the point on the reject curve associated with the specified target error rate.
        /// </summary>
        /// <param name="target">The target error rate.</param>
        /// <returns>The <see cref="RejectCurveTarget"/> object.</returns>
        /// <remarks>The <b>Point</b> property of returned <see cref="RejectCurveTarget"/> object sets to <b>null</b> when the target error rate cannot be reached.</remarks>
        public RejectCurveTarget GetTarget(float target)
        {
            int threshold = Array.FindIndex(this.points, x => x.ErrorRate <= target);

            return new RejectCurveTarget(target, threshold == -1 ? (RejectCurvePoint?)null : this.points[threshold]);
        }

        /// <summary>
        /// Finds the points on the reject curve associated with the specified target error rates.
        /// </summary>
        /// <param name="targets">The collection target error rates.</param>
        /// <returns>The collection of <see cref="RejectCurveTarget"/> objects.</returns>
        /// <remarks>The <b>Point</b> property of returned <see cref="RejectCurveTarget"/> objects sets to <b>null</b> when the target error rates cannot be reached.</remarks>
        /// <exception cref="ArgumentNullException"><c>targets</c> is <b>null</b>.</exception>
        public RejectCurveTarget[] GetTargets(params float[] targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return targets.Select(x => this.GetTarget(x)).ToArray();
        }

        /// <summary>
        /// Gets the area under the curve that has error rate as its x-axis and accept rate as its y-axis.
        /// </summary>
        /// <param name="errorRateStart">The starting error rate.</param>
        /// <param name="errorRateEnd">The ending error rate.</param>
        /// <returns>The area under the curve.</returns>
        public float GetArea(float errorRateStart, float errorRateEnd) => this.GetArea(errorRateStart, errorRateEnd, 10);

        /// <summary>
        /// Gets the area under the curve that has error rate as its x-axis and accept rate as its y-axis.
        /// </summary>
        /// <param name="errorRateStart">The starting error rate.</param>
        /// <param name="errorRateEnd">The ending error rate.</param>
        /// <param name="intervalCount">The number discrete intervals.</param>
        /// <returns>The area under the curve.</returns>
        public float GetArea(float errorRateStart, float errorRateEnd, int intervalCount)
        {
            if (!intervalCount.Between(1, 100))
            {
                throw new ArgumentOutOfRangeException(nameof(intervalCount));
            }

            float step = (errorRateEnd - errorRateStart) / intervalCount;
            float result = 0.0f;

            // initialize error rates
            checked
            {
                float[] errorRates = new float[intervalCount + 1];
                for (int i = 0; i < errorRates.Length; i++)
                {
                    errorRates[i] = errorRateStart + (step * i);
                }

                errorRates[errorRates.Length - 1] = errorRateEnd;

                // get targets
                RejectCurveTarget[] targets = this.GetTargets(errorRates);

                // calculate area
                float acceptRate1 = targets[0].Point.HasValue ? targets[0].Point.Value.AcceptRate : 0.0f;
                for (int i = 1; i < targets.Length; i++)
                {
                    float acceptRate2 = targets[i].Point.HasValue ? targets[i].Point.Value.AcceptRate : 0.0f;
                    result += ((acceptRate1 + acceptRate2) / 2) * step;
                    acceptRate1 = acceptRate2;
                }
            }

            return result / (errorRateEnd - errorRateStart);
        }
    }
}
