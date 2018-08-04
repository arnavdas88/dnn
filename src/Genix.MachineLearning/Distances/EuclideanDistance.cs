﻿// -----------------------------------------------------------------------
// <copyright file="EuclideanDistance.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Distances
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Measures the Euclidean distance between two points.
    /// </summary>
    public struct EuclideanDistance
        : IDistance<float, float>, IDistance<double, double>, IDistance<float[], float>, IDistance<double[], double>
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(float x, float y)
        {
            return Math.Abs(x - y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(double x, double y)
        {
            return Math.Abs(x - y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(float[] x, float[] y)
        {
            return Mathematics.EuclideanDistance(x.Length, x, 0, y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(double[] x, double[] y)
        {
            return Mathematics.EuclideanDistance(x.Length, x, 0, y, 0);
        }
    }
}
